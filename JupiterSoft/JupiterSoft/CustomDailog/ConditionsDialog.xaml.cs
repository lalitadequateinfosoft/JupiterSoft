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
using System.Windows.Shapes;

namespace JupiterSoft.CustomDailog
{
    /// <summary>
    /// Interaction logic for ConditionsDialog.xaml
    /// </summary>
    public partial class ConditionsDialog : Window
    {
        public bool Canceled { get; set; }
        private List<ConditionModel> conditions;
        private List<LogicalCommand> Commands;
        public ConditionsDialog(List<LogicalCommand> _Commands)
        {
            conditions = new List<ConditionModel>();
            Commands = _Commands;
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Canceled = false;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
            Close();
        }

        private void loadConditions()
        {
            conditions = ControlOp.GetConditions();
            ComparisonType.ItemsSource = conditions;
            ComparisonType.SelectedValuePath = "value";
            ComparisonType.DisplayMemberPath = "text";
            ComparisonType.SelectedValue = ((int)ConditionConstant.is_equal_to).ToString();
        }

        private void ComparisonValue1_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
