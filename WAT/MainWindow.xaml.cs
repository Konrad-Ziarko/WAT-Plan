using System;
using System.Windows;
using FirstFloor.ModernUI.Presentation;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Text;

namespace WAT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string login = Encryptor.Decrypt(Properties.Settings.Default.Login);
        private static string pass = Encryptor.Decrypt(Properties.Settings.Default.Pass);
        private static string groupNo = Encryptor.Decrypt(Properties.Settings.Default.Group);
        public static System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        public static System.Windows.Forms.WebBrowser webB = new System.Windows.Forms.WebBrowser();
        public static System.Windows.Forms.Form frm = new Form();
        private string envPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WAT - Plan";
        private XmlSerializer serializer = new XmlSerializer(typeof(List<SingleEvent>));
        private List<SingleEvent> schedule;
        public MainWindow()
        {
            InitializeComponent();
            ni.Icon = Properties.Resources.KZ;
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            //
            frm.Visible = true;
            //
            frm.Width = 500;
            frm.Height = 500;
            webB.ScriptErrorsSuppressed = true;
            frm.Controls.Add(webB);
            webB.Dock = DockStyle.Fill;
            //load schedule
            schedule = readScheduleFromXMLFile(envPath, "default.xml");
            //
        }

        #region XMLParser
        public List<SingleEvent> readScheduleFromXMLFile(string filePath, string fileName)
        {
            try
            {
                using (FileStream stream = File.OpenRead(filePath + "\\" + fileName))
                {
                    List<SingleEvent> dezerializedList = (List<SingleEvent>)serializer.Deserialize(stream);
                    return dezerializedList;
                }
            }
            catch { return new List<SingleEvent>(); }
        }
        public bool writeScheduleToXMLFile(string filePath, string fileName, List<SingleEvent> list)
        {
            if (list == null)
                return false;
            try
            {
                Directory.CreateDirectory(filePath);
                using (FileStream stream = File.Open(filePath + "\\" + fileName, FileMode.Create))
                {
                    serializer.Serialize(stream, list);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #region TitleBar buttons
        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            ni.Dispose();
            Close();
        }

        private void button_Max_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void button_Min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            this.Hide();
            ni.ShowBalloonTip(5000, "WAT", "Aplikacja będzie działać w tle.", System.Windows.Forms.ToolTipIcon.Info);
        }
        #endregion

        public async void button_ShowChanges_Click(object sender, RoutedEventArgs e)
        {
            webB.Navigate("https://s1.wcy.wat.edu.pl/ed/");
            //
            await PageLoad(10);
            //
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input"))
            {
                string comp = el.GetAttribute("name");
                if (comp.Equals("userid"))
                {
                    el.SetAttribute("value", login);
                    break;
                }
            }
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input"))
            {
                string comp = el.GetAttribute("name");
                if (comp.Equals("password"))
                {
                    el.SetAttribute("value", pass);
                    break;
                }
            }
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input"))
            {
                string comp = el.GetAttribute("value");
                if (comp.Equals(" Zaloguj się "))
                {
                    el.InvokeMember("click");
                    break;
                }
            }
            //
            await PageLoad(10);
            //
            string[] parts2 = webB.Url.AbsoluteUri.Split('?');
            string tmp = parts2[1];
            if (parts2.Length > 2)
                for (int i = 1; i < parts2.Length; i++)
                    tmp += "?" + parts2[i];

            string[] args = tmp.Split('&');
            string str;
            if (Properties.Settings.Default.Season == 0)
            {
                str = "2";
            }
            else
            {
                str = "1";
            }
            int year = DateTime.Now.Year - 1;
            if (DateTime.Now.Month > 7)
                year++;
            string URl = "https://s1.wcy.wat.edu.pl/ed/logged_inc.php?" + args[0] + "&mid=328&iid=" + year + str + "&exv=" + Encryptor.Decrypt(Properties.Settings.Default.Group) + "&pos=0&rdo=1&" + args[args.Length - 1];
            webB.Navigate(URl);
            //
            await PageLoad(10);
            //
            webB.Document.InvokeScript("showGroupPlan", new string[] { Encryptor.Decrypt(Properties.Settings.Default.Group) });
            //
            await PageLoad(10);
            //
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            Uri uri = new Uri("https://s1.wcy.wat.edu.pl/ed/" + webB.Document.InvokeScript("prepareURL") + "DTXT");
            Directory.CreateDirectory(envPath);
            webClient.DownloadFileAsync(uri, envPath + "\\tmp");
            //
            string plan = File.ReadAllText(envPath + "\\tmp");
            string[] lines = plan.Split('\n');
            List<SingleEvent> allEvents = new List<SingleEvent>();
            foreach (string line in lines)
            {
                try
                {
                    string[] parts = line.Split(',');
                    if (parts.Length < 6)
                        continue;
                    SingleEvent ev = new SingleEvent(parts[0], parts[1], parts[2] + " " + parts[3], parts[4] + " " + parts[5]);
                    allEvents.Add(ev);
                }
                catch { }
                ;
            }
            //
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            ;
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ;
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            var frm = new SettingsWindow();
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
            login = Encryptor.Decrypt(Properties.Settings.Default.Login);
            pass = Encryptor.Decrypt(Properties.Settings.Default.Pass);
            groupNo = Encryptor.Decrypt(Properties.Settings.Default.Group);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.Remember)
            {
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Login = "";
                Properties.Settings.Default.Pass = "";
                Properties.Settings.Default.Group = "";
                Properties.Settings.Default.Save();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string plan = File.ReadAllText(envPath + "\\tmp", Encoding.Default);
            string[] lines = plan.Split('\n');
            List<SingleEvent> allEvents = new List<SingleEvent>();
            foreach (string line in lines)
            {
                try
                {
                    string[] parts = line.Split(',');
                    if (parts.Length < 6)
                        continue;
                    SingleEvent ev = new SingleEvent(parts[0], parts[1], parts[2] + " " + parts[3], parts[4] + " " + parts[5]);
                    allEvents.Add(ev);
                }
                catch { }
                ;
            }
            //
            bool hasChanged = false;
            if (schedule.Count == allEvents.Count)
                for (int i = 0; i < schedule.Count; i++)
                {
                    if (!schedule[i].IsEqualTo(allEvents[i]))
                        hasChanged = true;
                }
            else
                hasChanged = true;

            //wyświetlić nowy plan zajec
            //zapytac czy zaktualizowac
            if (hasChanged)
                if (System.Windows.Forms.DialogResult.Yes == System.Windows.Forms.MessageBox.Show("Czy chesz zaktualizować plan zajęć?", "Wykryto zmiany w planie zajęć", MessageBoxButtons.YesNo))
                {
                    schedule = allEvents;
                    writeScheduleToXMLFile(envPath, "default.xml", schedule);

                }
            //

            //writeScheduleToXMLFile(envPath, "tmp", new List<SingleEvent>());
            //WebClient webClient = new WebClient();
            //webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Completed);
            //webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            //Uri uri = new Uri("https://s1.wcy.wat.edu.pl/ed/logged.php?sid=db0bb722ae892f8cea5917cc5e25cc32&mid=328&iid=20165&vrf=32820165&rdo=1&pos=0&exv=I6G2S4&opr=DTXT");
            //webClient.DownloadFileAsync(uri, envPath + "\\tmp");
        }

        private async Task PageLoad(int TimeOut)
        {
            TaskCompletionSource<bool> PageLoaded = null;
            PageLoaded = new TaskCompletionSource<bool>();
            int TimeElapsed = 0;
            webB.DocumentCompleted += (s, e) =>
            {
                if (webB.ReadyState != WebBrowserReadyState.Complete)
                    return;
                if (PageLoaded.Task.IsCompleted)
                    return;
                PageLoaded.SetResult(true);
            };
            while (PageLoaded.Task.Status != TaskStatus.RanToCompletion)
            {
                await Task.Delay(10);
                TimeElapsed++;
                if (TimeElapsed >= TimeOut * 100)
                    PageLoaded.TrySetResult(true);
            }
        }
    }
}
