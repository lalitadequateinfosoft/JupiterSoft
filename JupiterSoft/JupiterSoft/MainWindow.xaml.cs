using JupiterSoft.Models;
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
using FontAwesome;
using System.IO;
using System.Threading;

namespace JupiterSoft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BrushConverter bc;
        private string _FileDirectory = ApplicationConstant._FileDirectory;
        public MainWindow()
        {
            InitializeComponent();
            CheckDefaultConfiguration();
        }

        private void StartNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartNew.IsEnabled = false;
                var dashForm = new Dashboard();
                dashForm.Show();
                this.Close();
            }
            catch
            {
                StartNew.IsEnabled = true;
            }
        }

        private void StartSaved_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartSaved.IsEnabled = false;

                var dashForm = new Dashboard();
                dashForm.Show();
                this.Close();
            }
            catch
            {
                StartSaved.IsEnabled = true;
            }
        }

        private void CheckDefaultConfiguration()
        {
            if (System.IO.Directory.Exists(_FileDirectory))
            {
                DirectoryInfo d = new DirectoryInfo(_FileDirectory);

                FileInfo[] Files = d.GetFiles("*.json");
                if (Files != null && Files.Length > 0)
                {
                    foreach (FileInfo file in Files)
                    {
                        if (file.Name.Contains('_'))
                        {
                            string[] FileSpl = file.Name.Split('_');
                            if (FileSpl.Last().ToString().ToLower() == "default.json")
                            {
                                var dashForm = new Dashboard(file.FullName.ToString());
                                dashForm.Show();
                                this.Close();
                                break;
                            }
                        }

                    }
                }


            }
        }
    }
}
