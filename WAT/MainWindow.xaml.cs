using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Text;
using QuickWPFMonthCalendar;
using System.Drawing;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace WAT {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static string login = Encryptor.Decrypt(Properties.Settings.Default.Login);
        private static string pass = Encryptor.Decrypt(Properties.Settings.Default.Pass);
        private static string groupNo = Encryptor.Decrypt(Properties.Settings.Default.Group);

        private static SemaphoreSlim groupSemaphore = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim teacherSemaphore = new SemaphoreSlim(1, 1);


        public static NotifyIcon ni;
        public static MyWebBrowser webB = new MyWebBrowser();
        public static Form frm = new Form();
        private string envPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WAT - Plan";
        private XmlSerializer serializer = new XmlSerializer(typeof(List<Appointment.Appointment>));
        private List<Appointment.Appointment> schedule;

        private ComparePlans comparePlans;

        public MainWindow() {
            InitializeComponent();
            textBox.Visibility = Visibility.Hidden;

            ni = new NotifyIcon {
                Icon = Properties.Resources.KZ,
                Visible = true
            };
            ni.DoubleClick +=
                delegate (object sender, EventArgs args) {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };

            //
            //frm.Visible = true;
            //
            frm.Width = 500;
            frm.Height = 500;
            frm.Controls.Add(webB);

            webB.ScriptErrorsSuppressed = true;
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            webB.Dock = DockStyle.Fill;
            webB.Navigate("about:blank");

            //load schedule
            schedule = readScheduleFromXMLFile(envPath, "default.xml");
            ///
            SetAppointments();
            Calendar.DayBoxDoubleClicked += DayBoxDoubleClicked_event;
            Calendar.AppointmentDblClicked += AppointmentDblClicked;
            Calendar.DisplayMonthChanged += DisplayMonthChanged;
            //

            //
        }
        
        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors) {
            return policyErrors == SslPolicyErrors.None;
        }
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            return true;
        }
        

        private void SetAppointments() {
            Calendar.MonthAppointments = schedule.FindAll(new System.Predicate<Appointment.Appointment>((Appointment.Appointment apt) => apt.StartTime != null && Convert.ToDateTime(apt.StartTime).Month == this.Calendar.DisplayStartDate.Month && Convert.ToDateTime(apt.StartTime).Year == this.Calendar.DisplayStartDate.Year));
        }
        private void DayBoxDoubleClicked_event(NewAppointmentEventArgs e) {
            string allEventsInThatDay = "";
            foreach (var a in schedule) {
                if (a.StartTime > e.StartDate && a.EndTime < e.EndDate) {
                    allEventsInThatDay += a.Details + "\n";
                }
            }

            System.Windows.Forms.MessageBox.Show(allEventsInThatDay, "Calendar Event", System.Windows.Forms.MessageBoxButtons.OK);
        }

        private void AppointmentDblClicked(int Appointment_Id) {
            foreach (var a in schedule) {
                if (a.AppointmentID == Appointment_Id) {
                    System.Windows.Forms.MessageBox.Show(a.Details + "\n" + a.StartTime + "\n" + a.EndTime + "\n" + a.Location, "", System.Windows.Forms.MessageBoxButtons.OK);
                    TextBox tx1 = new TextBox();
                    TextBox tx2 = new TextBox();
                    TextBox tx3 = new TextBox();
                    using (Form frm = new Form()) {
                        tx1.Text = a.BGR.ToString();

                        var loc = tx1.Location;
                        loc.X = 20;
                        loc.Y = 20;
                        tx1.Location = loc;
                        loc.Y += 20;
                        tx2.Text = a.BGG.ToString();
                        tx2.Location = loc;
                        loc.Y += 20;
                        tx3.Text = a.BGB.ToString();

                        tx3.Location = loc;

                        frm.Controls.Add(tx1);
                        frm.Controls.Add(tx2);
                        frm.Controls.Add(tx3);
                        frm.ShowDialog();
                    }
                    foreach (var b in schedule) {
                        if (b._Short == a._Short) {
                            b.BGR = Convert.ToInt32(tx1.Text);
                            b.BGG = Convert.ToInt32(tx2.Text);
                            b.BGB = Convert.ToInt32(tx3.Text);
                        }

                    }
                    SetAppointments();
                    break;
                }
            }
        }

        private void DisplayMonthChanged(MonthChangedEventArgs e) {
            SetAppointments();
        }
        #region XMLParser
        public List<Appointment.Appointment> readScheduleFromXMLFile(string filePath, string fileName) {
            try {
                using (FileStream stream = File.OpenRead(filePath + "\\" + fileName)) {
                    List<Appointment.Appointment> dezerializedList = (List<Appointment.Appointment>)serializer.Deserialize(stream);
                    return dezerializedList;
                }
            }
            catch { return new List<Appointment.Appointment>(); }
        }
        public bool writeScheduleToXMLFile(string filePath, string fileName, List<Appointment.Appointment> list) {
            if (list == null)
                return false;
            try {
                Directory.CreateDirectory(filePath);
                using (FileStream stream = File.Open(filePath + "\\" + fileName, FileMode.Create)) {
                    serializer.Serialize(stream, list);
                }
                return true;
            }
            catch {
                return false;
            }
        }
        #endregion
        #region TitleBar buttons
        private void button_Exit_Click(object sender, RoutedEventArgs e) {
            if (ni != null)
                ni.Dispose();
            Close();
        }

        private void button_Max_Click(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void button_Min_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
            this.Hide();
            ni.ShowBalloonTip(5000, "WAT", "Aplikacja będzie działać w tle.", System.Windows.Forms.ToolTipIcon.Info);
        }
        #endregion

        private async void downloadGroupPlan(string groupNumber, int seasonNum, DateTime when, AsyncCompletedEventHandler asyncCompletedEventHandler, DownloadProgressChangedEventHandler downloadProgressChangedEventHandler, string tmpFileName) {
            await groupSemaphore.WaitAsync();

            textBox.Visibility = Visibility.Visible;
            progress.Value = 0;
            webB.Navigate("https://s1.wcy.wat.edu.pl/ed/");
            //
            await PageLoad(10);
            //
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("name");
                if (comp.Equals("userid")) {
                    el.SetAttribute("value", login);
                    break;
                }
            }
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("name");
                if (comp.Equals("password")) {
                    el.SetAttribute("value", pass);
                    break;
                }
            }
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("value");
                if (comp.Equals(" Zaloguj się ")) {
                    el.InvokeMember("click");
                    break;
                }
            }
            //
            progress.Value = 15;
            await PageLoad(10);
            //

            try {
                string[] parts2 = webB.Url.AbsoluteUri.Split('?');
                string tmp = parts2[1];
                if (parts2.Length > 2)
                    for (int i = 1; i < parts2.Length; i++)
                        tmp += "?" + parts2[i];

                string[] args = tmp.Split('&');
                string str;
                if (seasonNum == 0) {
                    str = "2";
                }
                else {
                    str = "1";
                }
                int year = when.Year - 1;
                if (when.Month > 7)
                    year++;
                string URl = "https://s1.wcy.wat.edu.pl/ed/logged_inc.php?" + args[0] + "&mid=328&iid=" + year + str + "&exv=" + groupNumber + "&pos=0&rdo=1&" + args[args.Length - 1];
                webB.Navigate(URl);
                //
                progress.Value = 30;
                await PageLoad(10);
                //
                //webB.Document.InvokeScript("showGroupPlan", new string[] { groupNumber });
                //
                //progress.Value = 45;
                //await PageLoad(10);
                //
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += asyncCompletedEventHandler;
                webClient.DownloadProgressChanged += downloadProgressChangedEventHandler;
                Uri uri = new Uri("https://s1.wcy.wat.edu.pl/ed/" + webB.Document.InvokeScript("prepareURL") + "DTXT");
                Directory.CreateDirectory(envPath);
                progress.Value = 50;
                webClient.DownloadFileAsync(uri, envPath + tmpFileName);
            }
            catch {
                System.Windows.Forms.MessageBox.Show("Nie można nawiązać połączenia z serwerem!");
                textBox.Visibility = Visibility.Hidden;
            }
            finally {
            }
        }

        public void button_ShowChanges_Click(object sender, RoutedEventArgs e) {
            downloadGroupPlan(Encryptor.Decrypt(Properties.Settings.Default.Group),
                Properties.Settings.Default.Season, DateTime.Now,
                new AsyncCompletedEventHandler(CompletedNewSchedule),
                new DownloadProgressChangedEventHandler(ProgressChanged),
                "\\tmp");
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            downloadGroupPlan(Encryptor.Decrypt(Properties.Settings.Default.Group),
                Properties.Settings.Default.Season, DateTime.Now,
                new AsyncCompletedEventHandler(CompletedCheckSchedule),
                new DownloadProgressChangedEventHandler(ProgressChanged),
                "\\tmp");
        }
        private List<Appointment.Appointment> getAppointmentsFromFile(string fileName) {
            string plan = File.ReadAllText(envPath + "\\" + fileName, Encoding.Default);
            if (plan.Equals("")) {
                System.Windows.Forms.MessageBox.Show("Pobieranie kalendarza nie powidło się");
                return null;
            }
            else {
                string[] lines = plan.Split('\n');
                List<Appointment.Appointment> allEvents = new List<Appointment.Appointment>();
                foreach (string line in lines) {
                    Appointment.Appointment ev = null;
                    try {
                        string[] parts = line.Split(',');
                        string firstLetters = "";
                        string subject;
                        string a, b;
                        string[] arr = parts[0].Split(' ');
                        for (int i = 0; i < arr.Length - 2; i++) {
                            firstLetters += arr[i].Substring(0, 1).ToUpper();
                        }
                        try { a = arr[arr.Length - 2]; }
                        catch { a = ""; }
                        try { b = arr[arr.Length - 1]; }
                        catch { b = ""; }
                        subject = firstLetters + " " + a + " " + b + " " + parts[1];
                        try {
                            ev = new Appointment.Appointment(
                                                    subject, firstLetters, parts[1], parts[0],
                                                    DateTime.ParseExact(parts[2] + " " + parts[3], "yyyy-MM-dd HH:mm",
                                                    System.Globalization.CultureInfo.InvariantCulture),
                                                    DateTime.ParseExact(parts[4] + " " + parts[5], "yyyy-MM-dd HH:mm",
                                                    System.Globalization.CultureInfo.InvariantCulture));
                            allEvents.Add(ev);
                        }
                        catch {; }
                    }
                    catch { }
                }
                //kolorowanie
                Dictionary<string, Color> dict = new Dictionary<string, Color>();
                Color clr;
                Random randomGen = new Random();
                foreach (var apt in allEvents) {
                    if (dict.TryGetValue(apt._Short, out clr)) {
                        apt.BGA = clr.A;
                        apt.BGR = clr.R;
                        apt.BGG = clr.G;
                        apt.BGB = clr.B;
                    }
                    else {
                        KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
                        KnownColor randomColorName = names[randomGen.Next(names.Length)];
                        Color randomColor = Color.FromKnownColor(randomColorName);
                        dict.Add(apt._Short, randomColor);
                        apt.BGA = randomColor.A;
                        apt.BGR = randomColor.R;
                        apt.BGG = randomColor.G;
                        apt.BGB = randomColor.B;
                    }
                    apt.BBA = Color.DarkGray.A;
                    apt.BBR = Color.DarkGray.R;
                    apt.BBG = Color.DarkGray.G;
                    apt.BBB = Color.DarkGray.B;
                }
                //
                return allEvents;
            }

        }
        private void CompletedNewSchedule(object sender, AsyncCompletedEventArgs e) {
            List<Appointment.Appointment> allEvents = getAppointmentsFromFile("tmp");
            if (allEvents != null) {
                //
                schedule = allEvents;
                //
                writeScheduleToXMLFile(envPath, "default.xml", schedule);
                SetAppointments();
            }

            textBox.Visibility = Visibility.Hidden;
        }
        private void CompletedCheckSchedule(object sender, AsyncCompletedEventArgs e) {
            List<Appointment.Appointment> allEvents = getAppointmentsFromFile("tmp");
            if (allEvents != null) {
                //
                bool hasChanged = false;
                if (schedule.Count == allEvents.Count)
                    for (int i = 0; i < schedule.Count; i++) {
                        if (!schedule[i].IsEqualTo(allEvents[i]))
                            hasChanged = true;
                    }
                else
                    hasChanged = true;
                //wyświetlić nowy plan zajec
                //albo różnice

                //zapytac czy zaktualizowac
                if (hasChanged) {
                    if (System.Windows.Forms.DialogResult.Yes == System.Windows.Forms.MessageBox.Show("Czy chesz zaktualizować plan zajęć?", "Wykryto zmiany w planie zajęć", MessageBoxButtons.YesNo)) {
                        schedule = allEvents;
                        writeScheduleToXMLFile(envPath, "default.xml", schedule);
                        //
                    }
                }
                else
                    System.Windows.Forms.MessageBox.Show("Brak zmian w planie");
                SetAppointments();
            }

            textBox.Visibility = Visibility.Hidden;
        }
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            progress.Value += e.ProgressPercentage;
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e) {
            var frm = new SettingsWindow();
            frm.Owner = this;
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            frm.ShowDialog();
            login = Encryptor.Decrypt(Properties.Settings.Default.Login);
            pass = Encryptor.Decrypt(Properties.Settings.Default.Pass);
            groupNo = Encryptor.Decrypt(Properties.Settings.Default.Group);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Properties.Settings.Default.Remember) {
                Properties.Settings.Default.Save();
            }
            else {
                Properties.Settings.Default.Login = "";
                Properties.Settings.Default.Pass = "";
                Properties.Settings.Default.Group = "";
                Properties.Settings.Default.Save();
            }
            writeScheduleToXMLFile(envPath, "default.xml", schedule);
        }

        private async Task PageLoad(int TimeOut) {
            TaskCompletionSource<bool> PageLoaded = null;
            PageLoaded = new TaskCompletionSource<bool>();
            int TimeElapsed = 0;
            webB.DocumentCompleted += (s, e) => {
                if (webB.ReadyState != WebBrowserReadyState.Complete)
                    return;
                if (PageLoaded.Task.IsCompleted)
                    return;
                PageLoaded.SetResult(true);
            };
            while (PageLoaded.Task.Status != TaskStatus.RanToCompletion) {
                await Task.Delay(13);
                TimeElapsed++;
                if (TimeElapsed >= TimeOut * 100)
                    PageLoaded.TrySetResult(true);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog {
                Filter = "Plik tekstowy|*.txt|Wszystko|*.*",
                Title = "Wczytaj plan zajęć",
                InitialDirectory = envPath
            };

            DialogResult openFileDialogResult = ofd.ShowDialog();
            if (openFileDialogResult == System.Windows.Forms.DialogResult.OK && ofd.FileName != "") {
                List<Appointment.Appointment> allEvents = getAppointmentsFromFile("tmp");
                if (allEvents != null) {
                    schedule = allEvents;
                    //
                    writeScheduleToXMLFile(envPath, "default.xml", schedule);
                }
                SetAppointments();
            }
        }

        private async void MenuItem_Click_2(object sender, RoutedEventArgs e) {

            BaseWindow window = new BaseWindow();
            comparePlans = new ComparePlans(ref window);
            window.ClientArea.Content = comparePlans;
            window.Closing += Window_Closing1Async;

            window.ShowDialog();
        }

        private async void Window_Closing1Async(object sender, CancelEventArgs e) {
            string teacherName = comparePlans.teacherName;
            string[] groups = comparePlans.groups;
            //string building = "65";
            //string room = "125";

            //pobrac nazwy wszystkich grup
            //dla kazdej grupy pobrac plan

            List<Appointment.Appointment>[] allGroups = new List<Appointment.Appointment>[groups.Length];

            for (int i = 0; i < groups.Length; i++)
                downloadGroupPlan(groups[i],
                    Properties.Settings.Default.Season, DateTime.Now,
                    new AsyncCompletedEventHandler(SaveGroupForCompare),
                    new DownloadProgressChangedEventHandler(ProgressChanged),
                    "\\" + groups[i]);//plik nazwac jak grupa i potem usunąć


            Appointment.Appointment firstAppointment = null;
            Appointment.Appointment lastAppointment = null;

            await groupSemaphore.WaitAsync();

            for (int i = 0; i < groups.Length; i++)
                allGroups[i] = getAppointmentsFromFile(groups[i]);//zaladowac plany grup do list

            foreach (var list in allGroups)
                foreach (var appo in list) {
                    if (firstAppointment == null)
                        firstAppointment = appo;
                    if (lastAppointment == null)
                        lastAppointment = appo;
                    if (appo.StartTime < firstAppointment._StartTime)
                        firstAppointment = appo;
                    if (appo.StartTime > lastAppointment._StartTime)
                        lastAppointment = appo;
                }


            //pobrac plan nauczyciela
            downloadTeacherPlan(teacherName,
                Properties.Settings.Default.Season,
                DateTime.Now,
                new AsyncCompletedEventHandler(SaveTeacherForCompare),
                new DownloadProgressChangedEventHandler(ProgressChanged));

            await teacherSemaphore.WaitAsync();
            //zaladowac plan nauczyciela do listy
            List<Appointment.Appointment> teacherEvents = getAppointmentsFromFile("teacher");

            foreach (var appo in teacherEvents) {
                if (firstAppointment == null)
                    firstAppointment = appo;
                if (lastAppointment == null)
                    lastAppointment = appo;
                if (appo.StartTime < firstAppointment._StartTime)
                    firstAppointment = appo;
                if (appo.StartTime > lastAppointment._StartTime)
                    lastAppointment = appo;
            }


            groupSemaphore.Release();
            teacherSemaphore.Release();

            List<DateTime> allFreeDays = new List<DateTime>();

            DateTime tempDay = new DateTime(firstAppointment._StartTime.Ticks);

            while (tempDay < lastAppointment._StartTime) {

                bool shouldBeAdded = true;
                foreach (var appo in teacherEvents) {
                    if (appo._StartTime == tempDay) {
                        shouldBeAdded = false;
                        break;
                    }
                }
                foreach (var list in allGroups)
                    foreach (var appo in list) {
                        if (appo._StartTime == tempDay) {
                            shouldBeAdded = false;
                            break;
                        }
                    }
                if (shouldBeAdded)
                    allFreeDays.Add(new DateTime(tempDay.Ticks));

                if (tempDay.DayOfWeek == DayOfWeek.Saturday || tempDay.DayOfWeek == DayOfWeek.Sunday) {
                    tempDay = tempDay.AddDays(1);
                }
                else {
                    if (tempDay.TimeOfDay < Appointment.Appointment.PossibleStartTimes[6]) {
                        if (tempDay.TimeOfDay == Appointment.Appointment.PossibleStartTimes[0]) {
                            tempDay = tempDay.AddHours(2);
                            tempDay = tempDay.AddMinutes(-10);
                        }
                        else if (tempDay.TimeOfDay == Appointment.Appointment.PossibleStartTimes[1]) {
                            tempDay = tempDay.AddHours(2);
                            tempDay = tempDay.AddMinutes(-10);
                        }
                        else if (tempDay.TimeOfDay == Appointment.Appointment.PossibleStartTimes[2]) {
                            tempDay = tempDay.AddHours(2);
                            tempDay = tempDay.AddMinutes(-10);
                        }
                        else if (tempDay.TimeOfDay == Appointment.Appointment.PossibleStartTimes[3]) {
                            tempDay = tempDay.AddHours(2);
                            tempDay = tempDay.AddMinutes(15);
                        }
                        else if (tempDay.TimeOfDay == Appointment.Appointment.PossibleStartTimes[4]) {
                            tempDay = tempDay.AddHours(2);
                            tempDay = tempDay.AddMinutes(-10);
                        }
                        else if (tempDay.TimeOfDay == Appointment.Appointment.PossibleStartTimes[5]) {
                            tempDay = tempDay.AddHours(2);
                            tempDay = tempDay.AddMinutes(-10);
                        }
                    }
                    else {
                        tempDay = tempDay.AddHours(12);
                        tempDay = tempDay.AddMinutes(35);
                    }
                }
            }
            using (TextWriter tw = new StreamWriter(envPath + "\\freeDays")) {
                foreach (var s in allFreeDays)
                    tw.WriteLine(s.ToString());
            }





            //pobrac plan sali
            //downloadRoomPlan(building, room,
            //    Properties.Settings.Default.Season,
            //    DateTime.Now,
            //    new AsyncCompletedEventHandler(SaveRoomForCompare),
            //    new DownloadProgressChangedEventHandler(ProgressChanged));

            //zaladowac plan sali do listy

        }

        private void SaveGroupForCompare(object sender, AsyncCompletedEventArgs e) {
            List<Appointment.Appointment> allEvents = getAppointmentsFromFile("tmp");
            if (allEvents != null) {
                //
            }

            groupSemaphore.Release();
            textBox.Visibility = Visibility.Hidden;
        }

        private void SaveTeacherForCompare(object sender, AsyncCompletedEventArgs e) {
            List<Appointment.Appointment> allEvents = getAppointmentsFromFile("teacher");
            if (allEvents != null) {
                //

            }

            teacherSemaphore.Release();
            textBox.Visibility = Visibility.Hidden;
        }

        private void SaveRoomForCompare(object sender, AsyncCompletedEventArgs e) {
            List<Appointment.Appointment> roomEvents = getAppointmentsFromFile("65+125");

            if (roomEvents != null) {
                //
                //
            }

            textBox.Visibility = Visibility.Hidden;
        }

        private async void downloadTeacherPlan(string teacherName, int seasonNum, DateTime when, AsyncCompletedEventHandler asyncCompletedEventHandler, DownloadProgressChangedEventHandler downloadProgressChangedEventHandler) {
            await teacherSemaphore.WaitAsync();

            //teacherName można by rozdzielić na imie nazwisko innaczej tylko pierwsza osoba z tym nazwiskiem będzie uwzględniona

            textBox.Visibility = Visibility.Visible;
            progress.Value = 0;
            webB.Navigate("https://s1.wcy.wat.edu.pl/ed/");
            //
            await PageLoad(10);
            //
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("name");
                if (comp.Equals("userid")) {
                    el.SetAttribute("value", login);
                    break;
                }
            }
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("name");
                if (comp.Equals("password")) {
                    el.SetAttribute("value", pass);
                    break;
                }
            }
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("value");
                if (comp.Equals(" Zaloguj się ")) {
                    el.InvokeMember("click");
                    break;
                }
            }
            //
            progress.Value = 15;
            await PageLoad(10);
            //

            try {
                string[] parts2 = webB.Url.AbsoluteUri.Split('?');
                string tmp = parts2[1];
                if (parts2.Length > 2)
                    for (int i = 1; i < parts2.Length; i++)
                        tmp += "?" + parts2[i];

                string[] args = tmp.Split('&');
                string str;
                if (seasonNum == 0) {
                    str = "2";
                }
                else {
                    str = "1";
                }
                int year = when.Year - 1;
                if (when.Month > 7)
                    year++;
                string URl = "https://s1.wcy.wat.edu.pl/ed/logged_inc.php?" + args[0] + "&mid=327&iid=" + year + str + "&pos=0&rdo=1&" + args[args.Length - 1];
                webB.Navigate(URl);
                //
                progress.Value = 30;
                await PageLoad(10);

                foreach (HtmlElement el in webB.Document.GetElementsByTagName("option")) {
                    string comp = el.InnerText;
                    if (comp.Contains(teacherName)) {
                        el.InvokeMember("click");
                        el.SetAttribute("selected", "selected");
                        break;
                    }
                }
                webB.Document.InvokeScript("showPersonPlan");

                await PageLoad(10);
                //
                //webB.Document.InvokeScript("showGroupPlan", new string[] { groupNumber });
                //
                //progress.Value = 45;
                //await PageLoad(10);
                //
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += asyncCompletedEventHandler;
                webClient.DownloadProgressChanged += downloadProgressChangedEventHandler;
                Uri uri = new Uri("https://s1.wcy.wat.edu.pl/ed/" + webB.Document.InvokeScript("prepareURL") + "DTXT");
                Directory.CreateDirectory(envPath);
                progress.Value = 50;
                webClient.DownloadFileAsync(uri, envPath + "//teacher");
            }
            catch {
                System.Windows.Forms.MessageBox.Show("Nie można nawiązać połączenia z serwerem!");
                textBox.Visibility = Visibility.Hidden;
            }
            finally {

            }
        }

        private async void downloadRoomPlan(string building, string room, int seasonNum, DateTime when, AsyncCompletedEventHandler asyncCompletedEventHandler, DownloadProgressChangedEventHandler downloadProgressChangedEventHandler) {
            textBox.Visibility = Visibility.Visible;
            progress.Value = 0;
            webB.Navigate("https://s1.wcy.wat.edu.pl/ed/");
            //
            await PageLoad(10);
            //
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("name");
                if (comp.Equals("userid")) {
                    el.SetAttribute("value", login);
                    break;
                }
            }
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("name");
                if (comp.Equals("password")) {
                    el.SetAttribute("value", pass);
                    break;
                }
            }
            foreach (HtmlElement el in webB.Document.GetElementsByTagName("input")) {
                string comp = el.GetAttribute("value");
                if (comp.Equals(" Zaloguj się ")) {
                    el.InvokeMember("click");
                    break;
                }
            }
            //
            progress.Value = 15;
            await PageLoad(10);
            //

            try {
                string[] parts2 = webB.Url.AbsoluteUri.Split('?');
                string tmp = parts2[1];
                if (parts2.Length > 2)
                    for (int i = 1; i < parts2.Length; i++)
                        tmp += "?" + parts2[i];

                string[] args = tmp.Split('&');
                string str;
                if (seasonNum == 0) {
                    str = "2";
                }
                else {
                    str = "1";
                }
                int year = when.Year - 1;
                if (when.Month > 7)
                    year++;
                string URl = "https://s1.wcy.wat.edu.pl/ed/logged_inc.php?" + args[0] + "&mid=294&iid=" + year + str + "&pos=0&rdo=1&" + args[args.Length - 1];
                webB.Navigate(URl);
                //
                progress.Value = 30;


                webB.Show();
                await PageLoad(10);



                foreach (HtmlElement el in webB.Document.GetElementsByTagName("option")) {
                    string comp = el.InnerText;
                    if (el.Parent.Name.Equals("Data1"))
                        if (comp.Equals(building)) {
                            el.InvokeMember("click");
                            el.SetAttribute("selected", "selected");
                            el.InvokeMember("onChange");
                            el.Parent.InvokeMember("onChange");
                            break;
                        }
                        else {
                            el.SetAttribute("selected", "");
                        }
                }

                webB.Document.InvokeScript("changeBuiling");

                progress.Value = 40;

                await PageLoad(10);

                foreach (HtmlElement el in webB.Document.GetElementsByTagName("option")) {
                    string comp = el.InnerText;
                    if (el.Parent.Name.Equals("Data2"))
                        if (comp.Equals(room)) {
                            el.InvokeMember("click");
                            el.SetAttribute("selected", "selected");
                            el.InvokeMember("onChange");
                            el.Parent.InvokeMember("onChange");
                            break;
                        }
                        else {
                            el.SetAttribute("selected", "");
                        }
                }
                webB.Document.InvokeScript("showRoomPlan");

                await PageLoad(10);


                //wyciąganie danych z tabeli




                //
                //webB.Document.InvokeScript("showGroupPlan", new string[] { groupNumber });
                //
                //progress.Value = 45;
                //await PageLoad(10);
                //
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += asyncCompletedEventHandler;
                webClient.DownloadProgressChanged += downloadProgressChangedEventHandler;
                Uri uri = new Uri("https://s1.wcy.wat.edu.pl/ed/" + webB.Document.InvokeScript("prepareURL") + "DTXT");
                Directory.CreateDirectory(envPath);
                progress.Value = 50;
                webClient.DownloadFileAsync(uri, envPath + "//" + building + "+" + room);
            }
            catch {
                System.Windows.Forms.MessageBox.Show("Nie można nawiązać połączenia z serwerem!");
                textBox.Visibility = Visibility.Hidden;

            }
        }

    }
}
