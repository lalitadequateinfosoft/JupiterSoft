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
using Util;
using System.Timers;

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
        private List<RegisterOutputStatus> registerOutputStatuses;
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

        System.Timers.Timer ExecutionTimer = new System.Timers.Timer();
        System.Timers.Timer PushingTimer = new System.Timers.Timer();
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
        private static readonly Regex _regex = new Regex("[^0-9.-]+");
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
            registerOutputStatuses = new List<RegisterOutputStatus>();
            deviceInfo = DeviceInformation.GetConnectedDevices();

        }


        private void LoadSytem()
        {

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

                            model.minRange = Convert.ToDecimal(range.MinimumRange.Text);
                            model.maxRange = Convert.ToDecimal(range.MaxRange.Text);
                            model.selectedUnit = range.unit.SelectionBoxItem.ToString();
                            weight.IsConfigured = true;
                            model.IsWeightConfigured = true;
                        }

                    }
                }

                if(!weight.IsCalibrated)
                {
                    CalibrationHMI calibrationHMI = new CalibrationHMI(weight);
                    calibrationHMI.ShowDialog();
                    weight.Zero = calibrationHMI.hMIViewModel.Zero;
                    weight.Factor = calibrationHMI.hMIViewModel.Factor;
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

                        ConnectivityInfo connectivityInfo = new ConnectivityInfo();
                        connectivityInfo.ShowDialog();

                        if(!connectivityInfo.Canceled)
                        {
                            modbus.slaveAddress = Convert.ToInt32(connectivityInfo.AddressBox.Text.ToString());


                            modbus.PushingArm = new RegisterConfiguration
                            {
                                RType = 0,
                                DeviceType = (int)Module_Device_Type.ControlCard,
                                RegisterNo = Convert.ToInt32(connectivityInfo.PushingArm.Text.ToString()),
                                Frequency = 30,
                                Count = 0,
                            };
                            modbus.Sensor = new RegisterConfiguration
                            {
                                RType = 1,
                                DeviceType = (int)Module_Device_Type.ControlCard,
                                RegisterNo = Convert.ToInt32(connectivityInfo.Sensors.Text.ToString()),
                                Frequency = 0,
                                Count = 0,
                            };

                            modbus.IsConfigured = true;
                            model.IsPhotoConfigured = true;
                            model.IsPushingConfigured = true;
                        }
                        
                    }
                }

                if (!MotorDrive.IsConfigured)
                {
                    ConfigurationDailog dailog = new ConfigurationDailog("Set Motor Drive Configuration", deviceInfo.CustomDeviceInfos);
                    dailog.ShowDialog();
                    if(!dailog.Canceled)
                    {
                        MotorDriveConnectivityInfo frequency = new MotorDriveConnectivityInfo();
                        frequency.ShowDialog();
                        if (!frequency.Canceled)
                        {
                            MotorDrive.MotorDrive = new RegisterConfiguration
                            {
                                RType = 0,
                                DeviceType = (int)Module_Device_Type.MotorDrive,
                                RegisterNo = Convert.ToInt32(frequency.MotorRegister.Text),
                                Frequency = Convert.ToDecimal(frequency.MotorFrequency.Text),
                                Count = 0
                            };
                            MotorDrive.slaveAddress = Convert.ToInt32(frequency.Addressbox.Text);
                            MotorDrive.IsConfigured = true;
                            model.IsMotorConfigured = true;
                        }
                        
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
            if (_webCamera == null) return;
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
                if(!model.IsWeightConfigured || !model.IsPushingConfigured || !model.IsMotorConfigured || !model.IsPhotoConfigured)
                { 
                    MessageBox.Show("Please configure Devices.");
                    LoadSytem();
                    MessageBox.Show("Please run again to start process..");
                    return;
                }

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
            StopPortCommunication((int)Models.Module_Device_Type.ControlCard);
            DisconnectCamera();
            StopTimer();
            EnableWindowControl();
        }

        #endregion

        #region Command Logics

        private void StartTimer()
        {
            ExecutionTimer.Elapsed += ExecutionTimer_Elapsed;
            ExecutionTimer.Interval = 30;
            ExecutionTimer.Enabled = true;
        }
        private void StopTimer()
        {
            ExecutionTimer.Interval = 0;
            ExecutionTimer.Elapsed -= ExecutionTimer_Elapsed;
            ExecutionTimer.Enabled = false;
        }

        private void ExecutionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReadAllControCardInputOutput();
        }

        private void ExecuteLogic()
        {
            //Camera
            DisableWindowControl();
            ConnectionUSB();
            StartVideoCapture(_VideoDirectory);
            ConnectWeight(weight.PortName, weight.BaudRate, weight.DataBit, weight.StopBit, weight.Parity);
            ConnectMotor(MotorDrive.PortName, MotorDrive.BaudRate, MotorDrive.DataBit, MotorDrive.StopBit, MotorDrive.Parity);
            Connect_control_card(modbus.PortName, modbus.BaudRate, modbus.DataBit, modbus.StopBit, modbus.Parity);
            StartTimer();
        }

        public void DisableWindowControl()
        {
            if (this.DataContext is CheckWeigherViewModel model)
            {
                model.IsWeightEditEnabled = false;
                model.IsWeightSaveEnabled = false;


                model.IsMotorEditEnabled = false;
                model.IsMotorSaveEnabled = false;


                model.IsPushingEditEnabled = false;
                model.IsPushingSaveEnabled = false;

                model.IsPaused = true;
                model.IsRunning = false;
            }

        }
        public void EnableWindowControl()
        {
            if (this.DataContext is CheckWeigherViewModel model)
            {
                model.IsWeightEditEnabled = true;
                model.IsWeightSaveEnabled = false;


                model.IsMotorEditEnabled = true;
                model.IsMotorSaveEnabled = false;


                model.IsPushingEditEnabled = true;
                model.IsPushingSaveEnabled = false;

                model.IsPaused = false;
                model.IsRunning = true;
            }

        }

        void CompareWeightModuleResponse(RecData _recData)
        {
            if (weight.Zero <= 0 || weight.Factor <= 0)
            {
                return;
            }

            if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
            {
                byte[] bytestToRead = _recData.MbTgm.Skip(readIndex).ToArray();
                string str = Encoding.Default.GetString(bytestToRead).Replace(System.Environment.NewLine, string.Empty);

                string actualdata = Regex.Replace(str, @"[^\t\r\n -~]", "_").RemoveWhitespace().Trim();
                string[] data = actualdata.Split('_');

                var lastitem = data[data.Length - 1];
                var outP = lastitem.ToLower().ToString();


                decimal Cweight = 0;

                if (!string.IsNullOrEmpty(outP))
                {
                    Regex re = new Regex(@"\d+");

                    foreach (string arr in data)
                    {
                        if (string.IsNullOrWhiteSpace(arr)) continue;

                        Match m = re.Match(outP);
                        decimal balance = Convert.ToDecimal(m.Value);
                        Cweight = balance - weight.Zero;
                        Cweight = Cweight / weight.Factor;

                        if (Cweight < 0) continue;

                        weigherViewModel.Weight = Math.Round(Cweight, 2);
                        if (!(weigherViewModel.Weight >= weight.minRange && weigherViewModel.Weight <= weight.maxRange))
                        {
                            PushingTimer.Elapsed += PushingTimer_Elapsed;
                            PushingTimer.Interval = 34 + Convert.ToInt32(modbus.PushingArm.Frequency);
                            PushingTimer.Enabled = true;
                        }
                    }
                   
                   
                }

            }
        }

        private void PushingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TurnOnPushingArm();
            PushingTimer.Interval = 0;
            PushingTimer.Elapsed -= PushingTimer_Elapsed;
            PushingTimer.Enabled = false;
            TurnOffPushing();
        }

        private void TurnOnPushingArm()
        {
            WriteControCardState(modbus.PushingArm.RegisterNo, 1, modbus.slaveAddress);
            modbus.PushingArm.Count = modbus.PushingArm.Count + 1;
            _dispathcer.Invoke(new Action(() =>
            {
                PushingOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                PushingOn.Stroke = Brushes.LightBlue;
                PushingOff.Fill = new SolidColorBrush(Colors.DarkGray);
                PushingOff.Stroke = Brushes.LightGray;
                PushCount.Content = modbus.PushingArm.Count.ToString();
            }));
        }

        private void TurnOffPushing()
        {
            _dispathcer.Invoke(new Action(() =>
            {
                PushingOn.Fill = new SolidColorBrush(Colors.DarkGray);
                PushingOn.Stroke = Brushes.LightGray;
                PushingOff.Fill = new SolidColorBrush(Colors.Red);
                PushingOff.Stroke = Brushes.LightCoral;
            }));
            WriteControCardState(modbus.PushingArm.RegisterNo, 0, modbus.slaveAddress);
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

        private void ConnectMotor(string Port, int Baudrate, int databit, int stopbit, int parity)
        {
            try
            {
                if (MotorDrive == null || MotorDrive.IsTurnedOn == false)
                {

                    MotorDrive.RecState = 1;
                    RecData _recData = new RecData();
                    _recData.SessionId = Common.GetSessionNewId;
                    _recData.Ch = 0;
                    _recData.Indx = 0;
                    _recData.Reg = 0;
                    _recData.NoOfVal = 0;
                    _recData.Status = PortDataStatus.Requested;
                    MotorDrive.CurrentRequest = _recData;
                    SerialPort WeightDevice = new SerialPort();
                    SerialPortCommunications(ref WeightDevice, (int)Models.Module_Device_Type.MotorDrive, Port, Baudrate, databit, stopbit, parity);
                    MotorDrive.SerialDevice = WeightDevice;
                    MotorDrive.IsTurnedOn = true;

                    //WriteMotorDriveState(MotorDrive.MotorDrive.RegisterNo, Convert.ToInt32(MotorDrive.MotorDrive.Frequency) * 100);
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
                    SerialPortCommunications(ref WeightDevice, (int)Models.Module_Device_Type.ControlCard, Port, Baudrate, databit, stopbit, parity);
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
                                ReadControlCardResponse(modbus.CurrentRequest);
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
                                ReadControlCardResponse(modbus.CurrentRequest);
                            }

                        }
                    }


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
                        CompareWeightModuleResponse(weight.CurrentRequest);
                        return;
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

        private void ReadAllControCardInputOutput()
        {

            modbus.RecState = 1;
            RecData _recData = new RecData();
            _recData.SessionId = Common.GetSessionNewId;
            _recData.Ch = 0;
            _recData.Indx = 0;
            _recData.Reg = 0;
            _recData.NoOfVal = 0;
            _recData.Status = PortDataStatus.Requested;
            modbus.CurrentRequest = _recData;
            modbus.IsComplete = false;

            MODBUSComnn obj = new MODBUSComnn();
            obj.GetMultiSendorValueFM3(modbus.slaveAddress, 0, modbus.SerialDevice, 0, 30, "ControlCard", 1, 0, Models.DeviceType.ControlCard);


        }

        private void ReadControCardState(int reg)
        {

            modbus.RecState = 1;
            RecData _recData = new RecData();
            _recData.SessionId = Common.GetSessionNewId;
            _recData.Ch = 0;
            _recData.Indx = 0;
            _recData.Reg = 0;
            _recData.NoOfVal = 0;
            _recData.Status = PortDataStatus.Requested;
            modbus.CurrentRequest = _recData;
            modbus.IsComplete = false;
            MODBUSComnn obj = new MODBUSComnn();
            obj.GetMultiSendorValueFM3(1, 0, modbus.SerialDevice, reg, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard);

        }

        private void WriteControCardState(int reg, int val,int slave)
        {

            modbus.RecState = 1;
            RecData _recData = new RecData();
            _recData.SessionId = Common.GetSessionNewId;
            _recData.Ch = 0;
            _recData.Indx = 0;
            _recData.Reg = 0;
            _recData.NoOfVal = 0;
            _recData.Status = PortDataStatus.Requested;
            modbus.CurrentRequest = _recData;
            modbus.IsComplete = false;

            MODBUSComnn obj = new MODBUSComnn();
            int[] _val = new int[2] { 0, val };
            obj.SetMultiSendorValueFM16(slave, 0, modbus.SerialDevice, reg + 1, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard, _val, false);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);


        }

        private void WriteMotorDriveState(int reg, int val)
        {

            MotorDrive.RecState = 1;
            RecData _recData = new RecData();
            _recData.SessionId = Common.GetSessionNewId;
            _recData.Ch = 0;
            _recData.Indx = 0;
            _recData.Reg = 0;
            _recData.NoOfVal = 0;
            _recData.Status = PortDataStatus.Requested;
            MotorDrive.CurrentRequest = _recData;
            MotorDrive.IsComplete = false;

            MODBUSComnn obj = new MODBUSComnn();
            byte[] _val1 = BitConverter.GetBytes(val);
            int[] _val = new int[2] { _val1[1], _val1[0] };
            obj.SetMultiSendorValueFM16(1, 0, MotorDrive.SerialDevice, reg + 1, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard, _val, false);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);


        }

        void ReadControlCardResponse(RecData _recData)
        {
            if (_recData.MbTgm != null)
            {
                if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
                {
                    //To Read Function Code response.
                    if (_recData.MbTgm[1] == (int)COM_Code.three)
                    {

                        int _i0 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 3);
                        int _i1 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 5);
                        int _i2 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 7);
                        int _i3 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 9);
                        int _i4 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 11);
                        int _i5 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 13);
                        int _i6 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 15);
                        int _i7 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 17);
                        int _i8 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 19);
                        int _i9 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 21);
                        int _i10 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 23);
                        int _i11 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 25);
                        int _i12 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 27);
                        int _i13 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 29);
                        int _i14 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 31);
                        int _i15 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 33);
                        int _i16 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 35);
                        int _i17 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 37);
                        int _i18 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 39);
                        int _i19 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 41);
                        int _i20 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 43);
                        int _i21 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 45);
                        int _i22 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 47);
                        int _i23 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 49);
                        int _i24 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 51);
                        int _i25 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 53);
                        int _i26 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 55);
                        int _i27 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 57);
                        int _i28 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 59);
                        int _i29 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 61);


                        if(modbus.Sensor.RegisterNo==1)
                        {
                            if (_i0 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 2)
                        {
                            if (_i1 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 3)
                        {
                            if (_i2 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 4)
                        {
                            if (_i3 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 5)
                        {
                            if (_i4 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 6)
                        {
                            if (_i5 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 7)
                        {
                            if (_i6 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 8)
                        {
                            if (_i7 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 9)
                        {
                            if (_i8 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 10)
                        {
                            if (_i9 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 11)
                        {
                            if (_i10 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 12)
                        {
                            if (_i11 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 13)
                        {
                            if (_i12 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 14)
                        {
                            if (_i13 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 15)
                        {
                            if (_i14 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }
                        else if (modbus.Sensor.RegisterNo == 16)
                        {
                            if (_i15 == 0)
                            {
                                if (modbus.Sensor.ObjectIntervals != null && modbus.Sensor.ObjectIntervals.Count() > 0)
                                {
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                else
                                {
                                    modbus.Sensor.ObjectIntervals = new List<DateTime>();
                                    modbus.Sensor.ObjectIntervals.Add(DateTime.Now);
                                }
                                modbus.Sensor.Count = modbus.Sensor.Count + 1;
                                modbus.MotorDrive.Count = modbus.MotorDrive.Count + 1;
                                weight.count = weight.count + 1;
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DodgerBlue);
                                    SensorOn.Stroke = Brushes.LightBlue;
                                    SensorOff.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOff.Stroke = Brushes.LightGray;
                                    MotorCount.Content = modbus.MotorDrive.Count.ToString();
                                }));
                                UartDataReader();
                            }
                            else
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    SensorOn.Fill = new SolidColorBrush(Colors.DarkGray);
                                    SensorOn.Stroke = Brushes.LightGray;
                                    SensorOff.Fill = new SolidColorBrush(Colors.Red);
                                    SensorOff.Stroke = Brushes.LightCoral;
                                }));
                            }
                        }



                    }
                }
            }
        }



        void GetControlCardInputOutputState(RecData _recData)
        {
            if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
            {
                //To Read Function Code response.
                if (_recData.MbTgm[1] == (int)COM_Code.three)
                {

                    int _i0 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 3);
                    int _i1 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 5);
                    int _i2 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 7);
                    int _i3 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 9);
                    int _i4 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 11);
                    int _i5 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 13);
                    int _i6 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 15);
                    int _i7 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 17);
                    int _i8 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 19);
                    int _i9 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 21);
                    int _i10 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 23);
                    int _i11 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 25);
                    int _i12 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 27);
                    int _i13 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 29);
                    int _i14 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 31);
                    int _i15 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 33);
                    int _i16 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 35);
                    int _i17 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 37);
                    int _i18 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 39);
                    int _i19 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 41);
                    int _i20 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 43);
                    int _i21 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 45);
                    int _i22 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 47);
                    int _i23 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 49);
                    int _i24 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 51);
                    int _i25 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 53);
                    int _i26 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 55);
                    int _i27 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 57);
                    int _i28 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 59);
                    int _i29 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 61);
                }

            }
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
                    case (int)Models.Module_Device_Type.ControlCard:
                        serialPort.DataReceived += ControlDevice_DataReceived;
                        break;
                    case (int)Models.Module_Device_Type.MotorDrive:
                        serialPort.DataReceived += MotorDrive_DataReceived;
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
            else if (modbus.SerialDevice.IsOpen && device == (int)Models.Module_Device_Type.ControlCard)
            {
                modbus.SerialDevice.DtrEnable = false;
                modbus.SerialDevice.RtsEnable = false;

                modbus.SerialDevice.DataReceived -= ControlDevice_DataReceived;
                modbus.SerialDevice.DiscardInBuffer();
                modbus.SerialDevice.DiscardOutBuffer();
                modbus.SerialDevice.Close();
            }
            else if (MotorDrive.SerialDevice.IsOpen && device == (int)Models.Module_Device_Type.MotorDrive)
            {
                MotorDrive.SerialDevice.DtrEnable = false;
                MotorDrive.SerialDevice.RtsEnable = false;

                MotorDrive.SerialDevice.DataReceived -= MotorDrive_DataReceived;
                MotorDrive.SerialDevice.DiscardInBuffer();
                MotorDrive.SerialDevice.DiscardOutBuffer();
                MotorDrive.SerialDevice.Close();
            }
        }

        private void MotorDrive_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {


            if (MotorDrive == null)
            {
                return;
            }

            switch (MotorDrive.RecState)
            {
                case 0:
                    break;
                case 1:
                    int i = 0;
                    MotorDrive.RecIdx = 0;
                    MotorDrive.RecState = 2;
                    while (MotorDrive.SerialDevice.BytesToRead > 0)
                    {
                        byte[] rec = new byte[1];
                        MotorDrive.RecIdx += MotorDrive.SerialDevice.Read(rec, 0, 1);
                        recBuf[i] = rec[0];
                        i++;
                    }

                    if (MotorDrive.RecIdx > 3)
                    {

                        TotalReceiveSize = (uint)recBuf[2] + 5;
                    }
                    if (TotalReceiveSize > MotorDrive.RecIdx)
                    {
                        MotorDrive.IsComplete = false;
                    }
                    else
                    {
                        MotorDrive.IsComplete = true;
                        MotorDrive.ReceiveBufferQueue.Enqueue(recBuf);
                    }
                   
                    MotorDrive.LastResponseReceived = DateTime.Now;
                    recBuf = new byte[REC_BUF_SIZE];
                    break;
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

        private void Wedit_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CheckWeigherViewModel model)
            {
                model.IsWeightEditEnabled = false;
                model.IsWeightSaveEnabled = true;
            }
        }

        private void Wsave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(minrange.Text) || minrange.Text.Contains(" ") || _regex.IsMatch(minrange.Text))
            {
                MessageBox.Show("Please enter valid minimum range.");
                minrange.Text = weight.minRange.ToString();
                return;
            }

            if (string.IsNullOrEmpty(maxrange.Text) || maxrange.Text.Contains(" ") || _regex.IsMatch(maxrange.Text))
            {
                MessageBox.Show("Please enter valid maximum range.");
                maxrange.Text = weight.maxRange.ToString();
                return;
            }

            if (string.IsNullOrEmpty(unit.SelectedValue.ToString()))
            {
                MessageBox.Show("Please select a valid unit.");
                unit.SelectedValue = weight.selectedUnit;
                return;
            }

            if (this.DataContext is CheckWeigherViewModel model)
            {
               
                model.IsWeightEditEnabled = true;
                model.IsWeightSaveEnabled = false;
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            var dashForm = new MainWindow();
            dashForm.Show();
            //dashForm.Show();
            this.Close();
        }
    }
}
