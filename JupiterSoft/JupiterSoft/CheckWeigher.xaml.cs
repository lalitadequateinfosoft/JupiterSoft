using Components;
using JupiterSoft.Annotations;
using JupiterSoft.CustomDailog;
using JupiterSoft.Models;
using JupiterSoft.ViewModel;
using Ozeki.Camera;
using Ozeki.Media;
using Ozeki;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace JupiterSoft
{
    /// <summary>
    /// Interaction logic for CheckWeigher.xaml
    /// </summary>
    public partial class CheckWeigher : Window, INotifyPropertyChanged
    {
        #region camrea vairables
        string _VideoDirectory = ApplicationConstant._VideoDirectory;
        BrushConverter bc;
        private IIPCamera _camera;
        private DrawingImageProvider drawingImageProvider;
        public DrawingImageProvider _drawingImageProvider
        {
            get => drawingImageProvider;
            set
            {
                OnPropertyChanged(nameof(_drawingImageProvider));
                drawingImageProvider = value;
            }
        }
        private MediaConnector _connector;
        private IWebCamera _webCamera;
        private static string _runningCamera = null;
        private MJPEGStreamer _streamer;
        private IVideoSender _videoSender;
        private Dispatcher _dispathcer;
        private MPEG4Recorder _mpeg4Recorder;
        #endregion

        #region Device Variables
        ModbusConfiguration modbus;
        UartDeviceConfiguration weight;
        ModbusConfiguration MotorDrive;
        #endregion

        #region function Variable
        const int REC_BUF_SIZE = 10000;
        byte[] recBuf = new byte[REC_BUF_SIZE];
        byte[] recBufParse = new byte[REC_BUF_SIZE];

        public Byte[] _MbTgmBytes;
        internal UInt32 TotalReceiveSize = 0;
        bool IsComplete = false;
        private int readIndex = 0;
        #endregion

        CheckWeigherViewModel weigherViewModel;
        private string _videofiledirectory;

        public string VideoFileDirectory
        {
            get
            {
                return _videofiledirectory;
            }
            set
            {
                _videofiledirectory = value;
            }
        }

        private List<DiscoveredDeviceInfo> devices;
        private List<DeviceModel> DeviceModels;
        private DeviceInfo deviceInfo;
        public CheckWeigher()
        {
            weigherViewModel = new CheckWeigherViewModel();
            InitializeComponent();
            this.StateChanged += new EventHandler(Window_StateChanged);
            RefreshMaximizeRestoreButton();
            DataContext = weigherViewModel;
            _dispathcer = Dispatcher.CurrentDispatcher;
            VideoFileDirectory = _VideoDirectory;
            if (!Directory.Exists(VideoFileDirectory))
            {
                Directory.CreateDirectory(VideoFileDirectory);
            }
            _drawingImageProvider = new DrawingImageProvider();
            _connector = new MediaConnector();
            modbus = new ModbusConfiguration();
            weight = new UartDeviceConfiguration();
            MotorDrive = new ModbusConfiguration();

            devices = new List<DiscoveredDeviceInfo>();
            DeviceModels = new List<DeviceModel>();

            deviceInfo = DeviceInformation.GetConnectedDevices();

        }


        private void LoadSytem()
        {
            //Camera
            ConnectionUSB();
            if (this.DataContext is CheckWeigherViewModel model)
            {
                if (!weight.IsConfigured)
                {
                    ConfigurationDailog dailog = new ConfigurationDailog("Set Weight Device Configuration", deviceInfo.CustomDeviceInfos);
                    dailog.ShowDialog();
                    if (!dailog.Canceled)
                    {

                        weight.PortName = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == dailog.DeviceId).FirstOrDefault().PortName;
                        weight.DeviceId = dailog.DeviceId;
                        weight.BaudRate = dailog.BaudRate;
                        weight.DataBit = dailog.Databit;
                        weight.StopBit = dailog.Stopbit;
                        weight.Parity = dailog.ParityValue;

                        RangeAndUnit range = new RangeAndUnit();
                        range.ShowDialog();
                        if (!range.Canceled)
                        {
                            weight.minRange = Convert.ToDecimal(range.MinimumRange.Text);
                            weight.maxRange = Convert.ToDecimal(range.MaxRange.Text);
                            weight.selectedUnit = range.unit.SelectionBoxItem.ToString();
                            unit.SelectedValue = weight.selectedUnit;

                            //CalibrationDailog calibration = new CalibrationDailog();
                            //calibration.ShowDialog();
                            //if(!calibration.Canceled)
                            //{
                            //    weight.calibrations = calibration.calibrations;
                            //}
                        }

                        weight.IsConfigured = true;
                        model.IsWeightConfigured = true;
                    }
                }

                if (!modbus.IsConfigured)
                {
                    ConfigurationDailog dailog = new ConfigurationDailog("Set Control Card Configuration", deviceInfo.CustomDeviceInfos);
                    dailog.ShowDialog();
                    if (!dailog.Canceled)
                    {

                        modbus.PortName = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == dailog.DeviceId).FirstOrDefault().PortName;
                        modbus.DeviceId = dailog.DeviceId;
                        modbus.BaudRate = dailog.BaudRate;
                        modbus.DataBit = dailog.Databit;
                        modbus.StopBit = dailog.Stopbit;
                        modbus.Parity = dailog.ParityValue;

                        modbus.MotorDrive = new RegisterConfiguration
                        {
                            RType = 0,
                            DeviceType = (int)Module_Device_Type.MotorDrive,
                            RegisterNo = 1,
                            Frequency = 0,
                            Count = 0
                        };
                        modbus.PushingArm = new RegisterConfiguration
                        {
                            RType = 0,
                            DeviceType = (int)Module_Device_Type.PushingArm,
                            RegisterNo = 2,
                            Frequency = 0,
                            Count = 0,
                        };
                        modbus.Sensor = new RegisterConfiguration
                        {
                            RType = 1,
                            DeviceType = (int)Module_Device_Type.Sensor,
                            RegisterNo = 1,
                            Frequency = 0,
                            Count = 0,
                        };

                        modbus.IsConfigured = true;
                        model.IsMotorConfigured = true;
                        model.IsPhotoConfigured = true;
                        model.IsPushingConfigured = true;
                    }
                }
            }

        }


        #region custom window functions

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "you want to exit", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                DisconnectCamera();
                this.Close();
            }

        }

        private void RefreshMaximizeRestoreButton()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.maximizeButton.Visibility = Visibility.Collapsed;
                this.restoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.maximizeButton.Visibility = Visibility.Visible;
                this.restoreButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            this.RefreshMaximizeRestoreButton();
        }



        #endregion

        #region Camera Function


        private bool ConnectionUSB()
        {
            bool isStarted = false;
            try
            {


                //_webCamera = new WebCamera();
                _webCamera = WebCameraFactory.GetDefaultDevice();

                if (_webCamera != null)
                {

                    _connector.Connect(_webCamera.VideoChannel, drawingImageProvider);

                    //_webCamera.StateChanged += _webCamera_StateChanged;

                    videoViewer.SetImageProvider(drawingImageProvider);

                    _webCamera.Start();

                    videoViewer.Start();

                    _videoSender = _webCamera.VideoChannel;

                    isStarted = true;


                    _runningCamera = "USB";

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Camera failed to start");
            }
            return isStarted;
        }

        private void DisconnectCamera()
        {

            DisconnectUSBCamera();
        }

        private void DisconnectUSBCamera()
        {
            if (_webCamera.Capturing)
            {
                StopVideoCapture();
                videoViewer.Stop();
                videoViewer.Background = Brushes.Black;
                _webCamera.Stop();
                _webCamera.Dispose();
                drawingImageProvider.Dispose();
                _connector.Disconnect(_webCamera.VideoChannel, drawingImageProvider);
                _connector.Dispose();
            }

        }

        private void _mpeg4Recorder_MultiplexFinished(object sender, VoIPEventArgs<bool> e)
        {
            _connector.Disconnect(_webCamera.AudioChannel, _mpeg4Recorder.AudioRecorder);
            _connector.Disconnect(_webCamera.VideoChannel, _mpeg4Recorder.VideoRecorder);
            _mpeg4Recorder.Dispose();
        }

        private string StartVideoCapture(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var date = DateTime.Now.Year + "y-" + DateTime.Now.Month + "m-" + DateTime.Now.Day + "d-" +
                       DateTime.Now.Hour + "h-" + DateTime.Now.Minute + "m-" + DateTime.Now.Second + "s";
            string currentpath = path + "\\" + date + ".mp4";

            _mpeg4Recorder = new MPEG4Recorder(currentpath);
            _mpeg4Recorder.MultiplexFinished += _mpeg4Recorder_MultiplexFinished;
            _connector.Connect(_webCamera.AudioChannel, _mpeg4Recorder.AudioRecorder);
            _connector.Connect(_webCamera.VideoChannel, _mpeg4Recorder.VideoRecorder);

            return currentpath;

        }

        private void StopVideoCapture()
        {
            if (_mpeg4Recorder != null)
            {
                _mpeg4Recorder.Multiplex();
                _connector.Disconnect(_webCamera.AudioChannel, _mpeg4Recorder.AudioRecorder);
                _connector.Disconnect(_webCamera.VideoChannel, _mpeg4Recorder.VideoRecorder);
            }
        }

        #endregion

        #region property changed event

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region HMI Command

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CheckWeigherViewModel model)
            {
                if (!model.IsWeightConfigured)
                {
                    MessageBox.Show("Please configure weight device.");
                    LoadSytem();
                    return;
                }
                if (!model.IsPushingConfigured)
                {
                    MessageBox.Show("Please configure pushing Arm.");
                    LoadSytem();
                    return;
                }
                if (!model.IsMotorConfigured)
                {
                    MessageBox.Show("Please configure motor drive.");
                    LoadSytem();
                    return;
                }
                if (!model.IsPhotoConfigured)
                {
                    MessageBox.Show("Please configure photo eye.");
                    LoadSytem();
                    return;
                }

                //Execute Commands.
                ExecuteLogic();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Under Maintenance..");
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            StopPortCommunication((int)Models.Module_Device_Type.Weight);
            StopPortCommunication((int)Models.Module_Device_Type.MotorDrive);
        }

        #endregion

        #region Command Logics
        private void ExecuteLogic()
        {
            Connect_control_card(modbus.PortName, modbus.BaudRate, modbus.DataBit, modbus.StopBit, modbus.Parity);
            ConnectWeight(weight.PortName, weight.BaudRate, weight.DataBit, weight.StopBit, weight.Parity);
        }

        double getWeightModuleResponse(RecData _recData)
        {
            double weight = 0;

            if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
            {
                byte[] bytestToRead = _recData.MbTgm.Skip(readIndex).ToArray();
                string str = Encoding.Default.GetString(bytestToRead).Replace(System.Environment.NewLine, string.Empty);

                string actualdata = Regex.Replace(str, @"[^\t\r\n -~]", "_").RemoveWhitespace().Trim();
                string[] data = actualdata.Split('_');

                var lastitem = data[data.Length - 1];
                var outP = lastitem.ToLower().ToString();




                if (outP.Contains("kg"))
                {
                    if (outP.Contains("kgg"))
                    {
                        string builder = outP.Replace("kgg", "");
                        weight = Convert.ToInt32(builder) - 3067;
                        weight = Convert.ToDouble(weight / 260.4);
                        weight = weight * 0.453592;
                        weight = Math.Round(weight, 2);
                    }
                    else if (outP.Contains("kgn"))
                    {
                        string builder = outP.Replace("kgn", "");
                        weight = Convert.ToInt32(builder) - 3067;
                        weight = Convert.ToDouble(weight / 260.4);
                        weight = weight * 0.453592;
                        weight = Math.Round(weight, 2);
                    }
                    else
                    {
                        string builder = outP.Replace("kg", "");
                        weight = Convert.ToInt32(builder) - 3067;
                        weight = Convert.ToDouble(weight / 260.4);
                        weight = weight * 0.453592;
                        weight = Math.Round(weight, 2);
                    }

                }
                else if (outP.Contains("lbs"))
                {

                    Regex regex = new Regex(@"([a-zA-Z]+)(\d+)");
                    Match result = regex.Match(outP);

                    string alphaPart1 = result.Groups[1].Value;
                    string numberPart1 = result.Groups[2].Value;
                    weight = Convert.ToInt32(numberPart1) - 3067;
                    weight = Convert.ToDouble(weight / 260.4);
                    weight = Math.Round(weight, 2);
                    //weight = weight * 0.453592;
                }
                else
                {
                    Regex regex = new Regex(@"([a-zA-Z]+)(\d+)");
                    Match result = regex.Match(outP);

                    string alphaPart1 = result.Groups[1].Value;
                    string numberPart1 = result.Groups[2].Value;
                    weight = Convert.ToInt32(numberPart1) - 3067;
                    weight = Convert.ToDouble(weight / 260.4);
                    weight = Math.Round(weight, 2);
                }

            }
            return weight; //TimerCheckReceiveData.Enabled = true;
        }

        private void ConnectWeight(string Port, int Baudrate, int databit, int stopbit, int parity)
        {
            try
            {
                if (weight == null || weight.IsTurnedOn == false)
                {

                    weight.RecState = 1;
                    RecData _recData = new RecData();
                    _recData.SessionId = Common.GetSessionNewId;
                    _recData.Ch = 0;
                    _recData.Indx = 0;
                    _recData.Reg = 0;
                    _recData.NoOfVal = 0;
                    _recData.Status = PortDataStatus.Requested;
                    weight.CurrentRequest = _recData;
                    SerialPort WeightDevice = new SerialPort();
                    SerialPortCommunications(ref WeightDevice, (int)Models.Module_Device_Type.Weight, Port, Baudrate, databit, stopbit, parity);
                    weight.SerialDevice = WeightDevice;
                    weight.IsTurnedOn = true;
                }
            }
            catch
            {

            }
        }

        private void Connect_control_card(string Port, int Baudrate, int databit, int stopbit, int parity)
        {

            try
            {

                if (modbus == null)
                {

                    modbus.RecState = 1;
                    RecData _recData = new RecData();
                    _recData.SessionId = Common.GetSessionNewId;
                    _recData.Ch = 0;
                    _recData.Indx = 0;
                    _recData.Reg = 0;
                    _recData.NoOfVal = 0;
                    _recData.Status = PortDataStatus.Requested;
                    _recData.RqType = (int)Models.Module_Device_Type.MotorDrive;
                    modbus.CurrentRequest = _recData;
                    modbus.RecState = 0;
                    SerialPort WeightDevice = new SerialPort();
                    SerialPortCommunications(ref WeightDevice, (int)Models.Module_Device_Type.MotorDrive, Port, Baudrate, databit, stopbit, parity);
                    modbus.SerialDevice = WeightDevice;

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ControlDataReader()
        {

            if (modbus.RecState > 0 && modbus.IsComplete)
            {
                modbus.IsComplete = false;


                //recState = 1;
                while (modbus.ReceiveBufferQueue.Count > 0)
                {
                    recBufParse = modbus.ReceiveBufferQueue.Dequeue();

                    modbus.RecState = 1;
                    SB1Reply _reply = new SB1Reply(Common.GetSessionId);
                    SB1Handler _hndl = new SB1Handler(modbus.SerialDevice);
                    UInt32 length = 32;
                    UInt32 payLoadSize = 0;
                    Byte[] payload;
                    Byte[] RxSB1 = recBufParse;
                    Byte MbAck;
                    UInt16 MbLength;
                    UInt16 Reserved;
                    Byte[] mbTgmBytes;


                    length = (uint)recBufParse[2] + 5;
                    payLoadSize = 0;
                    RxSB1 = recBufParse;
                    RecData _recData = modbus.CurrentRequest;

                    //if (_reply.CheckCrc(recBufParse, Convert.ToInt32(_reply.length)))  // SB1 Check CRC
                    //{

                    if (_recData != null)
                    {
                        //ExtractPayload
                        Byte[] _payload = new Byte[1000];
                        Array.Copy(RxSB1, 30, _payload, 0, payLoadSize);
                        payload = _payload;

                        //Set payloadrs
                        MbAck = (Byte)payload[0];
                        MbLength = Util.ByteArrayConvert.ToUInt16(payload, 1);
                        Reserved = Util.ByteArrayConvert.ToUInt16(payload, 3);


                        //extract modbus tgm
                        Byte[] _MbTgm = new Byte[1000];
                        Array.Copy(_payload, 5, _MbTgm, 0, MbLength);
                        mbTgmBytes = _MbTgm;


                        _MbTgmBytes = RxSB1;
                        MbLength = (ushort)_reply.length;
                        if (_MbTgmBytes != null && MbLength > 0)
                        {
                            bool _IsTgmErr = false;
                            _IsTgmErr = CheckTgmError(_recData, _payload, _MbTgmBytes, MbLength);
                            if (_IsTgmErr)
                            {
                                if (_recData.RqType == RQType.WireLess)
                                {
                                    mbTgmBytes = payload;
                                }
                                else
                                {
                                    mbTgmBytes = _MbTgmBytes;
                                }

                                modbus.CurrentRequest.MbTgm = mbTgmBytes;
                                modbus.CurrentRequest.Status = PortDataStatus.Received;

                                return;

                            }
                        }

                    }
                    else
                    {
                        if (_recData != null)
                        {
                            _recData.Status = PortDataStatus.Normal;
                            if (_recData.SessionId > 0)
                            {

                                modbus.CurrentRequest = _recData;
                            }

                        }
                    }



                    //}

                }

            }

        }

        private void UartDataReader()
        {

            if (weight.RecState > 0 && weight.IsComplete)
            {
                weight.IsComplete = false;


                //recState = 1;
                while (weight.ReceiveBufferQueue.Count > 0)
                {
                    recBufParse = weight.ReceiveBufferQueue.Dequeue();

                    weight.RecState = 1;
                    RecData _recData = weight.CurrentRequest;

                    if (_recData != null)
                    {

                        Common.GoodTmgm++;

                        weight.CurrentRequest.MbTgm = recBufParse;
                        weight.CurrentRequest.Status = PortDataStatus.Received;

                        return;
                    }
                    else
                    {
                        if (_recData != null)
                        {
                            _recData.Status = PortDataStatus.Normal;

                            weight.CurrentRequest = _recData;
                        }
                    }

                }

            }

        }



        private bool CheckTgmError(RecData _recData, Byte[] _payLoad, Byte[] _MBTgm, int _MbLength)
        {
            bool _IsTgmErr = false;
            if (_recData != null)
            {
                switch (_recData.RqType)
                {
                    case RQType.ModBus:
                        ModBus_Ack _Ack = MbTgm.GetModBusAck(_payLoad);
                        bool _IsOk = CrcModbus.CheckCrc(_MBTgm, _MbLength);
                        if (!_IsOk)
                        {
                            _Ack = ModBus_Ack.CrcFault;
                        }

                        if (_Ack == ModBus_Ack.OK)  // MobBus TgmCRC Check
                        {
                            //if (_recData.deviceType != Models.DeviceType.MotorDerive)
                            //{
                            _IsTgmErr = true;
                            //ToolTipStatus = ComStatus.OK;
                            Common.GoodTmgm++;
                            //}
                        }
                        else if (_Ack == ModBus_Ack.CrcFault)
                        {
                            _IsTgmErr = false;
                            //ToolTipStatus = ComStatus.CRC;
                            Common.CRCFaults++;
                        }
                        else if (_Ack == ModBus_Ack.Timeout)
                        {
                            _IsTgmErr = false;
                            //ToolTipStatus = ComStatus.TimeOut;
                            Common.TimeOuts++;
                        }
                        else
                        {
                            _IsTgmErr = false;
                            //ToolTipStatus = ComStatus.CRC;
                            Common.CRCFaults++;
                        }
                        break;

                }
            }
            return _IsTgmErr;
        }
        #endregion

        #region Port Communication
        private void SerialPortCommunications(ref SerialPort serialPort, int TypeOfDevice = 0, string port = "", int baudRate = 0, int databit = 0, int stopBit = 0, int parity = 0)
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
                switch (TypeOfDevice)
                {
                    case (int)Models.Module_Device_Type.Weight:
                        serialPort.DataReceived += WeightDevice_DataReceived;
                        break;
                    default:
                        serialPort.DataReceived += ControlDevice_DataReceived;
                        break;
                }


                serialPort.Open();
            }
            catch (Exception ex)
            {

            }

        }

        private void StopPortCommunication(int device)
        {
            if (weight.SerialDevice.IsOpen && device == (int)Models.Module_Device_Type.Weight)
            {

                weight.SerialDevice.DtrEnable = false;
                weight.SerialDevice.RtsEnable = false;

                weight.SerialDevice.DataReceived -= WeightDevice_DataReceived;
                weight.SerialDevice.DiscardInBuffer();
                weight.SerialDevice.DiscardOutBuffer();
                weight.SerialDevice.Close();
            }
            else
            {
                modbus.SerialDevice.DtrEnable = false;
                modbus.SerialDevice.RtsEnable = false;

                modbus.SerialDevice.DataReceived -= ControlDevice_DataReceived;
                modbus.SerialDevice.DiscardInBuffer();
                modbus.SerialDevice.DiscardOutBuffer();
                modbus.SerialDevice.Close();
            }
        }

        private void WeightDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {


            if (weight == null)
            {
                return;
            }

            switch (weight.RecState)
            {
                case 0:
                    break;
                case 1:
                    int i = 0;
                    weight.RecIdx = 0;
                    weight.RecState = 1;
                    recBuf = new byte[weight.SerialDevice.BytesToRead];
                    weight.SerialDevice.Read(recBuf, 0, recBuf.Length);
                    weight.ReceiveBufferQueue = new Queue<byte[]>();
                    weight.ReceiveBufferQueue.Enqueue(recBuf);
                    weight.LastResponseReceived = DateTime.Now;
                    weight.IsComplete = true;
                    recBuf = new byte[REC_BUF_SIZE];
                    break;
            }

            UartDataReader();
        }

        private void ControlDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {


            if (modbus == null)
            {
                return;
            }
            switch (modbus.RecState)
            {
                case 0:
                    break;
                case 1:


                    int i = 0;
                    modbus.RecIdx = 0;
                    modbus.RecState = 2;


                    while (modbus.SerialDevice.BytesToRead > 0)
                    {
                        byte[] rec = new byte[1];
                        modbus.RecIdx += modbus.SerialDevice.Read(rec, 0, 1);
                        recBuf[i] = rec[0];
                        i++;
                    }

                    if (modbus.RecIdx > 3)
                    {
                        TotalReceiveSize = (uint)recBuf[2] + 5;
                    }
                    if (TotalReceiveSize > modbus.RecIdx)
                    {
                        modbus.IsComplete = false;
                    }
                    else
                    {
                        modbus.IsComplete = true;
                        modbus.ReceiveBufferQueue = new Queue<byte[]>();
                        modbus.ReceiveBufferQueue.Enqueue(recBuf);
                        recBuf = new byte[REC_BUF_SIZE];
                    }
                    modbus.LastResponseReceived = DateTime.Now;

                    break;
            }

            ControlDataReader();
        }
        #endregion
    }
}
