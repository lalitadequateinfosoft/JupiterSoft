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
    /// Interaction logic for RangeAndUnit.xaml
    /// </summary>
    public partial class RangeAndUnit : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        public RangeAndUnit()
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
            if (string.IsNullOrEmpty(MinimumRange.Text) || MinimumRange.Text.Contains(" ") || _regex.IsMatch(MinimumRange.Text))
            {
                MessageBox.Show("Please enter valid minimum range.");
                return;
            }

            if (string.IsNullOrEmpty(MaxRange.Text) || MaxRange.Text.Contains(" ") || _regex.IsMatch(MaxRange.Text))
            {
                MessageBox.Show("Please enter valid maximum range.");
                return;
            }

            if (unit.SelectionBoxItem.ToString().ToLower() == "select unit")
            {
                MessageBox.Show("Please select a unit.");
                return;
            }

            Canceled = false;
            Close();

        }
    }
}
