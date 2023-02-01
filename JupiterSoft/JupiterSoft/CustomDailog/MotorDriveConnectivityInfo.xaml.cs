using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JupiterSoft.CustomDailog
{
    /// <summary>
    /// Interaction logic for MotorDriveConnectivityInfo.xaml
    /// </summary>
    public partial class MotorDriveConnectivityInfo : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9-.]+");
        public MotorDriveConnectivityInfo()
        {
            InitializeComponent();
        }

        
        

        public bool Canceled { get; set; }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Regex.IsMatch(Addressbox.Text, @"^\d+$") && !_regex.IsMatch(MotorFrequency.Text.ToString()) && Regex.IsMatch(MotorRegister.Text, @"^\d+$"))
                {
                    Canceled = false;
                    Close();
                }
                else
                {
                    MessageBox.Show("Please enter valid frequency and register number.");
                }
            }
            catch {
                MessageBox.Show("Please enter valid frequency and register number.");
            }

        }
    }
}
