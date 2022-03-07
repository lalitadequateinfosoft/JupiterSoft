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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JupiterSoft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartNew_Click(object sender, RoutedEventArgs e)
        {
            var dashForm = new Dashboard();
            dashForm.Show();
            this.Close();
        }

        private void StartSaved_Click(object sender, RoutedEventArgs e)
        {
            var dashForm = new Dashboard();
            dashForm.Show();
            this.Close();
        }
    }
}
