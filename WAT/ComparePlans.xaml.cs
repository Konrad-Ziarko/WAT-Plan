using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WAT {
    /// <summary>
    /// Interaction logic for ComparePlans.xaml
    /// </summary>
    public partial class ComparePlans : UserControl {

        private BaseWindow MyBaseWindow;

        public string[] groups { get; set; }
        public string teacherName { get; set; }

        public ComparePlans(ref BaseWindow MyBaseWindow) {
            InitializeComponent();

            MyBaseWindow.Title = "Porównaj plany";
            this.MyBaseWindow = MyBaseWindow;

            MyBaseWindow.SizeChanged += MyBaseWindow_SizeChanged;
            MyBaseWindow.Closing += MyBaseWindow_Closing;

            this.DataContext = this;

        }
        private void MyBaseWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            Height = MyBaseWindow.ClientArea.Height;
            Width = MyBaseWindow.ClientArea.Width;
        }

        private void MyBaseWindow_Closing(object sender, CancelEventArgs e) {

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            teacherName = teacherBox.Text;
            groups = groupsBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            MyBaseWindow.Close();
        }
    }
}
