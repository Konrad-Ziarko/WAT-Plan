using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Kalendarz.Properties;

namespace WAT
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            if (Settings.Default.Remember)
            {
                textBox.Text = Encryptor.Decrypt(Settings.Default.Login);
                textBox1.Password = Encryptor.Decrypt(Settings.Default.Pass);
                textBox2.Text = Encryptor.Decrypt(Settings.Default.Group);
                checkBox.IsChecked = Settings.Default.Remember;
                comboBox.SelectedIndex = Settings.Default.Season;
            }
        }
        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Login = Encryptor.Encrypt(textBox.Text);
            Settings.Default.Pass = Encryptor.Encrypt(textBox1.Password.ToString());
            Settings.Default.Group = Encryptor.Encrypt(textBox2.Text);
            Settings.Default.Season = comboBox.SelectedIndex;
            if (!checkBox.IsChecked.HasValue) //check for a value  
            {
                Settings.Default.Remember = false;
            }
            else
                Settings.Default.Remember = true;
        }
    }
}
