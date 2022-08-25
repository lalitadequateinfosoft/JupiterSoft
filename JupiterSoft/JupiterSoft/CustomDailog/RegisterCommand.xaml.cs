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
    /// Interaction logic for RegisterCommand.xaml
    /// </summary>
    public partial class RegisterCommand : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        public RegisterCommand()
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
            if (!string.IsNullOrEmpty(RegisterNumber.Text) && !RegisterNumber.Text.Contains(" ") && !_regex.IsMatch(RegisterNumber.Text) && !string.IsNullOrEmpty(RegisterOutput.SelectedValue.ToString()))
            {
                Canceled = false;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter valid value of register");
            }

        }
    }
}
