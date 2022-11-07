using JupiterSoft.Models;
using JupiterSoft.ViewModel;
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
    /// Interaction logic for CalibrationDailog.xaml
    /// </summary>
    public partial class CalibrationDailog : Window
    {
        CalibrationViewModel calibrationViewModel;
       public List<CalibrationModel> calibrations;
        public CalibrationDailog()
        {
            calibrationViewModel = new CalibrationViewModel();
            calibrations = new List<CalibrationModel>();
            InitializeComponent();
            DataContext = calibrationViewModel;
        }

        private void AddCalibration_Click(object sender, RoutedEventArgs e)
        {
            if(this.DataContext is CalibrationViewModel model)
            {
                model.addcalibrationrow();
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(this.DataContext is CalibrationViewModel model)
            {
                model.removeCalibration(Convert.ToInt32(button.Tag));
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (this.DataContext is CalibrationViewModel model)
            {
                model.updateCalibrationVal(Convert.ToInt32(textBox.Tag),Convert.ToDecimal(textBox.Text));
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var textBox = sender as ComboBox;
            if (this.DataContext is CalibrationViewModel model)
            {
                switch(textBox.SelectionBoxItem.ToString())
                {
                    case "Add":
                        model.updateCalibrationCommand(Convert.ToInt32(textBox.Tag), (int)functionConstant.Add);
                        break;
                    case "Subtract":
                        model.updateCalibrationCommand(Convert.ToInt32(textBox.Tag), (int)functionConstant.Subtract);
                        break;
                    case "Multiply":
                        model.updateCalibrationCommand(Convert.ToInt32(textBox.Tag), (int)functionConstant.Multiply);
                        break;
                    case "Divide":
                        model.updateCalibrationCommand(Convert.ToInt32(textBox.Tag), (int)functionConstant.Divide);
                        break;
                }
                
            }
        }

        public bool Canceled { get; set; }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if(this.DataContext is CalibrationViewModel model)
            {
                calibrations = model.Itemist;
            }

            Canceled = false;
            Close();
        }
    }
}
