using JupiterSoft.Models;
using JupiterSoft.ViewModel;
using Ozeki;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Threading;

namespace JupiterSoft.CustomDailog
{
    /// <summary>
    /// Interaction logic for CalibrationHMI.xaml
    /// </summary>
    public partial class CalibrationHMI : Window
    {

        BrushConverter bc;
        CommunicationDevices weighing;
        private decimal LastReceivedVal = 0;
        private Dispatcher _dispathcer;
        public CalibrationHMIViewModel hMIViewModel;
        private DeviceInfo deviceInfo;
        System.Timers.Timer ExecutionTimer = new System.Timers.Timer();

        #region function Variable
        const int REC_BUF_SIZE = 10000;
        byte[] recBuf = new byte[REC_BUF_SIZE];
        byte[] recBufParse = new byte[REC_BUF_SIZE];

        public Byte[] _MbTgmBytes;
        internal UInt32 TotalReceiveSize = 0;
        bool IsComplete = false;
        private int readIndex = 0;
        #endregion
        public CalibrationHMI(UartDeviceConfiguration uartDevice)
        {
            bc = new BrushConverter();
            hMIViewModel = new CalibrationHMIViewModel();
            InitializeComponent();
            this.DataContext = hMIViewModel;
            _dispathcer = Dispatcher.CurrentDispatcher;
            deviceInfo = DeviceInformation.GetConnectedDevices();
            weighing = new CommunicationDevices();
            weighing.PortName = uartDevice.PortName;
            weighing.DeviceId = uartDevice.DeviceId;
            weighing.BaudRate = uartDevice.BaudRate;
            weighing.DataBit = uartDevice.DataBit;
            weighing.StopBit = uartDevice.StopBit;
            weighing.Parity = uartDevice.Parity;
            weighing.IsTurnedOn = false;
        }

        private void Span_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textb = sender as System.Windows.Controls.TextBox;
            if (this.DataContext is CalibrationHMIViewModel model)
            {
                if (!string.IsNullOrEmpty(textb.Text.ToString()))
                {
                    model.Span = Convert.ToDecimal(textb.Text.ToString());
                    model.CalculateSpan = true;
                }
            }
        }

        private void SaveCalibration_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Calibration has been set");
        }

        private void ResetCalibration_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CalibrationHMIViewModel model)
            {
                model.Zero = 0;
                model.Span = 0;
                model.Factor = 0;
                model.CalculateSpan = true;
                model.Weight = 0;
                LastReceivedVal = 0;
                MessageBox.Show("Calibration has been reset");
            }
        }


        #region logic
        private void ConnectWeight(string Port, int Baudrate, int databit, int stopbit, int parity)
        {
            try
            {
                if (weighing.IsTurnedOn == false)
                {

                    weighing.RecState = 1;
                    RecData _recData = new RecData();
                    _recData.SessionId = Common.GetSessionNewId;
                    _recData.Ch = 0;
                    _recData.Indx = 0;
                    _recData.Reg = 0;
                    _recData.NoOfVal = 0;
                    _recData.Status = PortDataStatus.Requested;
                    weighing.CurrentRequest = _recData;
                    SerialPort WeightDevice = new SerialPort();
                    SerialPortCommunications(ref WeightDevice, Port, Baudrate, databit, stopbit, parity);
                    weighing.SerialDevice = WeightDevice;
                    weighing.IsTurnedOn = true;
                }
            }
            catch (Exception ex)
            {
                //string log = "An error has occured:\r" + ex.StackTrace.ToString() + ".";
                //log = log + "\r\n error description : " + ex.ToString();
                //LogWriter.LogWrite(log, sessionId);
            }
        }
        #endregion

        #region Serial Port Coms

        private void SerialPortCommunications(ref SerialPort serialPort, string port = "", int baudRate = 0, int databit = 0, int stopBit = 0, int parity = 0)
        {

            try
            {
                serialPort = new SerialPort(port);
                serialPort.BaudRate = baudRate;
                serialPort.DataBits = databit;
                serialPort.StopBits = stopBit == 0 ? StopBits.None : (stopBit == 1 ? StopBits.One : (stopBit == 2 ? StopBits.Two : StopBits.OnePointFive));
                switch (parity)
                {
                    case (int)Parity.None:
                        serialPort.Parity = Parity.None;
                        break;
                    case (int)Parity.Odd:
                        serialPort.Parity = Parity.Odd;
                        break;
                    case (int)Parity.Even:
                        serialPort.Parity = Parity.Even;
                        break;
                    case (int)Parity.Mark:
                        serialPort.Parity = Parity.Mark;
                        break;
                    case (int)Parity.Space:
                        serialPort.Parity = Parity.Space;
                        break;
                }

                serialPort.Handshake = Handshake.None;
                serialPort.Encoding = ASCIIEncoding.ASCII;

                serialPort.DataReceived += WeightDevice_DataReceived;
                serialPort.ReadTimeout= 1000;

                serialPort.Open();
            }
            catch (Exception ex)
            {

            }

        }

        private void StopPortCommunication()
        {
            if (weighing.SerialDevice != null)
            {
                if (weighing.SerialDevice.IsOpen)
                {
                    weighing.SerialDevice.DtrEnable = false;
                    weighing.SerialDevice.RtsEnable = false;
                    weighing.SerialDevice.DataReceived -= WeightDevice_DataReceived;
                    weighing.SerialDevice.DiscardInBuffer();
                    weighing.SerialDevice.DiscardOutBuffer();
                    weighing.SerialDevice.Close();
                }
            }


        }

        private void WeightDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {


            if (weighing.IsTurnedOn == false)
            {
                return;
            }

            switch (weighing.RecState)
            {
                case 0:
                    break;
                case 1:
                    try
                    {
                        // int i = 0;
                        weighing.RecIdx = 0;
                        weighing.RecState = 1;
                        recBuf = new byte[weighing.SerialDevice.BytesToRead];
                        if (recBuf.Length <= 30)
                        {
                            recBuf = new byte[REC_BUF_SIZE];
                            return;
                        }
                        weighing.SerialDevice.Read(recBuf, 0, recBuf.Length);
                        weighing.ReceiveBufferQueue = new Queue<byte[]>();
                        weighing.ReceiveBufferQueue.Enqueue(recBuf);
                        weighing.LastResponseReceived = System.DateTime.Now;
                        weighing.IsComplete = true;
                        recBuf = new byte[REC_BUF_SIZE];

                        UartDataReader();
                    }
                    catch (Exception ex)
                    {
                        //string log = "An error has occured:\r" + ex.StackTrace.ToString() + ".";
                        //log = log + "\r\n error description : " + ex.ToString();
                        //LogWriter.LogWrite(log, sessionId);
                    }

                    break;
            }

            

        }

        private void UartDataReader()
        {
            try
            {
                if (weighing.RecState > 0 && weighing.IsComplete)
                {
                    weighing.IsComplete = false;


                    //recState = 1;
                    while (weighing.ReceiveBufferQueue.Count > 0)
                    {
                        recBufParse = weighing.ReceiveBufferQueue.Dequeue();

                        weighing.RecState = 1;
                        RecData _recData = weighing.CurrentRequest;

                        if (_recData != null)
                        {

                            Common.GoodTmgm++;
                            weighing.CurrentRequest.MbTgm = recBufParse;
                            weighing.CurrentRequest.Status = PortDataStatus.Received;
                            CompareWeightModuleResponse(weighing.CurrentRequest);
                            return;
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                //string log = "An error has occured:\r" + ex.StackTrace.ToString() + ".";
                //log = log + "\r\n error description : " + ex.ToString();
                //LogWriter.LogWrite(log, sessionId);
            }
        }

        void CompareWeightModuleResponse(RecData _recData)
        {
            try
            {
                if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
                {
                    byte[] bytestToRead = _recData.MbTgm.Skip(readIndex).ToArray();
                    string str = Encoding.Default.GetString(bytestToRead).Replace(System.Environment.NewLine, string.Empty);

                    string actualdata = Regex.Replace(str, @"[^\t\r\n -~]", "_").RemoveWhitespace().Trim();
                    if (string.IsNullOrWhiteSpace(actualdata)) return;

                    //if (actualdata.StartsWith("_") || actualdata.EndsWith("_"))
                    //{
                    //    actualdata = filterString(actualdata);
                    //}

                    //if (!actualdata.EndsWith("m"))
                    //{
                    //    actualdata = actualdata.EndsWith("m") == false ? actualdata.Remove(actualdata.Length - 1, 1) : actualdata;
                    //    actualdata = filterString(actualdata);
                    //}

                    string[] data = actualdata.Split('_');

                    var lastitem = data[data.Length - 2];
                    var outP = lastitem.ToLower().ToString();

                    if (!string.IsNullOrEmpty(outP))
                    {
                        Regex re = new Regex(@"\d+");
                        Match m = re.Match(outP);
                        decimal balance = Convert.ToDecimal(m.Value);
                        _dispathcer.Invoke(new Action(() =>
                        {
                            ReadingsLog.Text = balance.ToString();
                        }));

                        if (hMIViewModel.Zero <= 0)
                        {
                            _dispathcer.Invoke(new Action(() =>
                            {
                                MessageLog.Text = "Reading weight..";

                            }));
                            hMIViewModel.Zero = balance;
                            _dispathcer.Invoke(new Action(() =>
                            {
                                MessageLog.Text = "Zero value has been set, Now put some weight on the scale and enter the actual weight in span to set calibration factor.";
                            }));
                            return;
                        }

                        if (hMIViewModel.Span <= 0)
                        {
                            _dispathcer.Invoke(new Action(() =>
                            {
                                MessageLog.Text = "Please put some weight on the scale and set span value for calibrations calculation...";
                            }));
                            return;
                        }

                        if (hMIViewModel.CalculateSpan == true && hMIViewModel.Span > 0)
                        {
                            if(LastReceivedVal<=0)
                            {
                                LastReceivedVal = balance;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    MessageLog.Text = "Waiting for the peak value..";

                                }));
                                return;
                            }

                            if(LastReceivedVal!=balance)
                            {
                                LastReceivedVal = balance;
                                return;
                            }


                            decimal diff = balance - hMIViewModel.Zero;
                            decimal divident = diff / hMIViewModel.Span;
                            hMIViewModel.Factor = Math.Round(divident,2);
                            hMIViewModel.CalculateSpan = false;
                            LastReceivedVal = 0;
                            _dispathcer.Invoke(new Action(() =>
                            {
                                MessageLog.Text = "Calibration completed. Now Put some known weight to check the calibration accuracy.";
                            }));
                            //hMIViewModel.Weight = 0;
                            return;
                        }
                        if (hMIViewModel.Factor <= 0)
                        {
                            _dispathcer.Invoke(new Action(() =>
                            {
                                MessageLog.Text = "calibrations is zero, Please enter some weight to calculate calibration.";
                            }));
                            return;
                        }

                        if (hMIViewModel.Weight <= 0)
                        {
                            _dispathcer.Invoke(new Action(() =>
                            {
                                MessageLog.Text = "Calibration completed, Please close the window...";
                            }));
                        }

                        decimal weight = 0;

                        foreach (string arr in data)
                        {
                            if (string.IsNullOrWhiteSpace(arr)) continue;

                            Match mm = re.Match(arr.Trim());
                            decimal b= Convert.ToDecimal(mm.Value);
                            weight = b - hMIViewModel.Zero;
                            weight = weight / hMIViewModel.Factor;

                            if (weight < 0) continue;

                            hMIViewModel.Weight = Math.Round(weight, 2);
                        }

                        
                        
                       

                        //if (LastReceivedVal <= 0)
                        //{
                        //    LastReceivedVal = balance;
                        //    _dispathcer.Invoke(new Action(() =>
                        //    {
                        //        MessageLog.Text = "Waiting for the peak value..";

                        //    }));
                        //    return;
                        //}

                        //if (LastReceivedVal != balance)
                        //{
                        //    LastReceivedVal = balance;
                        //    return;
                        //}

                        //hMIViewModel.Weight = Math.Round(weight, 2);
                        //LastReceivedVal = 0;

                    }

                }
            }
            catch (Exception ex)
            {
                //string log = "An error has occured:\r" + ex.StackTrace.ToString() + ".";
                //log = log + "\r\n error description : " + ex.ToString();
                //LogWriter.LogWrite(log, sessionId);
            }


        }

        public string filterString(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return filter;
            while (filter.StartsWith("_") || filter.EndsWith("_"))
            {
                filter = filter.StartsWith("_") == true ? filter.Substring(1) : filter;
                filter = filter.EndsWith("_") == true ? filter.Remove(filter.Length - 1, 1) : filter;
            }
            return filter;
        }
        #endregion

        private void STARTCalibration_Click(object sender, RoutedEventArgs e)
        {
            hMIViewModel.IsNotRunning = false;
            hMIViewModel.IsRunning = true;
            _dispathcer.Invoke(new Action(() =>
            {
                MessageLog.Text = "Starting Calibration..";
            }));
            ConnectWeight(weighing.PortName, weighing.BaudRate, weighing.DataBit, weighing.StopBit, weighing.Parity);
        }

        private void STOPCalibration_Click(object sender, RoutedEventArgs e)
        {
            StopPortCommunication();
            Close();
        }
    }
}
