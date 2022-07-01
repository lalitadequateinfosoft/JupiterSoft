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
    /// Interaction logic for ConfigurationDailog.xaml
    /// </summary>
    public partial class ConfigurationDailog : Window
    {
        private List<CustomDeviceInfo> CustomDeviceInfos;
        public string DeviceId { get; set; }
        public int BaudRate { get; set; }
        public int Databit { get; set; }
        public int Stopbit { get; set; }
        public int ParityValue { get; set; }


        public ConfigurationDailog(string title,List<CustomDeviceInfo> _CustomDeviceInfos)
        {
            InitializeComponent();
            CustomDeviceInfos = _CustomDeviceInfos;
            HeaderTitle.Content = title;
            LoadData();
        }

        public bool Canceled { get; set; }


        public void LoadData()
        {
            //Load Comport.
            var devices = CustomDeviceInfos;
            devices.Add(new CustomDeviceInfo { DeviceID = "0", PortName = "-Select-" });
            ComPortConfig.ItemsSource = devices;
            ComPortConfig.SelectedValuePath = "DeviceID";
            ComPortConfig.DisplayMemberPath = "PortName";
            ComPortConfig.SelectedValue = "0";


            //Load BaudRate.
            var baudRateData = new List<BaudRateModel>
            {
            new BaudRateModel{Id=1,Name="38400" },
            new BaudRateModel{Id=2,Name="9600" }
            };

            BaudRateConfig.ItemsSource = baudRateData;
            BaudRateConfig.SelectedValuePath = "Id";
            BaudRateConfig.DisplayMemberPath = "Name";
            BaudRateConfig.SelectedValue = "1";

            //Load DataBit.
            var DatabitData = new List<DatabitModel>
            {
            new DatabitModel{Id=1,Name="5" },
            new DatabitModel{Id=2,Name="6" },
            new DatabitModel{Id=3,Name="7" },
            new DatabitModel{Id=4,Name="8" }
            };

            DataBitConfig.ItemsSource = DatabitData;
            DataBitConfig.SelectedValuePath = "Id";
            DataBitConfig.DisplayMemberPath = "Name";
            DataBitConfig.SelectedValue = "4";

            //Load DataBit.
            var StopbitData = new List<StopBitModel>
            {
            new StopBitModel{Id=1,Name="1" },
            new StopBitModel{Id=2,Name="1.5" },
            new StopBitModel{Id=3,Name="2" }
            };

            StopBitConfig.ItemsSource = StopbitData;
            StopBitConfig.SelectedValuePath = "Id";
            StopBitConfig.DisplayMemberPath = "Name";
            StopBitConfig.SelectedValue = "3";

            //Load Parity
            var parityData = new List<ParityModel> {
            new ParityModel{Id=0,Name="None" },
            new ParityModel{Id=1,Name="Odd" },
            new ParityModel{Id=2,Name="Even" },
            new ParityModel{Id=3,Name="Mark" },
            new ParityModel{Id=4,Name="Space" },
            };



            ParityConfig.ItemsSource = parityData;
            ParityConfig.SelectedValuePath = "Id";
            ParityConfig.DisplayMemberPath = "Name";
            ParityConfig.SelectedValue = "0";
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (ComPortConfig.SelectedValue.ToString() != "0")
            {
                DeviceId = ComPortConfig.SelectedValue.ToString();
                var selectedBaud = (BaudRateModel)BaudRateConfig.SelectedItem;
                BaudRate = Convert.ToInt32(selectedBaud.Name.ToString());
                var selectdataBit = (DatabitModel)DataBitConfig.SelectedItem;
                Databit = Convert.ToInt32(selectdataBit.Name.ToString());
                var selectStopBit = (StopBitModel)StopBitConfig.SelectedItem;
                Stopbit = Convert.ToInt32(selectStopBit.Name.ToString());
                ParityValue = Convert.ToInt32(ParityConfig.SelectedValue.ToString());
                Canceled = false;
                Close();
            }
            else
            {
                MessageBox.Show("Please select COM PORT");
            }
                
        }

    }
}
