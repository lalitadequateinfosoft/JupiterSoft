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
using Util;

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
        private int readIndex = 0;

        public string ConditionTextName { get; set; }
        public string ComparisonVariable { get; set; }
        public int ComparisonCondition { get; set; }
        public string ComparisonValue { get; set; }
        public List<ConditionDataModel> conditionDatas;

        public ConditionsDialog(List<LogicalCommand> _Commands)
        {
            conditions = new List<ConditionModel>();
            Commands = _Commands;
            InitializeComponent();
            loadComaprisonVariables();
            loadConditions();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(Condition.Text))
            {
                Condition.Focus();
                Condition.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                Condition.BorderBrush = Brushes.Black;
                ConditionTextName = Condition.Text;
            }

            if(ComparisonValue1.SelectedValue==null)
            {
                ComparisonValue1.Focus();
                ComparisonValue1.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                ComparisonValue1.BorderBrush = Brushes.Black;
                ComparisonVariable = ComparisonValue1.SelectedValue.ToString();
            }

            if(ComparisonType.SelectedValue==null)
            {
                ComparisonType.Focus();
                ComparisonType.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                ComparisonType.BorderBrush = Brushes.Black;
                ComparisonCondition = Convert.ToInt32(ComparisonType.SelectedValue.ToString());
            }

            if(string.IsNullOrEmpty(ComparisonValue2.Text))
            {
                ComparisonValue2.Focus();
                ComparisonValue2.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                ComparisonValue2.BorderBrush = Brushes.Black;
                ComparisonValue = ComparisonValue2.Text.ToString();
            }
            conditionDatas = new List<ConditionDataModel>();
            conditionDatas.Add(new ConditionDataModel { ComparisonVariable = ComparisonVariable, ComparisonCondition = ComparisonCondition, ComparisonValue = ComparisonValue });
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

        private void loadComaprisonVariables()
        {
            List<OutPutModel> outPuts = new List<OutPutModel>();
            foreach (var item in Commands)
            {
                //var data = GetControlCardVariableIndex(item);
                //outPuts.AddRange(data);
                outPuts.Add(new OutPutModel { OutPutText = item.CommandText, OutPutVal = item.CommandType.ToString()});
            }

            ComparisonValue1.ItemsSource = outPuts;
            ComparisonValue1.SelectedValuePath = "OutPutVal";
            ComparisonValue1.DisplayMemberPath = "OutPutText";
        }


        private List<OutPutModel> GetControlCardVariableIndex(LogicalCommand command)
        {
            List<OutPutModel> outPuts = new List<OutPutModel>();

                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 0 + "]", OutPutVal = command.CommandText + "_" + 3 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 1 + "]", OutPutVal = command.CommandText + "_" + 5 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 2 + "]", OutPutVal = command.CommandText + "_" + 7 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 3 + "]", OutPutVal = command.CommandText + "_" + 9 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 4 + "]", OutPutVal = command.CommandText + "_" + 11 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 5 + "]", OutPutVal = command.CommandText + "_" + 13 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 6 + "]", OutPutVal = command.CommandText + "_" + 15 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 7 + "]", OutPutVal = command.CommandText + "_" + 17 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 8 + "]", OutPutVal = command.CommandText + "_" + 19 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 9 + "]", OutPutVal = command.CommandText + "_" + 21 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 10 + "]", OutPutVal = command.CommandText + "_" + 23 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 11 + "]", OutPutVal = command.CommandText + "_" + 25 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 12 + "]", OutPutVal = command.CommandText + "_" + 27});
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 13 + "]", OutPutVal = command.CommandText + "_" + 29 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 14 + "]", OutPutVal = command.CommandText + "_" + 31 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 15 + "]", OutPutVal = command.CommandText + "_" + 33 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 16 + "]", OutPutVal = command.CommandText + "_" + 35 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 17 + "]", OutPutVal = command.CommandText + "_" + 37 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 18 + "]", OutPutVal = command.CommandText + "_" + 39 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 19 + "]", OutPutVal = command.CommandText + "_" + 41 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 20 + "]", OutPutVal = command.CommandText + "_" + 43 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 21 + "]", OutPutVal = command.CommandText + "_" + 47 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 22 + "]", OutPutVal = command.CommandText + "_" + 45 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 23 + "]", OutPutVal = command.CommandText + "_" + 49 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 24 + "]", OutPutVal = command.CommandText + "_" + 51 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 25 + "]", OutPutVal = command.CommandText + "_" + 53 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 26 + "]", OutPutVal = command.CommandText + "_" + 55 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 27 + "]", OutPutVal = command.CommandText + "_" + 57 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 28 + "]", OutPutVal = command.CommandText + "_" + 59 });
                    outPuts.Add(new OutPutModel { OutPutText = command.CommandText + "[" + 29 + "]", OutPutVal = command.CommandText + "_" + 61 });

            return outPuts;
        }
    }
}
