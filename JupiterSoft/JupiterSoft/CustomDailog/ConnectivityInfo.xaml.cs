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
    /// Interaction logic for ConnectivityInfo.xaml
    /// </summary>
    public partial class ConnectivityInfo : Window
    {
        public ConnectivityInfo()
        {
            InitializeComponent();
        }

        private static readonly Regex _regex = new Regex("[^0-9-]+");
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
                if (Regex.IsMatch(AddressBox.Text, @"^\d+$")
                                && Regex.IsMatch(PushingArm.Text, @"^\d+$")
                                && Regex.IsMatch(Sensors.Text, @"^\d+$"))
                {
                    Canceled = false;
                    Close();
                }
                else
                {
                    MessageBox.Show("Please enter valid input");
                }
            }
            catch
            {
                MessageBox.Show("Please enter valid input");
            }
            

        }
    }
}
