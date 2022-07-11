using Components;
using JupiterSoft.Models;
using Newtonsoft.Json;
using Ozeki;
using Ozeki.Camera;
using Ozeki.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace JupiterSoft.CustomDailog
{
    /// <summary>
    /// Interaction logic for HMIDialoge.xaml
    /// </summary>
    public partial class HMIDialoge : Window
    {
        #region Function Variablle
        private bool write = false;
        const int REC_BUF_SIZE = 10000;
        byte[] recBuf = new byte[REC_BUF_SIZE];
        byte[] recBufParse = new byte[REC_BUF_SIZE];

        public Byte[] _MbTgmBytes;
        internal UInt32 TotalReceiveSize = 0;
        bool IsComplete = false;


        System.Timers.Timer TimerCheckReceiveData = new System.Timers.Timer();
        internal bool _UpdatePB = true;
        internal bool UseThisPage = true;
        static readonly object _object = new object();
        string _CurrentActiveMenu = "Modbus";
        string _FileDirectory = ApplicationConstant._FileDirectory;
        string _VideoDirectory = ApplicationConstant._VideoDirectory;
        BrushConverter bc;
        private IIPCamera _camera;
        private DrawingImageProvider _drawingImageProvider;
        private MediaConnector _connector;
        private IWebCamera _webCamera;
        private static string _runningCamera = null;
        private MJPEGStreamer _streamer;
        private IVideoSender _videoSender;
        private List<DiscoveredDeviceInfo> devices;
        private List<DeviceModel> DeviceModels;
        private DeviceInfo deviceInfo;
        private SerialPort SerialDevice;
        private Dispatcher _dispathcer;
        private int readIndex = 0;
        private MPEG4Recorder _mpeg4Recorder;

        private List<LogicalCommand> Commands;
        #endregion

        public bool IsClosed { get; set; }
        public HMIDialoge(List<LogicalCommand> _Commands, DeviceInfo _deviceInfo)
        {
            InitializeComponent();
            _dispathcer = Dispatcher.CurrentDispatcher;
            this.SerialDevice = new SerialPort();
            this.Commands = _Commands;
            this.Commands.ForEach(x => x.ExecutionStatus = (int)ExecutionStage.Not_Executed);
            this.deviceInfo = _deviceInfo;

        }

        private void StopProcess_Click(object sender, RoutedEventArgs e)
        {
            IsClosed = true;
            Close();
        }

        #region Camera Function
        private void StartCameraRecording()
        {

            try
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    if (!string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        StartVideoCapture(dialog.SelectedPath);
                    }

                }
            }
            catch
            {
                // MessageBox.Show("An error has occured. Recording will be saved at default location : " + _VideoDirectory);
                StartVideoCapture(_VideoDirectory);
            }
        }

        private void StopCameraRecording()
        {
            StopVideoCapture();
        }

        private void ConnectionUSB()
        {
            try
            {
                //DisconnectRunningCamera();
                _webCamera = new WebCamera();
                _drawingImageProvider = new DrawingImageProvider();
                _connector = new MediaConnector();

                _webCamera = WebCameraFactory.GetDefaultDevice();

                if (_webCamera != null)
                {
                    _webCamera = new WebCamera();
                    _connector.Connect(_webCamera.VideoChannel, _drawingImageProvider);
                    videoViewer.SetImageProvider(_drawingImageProvider);
                    _webCamera.Start();
                    videoViewer.Start();

                    // save
                    //if (!System.IO.Directory.Exists(_VideoDirectory))
                    //{
                    //    System.IO.Directory.CreateDirectory(_VideoDirectory);
                    //}

                    //_webCamera.VideoChannel
                    _runningCamera = "USB";
                    //StartVideoCapture(_VideoDirectory);
                }
                else { MessageBox.Show("No USB Camera found."); }
            }
            catch (Exception ex)
            {
                //StreamUSBCamera.IsEnabled = false;
                //USBCam_error.Content = ex.ToString();
            }
        }

        private void DisconnectRunningCamera()
        {
            if (!string.IsNullOrEmpty(_runningCamera))
            {
                if (_runningCamera == "USB")
                {
                    _webCamera.Stop();
                    _webCamera.Dispose();
                    _drawingImageProvider.Dispose();
                    _connector.Disconnect(_webCamera.VideoChannel, _drawingImageProvider);
                    _connector.Dispose();
                    videoViewer.Stop();
                    videoViewer.Background = Brushes.Black;

                }

            }
        }
        private void _mpeg4Recorder_MultiplexFinished(object sender, VoIPEventArgs<bool> e)
        {
            _connector.Disconnect(_webCamera.AudioChannel, _mpeg4Recorder.AudioRecorder);
            _connector.Disconnect(_webCamera.VideoChannel, _mpeg4Recorder.VideoRecorder);
            _mpeg4Recorder.Dispose();
        }

        private void StartVideoCapture(string path)
        {
            var date = DateTime.Now.Year + "y-" + DateTime.Now.Month + "m-" + DateTime.Now.Day + "d-" +
                       DateTime.Now.Hour + "h-" + DateTime.Now.Minute + "m-" + DateTime.Now.Second + "s";
            string currentpath;
            if (String.IsNullOrEmpty(path))
                currentpath = date + ".mp4";
            else
                currentpath = path + "\\" + date + ".mp4";

            _mpeg4Recorder = new MPEG4Recorder(currentpath);
            _mpeg4Recorder.MultiplexFinished += _mpeg4Recorder_MultiplexFinished;
            _connector.Connect(_webCamera.AudioChannel, _mpeg4Recorder.AudioRecorder);
            _connector.Connect(_webCamera.VideoChannel, _mpeg4Recorder.VideoRecorder);

            //await Task.Delay(100000);
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
        #region network camera
        private void ConnectIPCamera(string URL, string username, string password)
        {
            _drawingImageProvider = new DrawingImageProvider();
            _connector = new MediaConnector();
            try
            {
                _camera = IPCameraFactory.GetCamera(URL, username, password);
                _connector.Connect(_camera.VideoChannel, _drawingImageProvider);
                _camera.Start();
                videoViewer.SetImageProvider(_drawingImageProvider);
                videoViewer.Start();
            }
            catch
            {
                // MessageBox.Show("Failed to connect IP Camera..");
            }
        }

        private void DisconnectIPCamera()
        {
            _camera.Stop();
            _camera.Dispose();
            _drawingImageProvider.Dispose();
            _connector.Disconnect(_camera.VideoChannel, _drawingImageProvider);
            _connector.Dispose();
            videoViewer.Stop();
            videoViewer.Background = Brushes.Black;
        }
        #endregion
        #region Camera Streaming

        private void StreamUSBCamera_Click(string ipAddress, string Port)
        {
            try
            {
                var ip = ipAddress;
                var port = Port;

                OzConf_MJPEGStreamServer ozConf_ = new OzConf_MJPEGStreamServer(int.Parse(port), 25);
                ozConf_.Name = ip;
                _streamer = new MJPEGStreamer(ozConf_);

                _connector.Connect(_videoSender, _streamer.VideoChannel);

                _streamer.ClientConnected += _streamer_ClientConnected;
                _streamer.ClientDisconnected += _streamer_ClientDisconnected;
                _streamer.Start();


            }
            catch
            {

            }
        }

        private void _streamer_ClientDisconnected(object sender, OzEventArgs<OzBaseMJPEGStreamConnection> e)
        {
            e.Item.StopStreaming();
        }

        private void _streamer_ClientConnected(object sender, OzEventArgs<OzBaseMJPEGStreamConnection> e)
        {
            e.Item.StartStreaming();
        }

        private void UnstreamUSBCam_Click()
        {
            _streamer.Stop();
            _connector.Disconnect(_videoSender, _streamer.VideoChannel);
        }

        private void OpenInBrowserButton_Click(string ipAddress, string Port)
        {
            var ip = ipAddress;
            var port = Port;
            CreateHTMLPage(ip, port);
            System.Diagnostics.Process.Start("test.html");
        }

        private void CreateHTMLPage(string ipaddress, string port)
        {
            using (var fs = new FileStream("test.html", FileMode.Create))
            {
                using (var w = new StreamWriter(fs, Encoding.UTF8))
                {
                    w.WriteLine("<img id='cameraImage' style='height: 100%;' src='http://" + ipaddress + ":" + port + "' alt='camera image' />");
                }
            }
        }

        #endregion
        #region UI Interactive Function Weight Module for Test environment
        void showWeightModuleResponse(RecData _recData)
        {

            if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
            {
                byte[] bytestToRead = _recData.MbTgm.Skip(readIndex).ToArray();
                string str = Encoding.Default.GetString(bytestToRead).Replace(System.Environment.NewLine, string.Empty);
                //string actualdata = Regex.Replace(str, @"[^a-zA-Z0-9\\:_\- ]", string.Empty);
                string actualdata = Regex.Replace(str, @"[^\t\r\n -~]", "_").RemoveWhitespace().Trim();
                string[] data = actualdata.Split('_');

                for (int i = 0; i < data.Length; i++)
                {
                    if (!string.IsNullOrEmpty(data[i]) || !string.IsNullOrWhiteSpace(data[i]))
                    {
                        if (data[i].All(char.IsDigit))
                        {
                            _dispathcer.Invoke(new Action(() => { WeightContent.Content = data[i].ToString().Trim(); }));
                            //WeightContent.Content = data[i].ToString().Trim();

                            continue;
                        }

                        _dispathcer.Invoke(new Action(() => { WeightContent.Content = new String(data[i].Where(Char.IsDigit).ToArray()); }));
                        //WeightContent.Content = new String(data[i].Where(Char.IsDigit).ToArray());

                        string unit = new String(data[i].Where(Char.IsLetter).ToArray());
                        if (unit.ToLower().ToString().Contains(weightUnit.KG.ToString().ToLower()) || unit.ToLower().ToString().Contains(weightUnit.KGG.ToString().ToLower()) || unit.ToLower().ToString().Contains(weightUnit.KGGM.ToString().ToLower()))
                        {
                            _dispathcer.Invoke(new Action(() =>
                            {
                                if (unit.ToLower().ToString().Contains(weightUnit.KGGM.ToString().ToLower()))
                                {
                                    WeightUnitKG.Content = weightUnit.KGGM.ToString().ToLower();
                                }
                                else if (unit.ToLower().ToString().Contains(weightUnit.KGG.ToString().ToLower()))
                                {
                                    WeightUnitKG.Content = weightUnit.KGG.ToString().ToLower();
                                }
                                else
                                {
                                    WeightUnitKG.Content = weightUnit.KG.ToString().ToLower();
                                }
                                WeightUnitKG.Foreground = Brushes.Red;
                                WeightUnitLB.Foreground = Brushes.White;
                                WeightUnitOZ.Foreground = Brushes.White;
                                WeightUnitPCS.Foreground = Brushes.White;
                            }));


                        }
                        else if (unit.ToLower().ToString().Contains(weightUnit.LB.ToString().ToLower()))
                        {
                            _dispathcer.Invoke(new Action(() =>
                            {
                                WeightUnitKG.Foreground = Brushes.White;
                                WeightUnitLB.Foreground = Brushes.Red;
                                WeightUnitOZ.Foreground = Brushes.White;
                                WeightUnitPCS.Foreground = Brushes.White;
                            }));

                        }
                        else if (unit.ToLower().ToString().Contains(weightUnit.PCS.ToString().ToLower()))
                        {
                            _dispathcer.Invoke(new Action(() =>
                            {
                                WeightUnitKG.Foreground = Brushes.White;
                                WeightUnitLB.Foreground = Brushes.White;
                                WeightUnitOZ.Foreground = Brushes.White;
                                WeightUnitPCS.Foreground = Brushes.Red;
                            }));

                        }
                        else if (unit.ToLower().ToString().Contains(weightUnit.OZ.ToString().ToLower()))
                        {
                            _dispathcer.Invoke(new Action(() =>
                            {
                                WeightUnitKG.Foreground = Brushes.White;
                                WeightUnitLB.Foreground = Brushes.White;
                                WeightUnitOZ.Foreground = Brushes.Red;
                                WeightUnitPCS.Foreground = Brushes.White;
                            }));


                        }
                    }

                }

            }
            //TimerCheckReceiveData.Enabled = true;
        }

        bool validateResponse(RecData _recData)
        {
            bool isvalide = false;
            if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
            {
                byte[] bytestToRead = _recData.MbTgm.Skip(readIndex).ToArray();
                string str = Encoding.Default.GetString(bytestToRead).Replace(System.Environment.NewLine, string.Empty);
                //string actualdata = Regex.Replace(str, @"[^a-zA-Z0-9\\:_\- ]", string.Empty);
                string actualdata = Regex.Replace(str, @"[^\t\r\n -~]", "_").RemoveWhitespace().Trim();
                string[] data = actualdata.Split('_');

                for (int i = 0; i < data.Length; i++)
                {
                    if (!string.IsNullOrEmpty(data[i]) || !string.IsNullOrWhiteSpace(data[i]))
                    {
                        isvalide = true;
                    }

                }

            }
            return isvalide;
        }

        void ReadControlCardResponse(RecData _recData)
        {

            if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
            {
                //To Read Function Code response.
                if (_recData.MbTgm[1] == (int)COM_Code.three)
                {
                    // int read = _recData.MbTgm[2];
                    // byte[] arr = _recData.MbTgm.Where((item, index) => index > 2 && index < 63).ToArray();
                    //for (int i = 0; i < arr.Length; i+=2)
                    //{
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
                    //int _i30 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 63);
                    //int _i31 = ByteArrayConvert.ToUInt16(Common.MbTgmBytes, 65);



                    if (_i0 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput0.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput0.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i1 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput1.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput1.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }


                    if (_i2 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput2.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput2.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i3 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput3.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput3.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i4 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput4.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput4.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i5 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput5.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput5.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i6 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput6.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput6.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i7 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput7.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput7.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i8 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput8.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput8.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i9 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput9.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput9.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i10 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput10.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput10.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i11 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput11.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput0.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i12 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput12.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput12.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i13 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput13.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput13.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i14 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput14.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput14.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }

                    if (_i15 == 0)
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput15.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));

                    }
                    else
                    {
                        _dispathcer.Invoke(new Action(() => { ReadInput15.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));

                    }


                    //Read input register state.

                    _dispathcer.Invoke(new Action(() =>
                    {
                        Toggle16.IsChecked = _i16 == 0 ? true : false;
                        Toggle17.IsChecked = _i17 == 0 ? true : false;
                        Toggle18.IsChecked = _i18 == 0 ? true : false;
                        Toggle19.IsChecked = _i19 == 0 ? true : false;
                        Toggle20.IsChecked = _i20 == 0 ? true : false;
                        Toggle21.IsChecked = _i21 == 0 ? true : false;
                        Toggle22.IsChecked = _i22 == 0 ? true : false;
                        Toggle23.IsChecked = _i23 == 0 ? true : false;
                        Toggle24.IsChecked = _i24 == 0 ? true : false;
                        Toggle25.IsChecked = _i25 == 0 ? true : false;
                        Toggle26.IsChecked = _i26 == 0 ? true : false;
                        Toggle27.IsChecked = _i27 == 0 ? true : false;
                        Toggle28.IsChecked = _i28 == 0 ? true : false;
                        Toggle29.IsChecked = _i29 == 0 ? true : false;
                        //Toggle30.IsChecked = _i30 == 0 ? true : false;
                    }));

                }
                //To Write Function Code response.
                else if (_recData.MbTgm[1] == (int)COM_Code.sixteen)
                {
                    //switch(toggled)
                    //{
                    //    case 16:
                    //        Toggle16.IsChecked=
                    //}
                }
            }
        }
        #endregion
        #region Custom Operation
        private void StopPortCommunication()
        {
            if (this.SerialDevice.IsOpen)
            {
                Common.RecState = 0;
                Common.CurrentDevice = Models.DeviceType.None;
                Common.COMSelected = COMType.None;
                Common.RequestDataList.Clear();
                this.SerialDevice.DtrEnable = false;
                this.SerialDevice.RtsEnable = false;
                this.SerialDevice.DataReceived -= SerialDevice_DataReceived;
                this.SerialDevice.DiscardInBuffer();
                this.SerialDevice.DiscardOutBuffer();
                this.SerialDevice.Close();
                return;
            }

            AddOutPut("No Serial Port is open..", (int)OutPutType.WARNING);
        }

        private void SerialPortCommunications(string port = "", int baudRate = 0, int databit = 0, int stopBit = 0, int parity = 0)
        {
            if (!this.SerialDevice.IsOpen)
            {
                try
                {
                    this.SerialDevice = new SerialPort(port);
                    this.SerialDevice.BaudRate = baudRate;
                    this.SerialDevice.DataBits = databit;
                    this.SerialDevice.StopBits = stopBit == 0 ? StopBits.None : (stopBit == 1 ? StopBits.One : (stopBit == 2 ? StopBits.Two : StopBits.OnePointFive));
                    this.SerialDevice.Parity = Parity.None;
                    this.SerialDevice.Handshake = Handshake.None;
                    this.SerialDevice.Encoding = ASCIIEncoding.ASCII;
                    this.SerialDevice.DataReceived += SerialDevice_DataReceived;
                    this.SerialDevice.Open();
                }
                catch (Exception ex)
                {
                    AddOutPut("An error has occured : " + ex.Message.ToString(), (int)OutPutType.ERROR, true);
                }
                return;
            }
            AddOutPut("Serial port is busy..", (int)OutPutType.WARNING, true);
        }

        private void DataReader()
        {
            TimerCheckReceiveData.Enabled = false;

            SB1Request _SB1Request = new SB1Request();

            if (Common.RecState > 0)
            {
                if (!SerialDevice.IsOpen)
                {
                    //To show the status of Connects Ports
                }
                else
                {
                    //ToolTipStatus = ComStatus.OK;
                }

                TimeSpan _RqRsDiff = DateTime.Now - Common.LastRequestSent;
                //Common.WriteLog(Common.LastRequestSent.ToString("dd-MM-yyyy mm:ss:ffff"));
                if (_RqRsDiff.TotalMilliseconds > Common.SessionTimeOut)  // Timeout
                {
                    RecData _recData = Common.RequestDataList.Where(a => a.SessionId == Common.GetSessionId).FirstOrDefault();
                    if (_recData != null)
                    {

                        if (_recData.Status == PortDataStatus.AckOkWait)
                        {
                            Common.TimeOuts++;  // SB1 Timeout & Tgm Timeout      
                            //ToolTipStatus = ComStatus.OK;
                            _recData.Status = PortDataStatus.SessionEnd;
                            //Common.ReceiveDataQueue.Enqueue(_recData);
                        }
                        else
                        {
                            //ToolTipStatus = ComStatus.TimeOut;
                            _recData.Status = PortDataStatus.Normal;
                        }


                    }
                    else
                    {
                        //_SB1Request.EndSession(serialPort1); // End Session  
                    }
                }
            }

            if (Common.RecState > 0 && IsComplete)
            {
                IsComplete = false;


                //recState = 1;
                while (Common.ReceiveBufferQueue.Count > 0)
                {
                    recBufParse = Common.ReceiveBufferQueue.Dequeue();

                    Common.RecState = 1;
                    SB1Reply _reply = new SB1Reply(Common.GetSessionId);
                    SB1Handler _hndl = new SB1Handler(SerialDevice);
                    _reply.SetTgm(recBufParse, _CurrentActiveMenu); //recBuf old implementation
                    RecData _recData = Common.RequestDataList.Where(a => a.SessionId == _reply.sesId).FirstOrDefault();
                    if (_recData != null && _reply != null)
                    {
                        //Common.WriteLog("Response :- " + _recData.PropertyName + "-" + _reply.sesId.ToString());
                    }
                    if (_reply.CheckCrc(recBufParse, Convert.ToInt32(_reply.length)) || _CurrentActiveMenu == AppTools.UART || write == true || write == false)  // SB1 Check CRC
                    {

                        _UpdatePB = true;
                        //TimerCheckReceiveData.Enabled = true;
                        if (_reply.IsAckOk()) // Ok
                        {
                            if (_recData != null && _reply.sesId == _recData.SessionId)
                            {
                                if (Common.COMSelected == COMType.UART)
                                {
                                    _recData.MbTgm = recBufParse;
                                    _recData.Status = PortDataStatus.Received;
                                    Common.GoodTmgm++;
                                    Common.ReceiveDataQueue.Enqueue(_recData);

                                    //showWeightModuleResponse();   // To show on device UI

                                    //TimerCheckReceiveData.Enabled = true;
                                    return;
                                }

                                Byte[] _payLoad = _reply.ExtractPayload();
                                PayloadRS _PayloadRS = new PayloadRS();
                                _PayloadRS.SetPayLoadRS(_payLoad);
                                _MbTgmBytes = _PayloadRS.ExtractModBusTgm(_payLoad);
                                if (Common.COMSelected == COMType.MODBUS)
                                {
                                    _MbTgmBytes = _reply.RxSB1;
                                    _PayloadRS.MbLength = (ushort)_reply.length;
                                }
                                if (_MbTgmBytes != null && _PayloadRS.MbLength > 0)
                                {
                                    bool _IsTgmErr = false;
                                    _IsTgmErr = CheckTgmError(_recData, _payLoad, _MbTgmBytes, _PayloadRS.MbLength);
                                    if (_IsTgmErr || write == true)
                                    {
                                        if (_recData.RqType == RQType.WireLess)
                                        {
                                            Common.MbTgmBytes = _reply.payload;
                                        }
                                        else
                                        {
                                            Common.MbTgmBytes = _MbTgmBytes;
                                        }
                                        _recData.Status = PortDataStatus.Received;
                                        _recData.MbTgm = Common.MbTgmBytes;
                                        Common.ReceiveDataQueue.Enqueue(_recData);
                                        //ReadControlCardResponse();
                                        return;

                                    }
                                }
                                else if (_reply.ack == 5)
                                {

                                    if (_recData != null)
                                        _recData.Status = PortDataStatus.AckOkWait;
                                }
                                else
                                {
                                    if (_recData != null)
                                    {
                                        if (_recData.Status == PortDataStatus.SessionEnd)
                                        {
                                            Common.RecState = 0;
                                            _recData.Status = PortDataStatus.SessionEnd;
                                        }
                                        else
                                        {
                                            _recData.Status = PortDataStatus.AckOkWait;  // Change it to Ok
                                        }
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
                                        Common.ReceiveDataQueue.Enqueue(_recData);
                                    }

                                }
                            }
                        }
                        else if (_reply.ack == 1)  // Busy
                        {

                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Busy;

                                _SB1Request.EndSession(SerialDevice);

                            }
                        }
                        else if (_reply.ack == 2)  // IllegalCommand
                        {
                            _SB1Request.EndSession(SerialDevice);
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Normal;
                                if (_recData.SessionId > 0)
                                {
                                    Common.ReceiveDataQueue.Enqueue(_recData);
                                }
                            }
                        }
                        else if (_reply.ack == 3)  //CrcFault
                        {
                            _SB1Request.EndSession(SerialDevice);
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Normal;
                                if (_recData.SessionId > 0)
                                {
                                    Common.ReceiveDataQueue.Enqueue(_recData);
                                }
                            }
                        }
                        else if (_reply.ack == 4)  //Tgm Fault
                        {
                            _SB1Request.EndSession(SerialDevice);
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Normal;
                                if (_recData.SessionId > 0)
                                {
                                    Common.ReceiveDataQueue.Enqueue(_recData);
                                }
                            }
                        }
                    }
                    else
                    {
                        _UpdatePB = true;
                        _SB1Request.EndSession(SerialDevice);
                        if (_recData != null)
                        {
                            _recData.Status = PortDataStatus.Normal;
                            if (_recData.SessionId > 0)
                            {
                                Common.ReceiveDataQueue.Enqueue(_recData);
                            }
                        }


                    }
                }

            }

            TimerCheckReceiveData.Enabled = true;
        }

        private void SerialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            switch (Common.RecState)
            {
                case 0:
                    break;
                case 1:
                    int i = 0;
                    if (Common.COMSelected == COMType.UART)
                    {
                        Common.RecIdx = 0;
                        Common.RecState = 2;

                        recBuf = new byte[this.SerialDevice.BytesToRead];
                        this.SerialDevice.Read(recBuf, 0, recBuf.Length);
                        IsComplete = true;
                        Common.ReceiveBufferQueue.Enqueue(recBuf);


                    }
                    else if (Common.COMSelected == COMType.MODBUS)
                    {
                        Common.RecIdx = 0;
                        Common.RecState = 2;


                        while (SerialDevice.BytesToRead > 0)
                        {
                            byte[] rec = new byte[1];
                            Common.RecIdx += SerialDevice.Read(rec, 0, 1);
                            recBuf[i] = rec[0];
                            i++;
                        }
                        if (Common.RecIdx > 3)
                        {
                            if (_CurrentActiveMenu == AppTools.Modbus)
                            {
                                TotalReceiveSize = (uint)recBuf[2] + 5;
                            }
                        }
                        if (TotalReceiveSize > Common.RecIdx)
                        {
                            IsComplete = false;
                        }
                        else
                        {
                            IsComplete = true;
                            Common.ReceiveBufferQueue.Enqueue(recBuf);
                        }
                    }

                    //TimerCheckReceiveData.Enabled = true;
                    //TimerCheckReceiveData_Elapsed();
                    Common.LastResponseReceived = DateTime.Now;
                    break;
                case 2:


                    if (Common.COMSelected == COMType.XYZ)
                    {
                        i = Common.RecIdx;
                        while (SerialDevice.BytesToRead > 0)
                        {
                            byte[] rec = new byte[1];
                            Common.RecIdx += SerialDevice.Read(rec, 0, 1);
                            recBuf[i] = rec[0];
                            i++;
                        }
                        if (Common.RecIdx > 3)
                        {
                            TotalReceiveSize = Util.ByteArrayConvert.ToUInt32(recBuf, 0);
                        }

                        if (TotalReceiveSize > Common.RecIdx)
                        {
                            IsComplete = false;
                        }
                        else
                        {
                            IsComplete = true;
                            Common.ReceiveBufferQueue.Enqueue(recBuf);
                        }
                    }
                    else if (Common.COMSelected == COMType.MODBUS)
                    {
                        i = Common.RecIdx;
                        while (SerialDevice.BytesToRead > 0)
                        {
                            byte[] rec = new byte[1];
                            Common.RecIdx += SerialDevice.Read(rec, 0, 1);
                            recBuf[i] = rec[0];
                            i++;
                        }
                        if (Common.RecIdx > 3)
                        {
                            if (_CurrentActiveMenu == AppTools.Modbus)
                            {
                                TotalReceiveSize = (uint)recBuf[2] + 5;
                            }
                            else if (_CurrentActiveMenu == AppTools.EnOcean)
                            {
                                byte[] Temp = new byte[4];
                                Temp[2] = recBuf[1];
                                Temp[3] = recBuf[2];
                                UInt32 LenghtData = Util.ByteArrayConvert.ToUInt32(Temp, 0);
                                Temp[3] = recBuf[3];
                                UInt32 LengthOptional = Util.ByteArrayConvert.ToUInt32(Temp, 0);
                                TotalReceiveSize = LenghtData + LengthOptional + 7;
                            }
                        }

                        if (TotalReceiveSize > Common.RecIdx)
                        {
                            IsComplete = false;
                        }
                        else
                        {
                            IsComplete = true;
                            Common.ReceiveBufferQueue.Enqueue(recBuf);
                        }
                    }
                    Common.LastResponseReceived = DateTime.Now;
                    //TimerCheckReceiveData_Elapsed();
                    //TimerCheckReceiveData.Enabled = true;
                    break;
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
                        {
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
            }
            return _IsTgmErr;
        }

        private void ReadWeight(string Port, int Baudrate, int databit, int stopbit, int parity)
        {
            try
            {
                Common.RecState = 1;
                Common.CurrentDevice = Models.DeviceType.WeightModule;
                RecData _recData = new RecData();
                _recData.deviceType = Models.DeviceType.WeightModule;
                _recData.PropertyName = "WeightModule";
                _recData.SessionId = Common.GetSessionNewId;
                _recData.Ch = 0;
                _recData.Indx = 0;
                _recData.Reg = 0;
                _recData.NoOfVal = 0;
                Common.GetSessionId = _recData.SessionId;
                _recData.Status = PortDataStatus.Requested;
                _recData.RqType = RQType.UART;
                Common.COMSelected = COMType.UART;
                _CurrentActiveMenu = AppTools.UART;
                Common.currentReequest = _recData;
                Common.RequestDataList.Add(_recData);
                SerialPortCommunications(Port, Baudrate, databit, stopbit, parity);
                Thread.Sleep(30);
            }
            catch
            {

            }
        }

        private void ReadAllControCardInputOutput()
        {

            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            _CurrentActiveMenu = AppTools.Modbus;
            obj.GetMultiSendorValueFM3(1, 0, SerialDevice, 0, 30, "ControlCard", 1, 0, Models.DeviceType.ControlCard);
            // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        private void ReadControCardState(int reg, string Comport)
        {

            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            _CurrentActiveMenu = AppTools.Modbus;
            SerialPortCommunications(Comport, 38400, 8, 1, 0);
            obj.GetMultiSendorValueFM3(1, 0, SerialDevice, reg, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        private void WriteControCardState(int reg, int val)
        {

            write = true;
            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            int[] _val = new int[2] { 0, val };
            obj.SetMultiSendorValueFM16(1, 0, SerialDevice, reg + 1, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard, _val, false);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        public void ExecuteProcesses()
        {
            if (Commands != null && Commands.Count() > 0)
            {
                if (Commands.Where(x => x.ExecutionStatus != (int)ExecutionStage.Executed).ToList().Count() > 0)
                {

                    foreach (var command in Commands.Where(x => x.ExecutionStatus != (int)ExecutionStage.Executed).OrderBy(x => x.Order).ToList())
                    {
                        if (command.CommandType == (int)ElementConstant.Connect_ControlCard_Event)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                AddOutPut("Initializing control card port..", (int)OutPutType.INFORMATION);
                                AddOutPut("Connecting control card..", (int)OutPutType.INFORMATION);
                                Connect_control_card(command.Configuration.deviceDetail.PortName, command.Configuration.deviceDetail.BaudRate, command.Configuration.deviceDetail.DataBit, command.Configuration.deviceDetail.StopBit, command.Configuration.deviceDetail.Parity);
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                AddOutPut("Control card is connected..", (int)OutPutType.SUCCESS, true);

                            }
                            break;
                        }

                        if (command.CommandType == (int)ElementConstant.Disconnect_ControlCard_Event)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                AddOutPut("Disconnecting Control Card..", (int)OutPutType.INFORMATION);
                                StopPortCommunication();
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                AddOutPut("Control card is disconnected..", (int)OutPutType.SUCCESS, true);

                            }

                            break;
                        }

                        if (command.CommandType == (int)ElementConstant.Connect_Weight_Event)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                AddOutPut("Connecting weight module..", (int)OutPutType.INFORMATION);
                                ReadWeight(command.Configuration.deviceDetail.PortName, command.Configuration.deviceDetail.BaudRate, command.Configuration.deviceDetail.DataBit, command.Configuration.deviceDetail.StopBit, command.Configuration.deviceDetail.Parity);
                                command.ExecutionStatus = (int)ExecutionStage.Executing;

                                AddOutPut("weight module is connected..", (int)OutPutType.SUCCESS, true);
                                AddOutPut("Wait for weight module response..", (int)OutPutType.INFORMATION);
                                //Thread.Sleep(500);
                            }
                            else
                            {
                                AddOutPut("Started Fetching weight module input..", (int)OutPutType.SUCCESS, true);
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                AddOutPut("Fetching Stopped..", (int)OutPutType.SUCCESS, true);
                            }
                            break;
                        }

                        if (command.CommandType == (int)ElementConstant.Receive_Message_Event)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                if (Common.CurrentDevice == Models.DeviceType.WeightModule)
                                {
                                    AddOutPut("Receiving data..", (int)OutPutType.INFORMATION);
                                    bool received = false;
                                    RecData _rec = new RecData();
                                    while (received == false)
                                    {
                                        DataReader();
                                        if (Common.ReceiveDataQueue.Count > 0)
                                        {
                                            _rec = new RecData();
                                            _rec = Common.ReceiveDataQueue.Dequeue();
                                            if (validateResponse(_rec))
                                            {
                                                received = true;
                                            }
                                        }
                                    }


                                    command.OutPutData = _rec;
                                    command.ExecutionStatus = (int)ExecutionStage.Executed;
                                    AddOutPut("Storing data into " + command.CommandText + "..", (int)OutPutType.INFORMATION);
                                    AddOutPut("Showing weight module response..", (int)OutPutType.INFORMATION);
                                    showWeightModuleResponse(_rec);
                                }
                                else
                                {
                                    AddOutPut("Reading control card initial state..", (int)OutPutType.INFORMATION);
                                    ReadAllControCardInputOutput();
                                    command.ExecutionStatus = (int)ExecutionStage.Executing;
                                }
                            }
                            else
                            {
                                if (Common.CurrentDevice == Models.DeviceType.ControlCard || Common.CurrentDevice == Models.DeviceType.MotorDerive)
                                {
                                    AddOutPut("Storing data into " + command.CommandText + "..", (int)OutPutType.INFORMATION);
                                    DataReader();
                                    RecData _rec = new RecData();
                                    _rec = Common.ReceiveDataQueue.Dequeue();
                                    command.OutPutData = _rec;
                                    command.ExecutionStatus = (int)ExecutionStage.Executed;
                                    AddOutPut("Data stored into " + command.CommandText + "..", (int)OutPutType.INFORMATION);
                                    ReadControlCardResponse(_rec);
                                    AddOutPut("Showing control card state..", (int)OutPutType.INFORMATION);
                                }
                                else
                                {
                                    command.ExecutionStatus = (int)ExecutionStage.Executed;
                                }
                            }
                            break;
                        }
                        if (command.CommandType == (int)ElementConstant.Disconnect_Weight_Event)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                AddOutPut("Disconnecting weight module..", (int)OutPutType.WARNING, true);
                                StopPortCommunication();
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                AddOutPut("Weight module disconnected..", (int)OutPutType.SUCCESS, true);

                            }
                            else
                            {
                                //OutPutArea.AppendText("\n Executing " + command.CommandText + "..");
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                            }

                            break;
                        }
                    }

                    ExecuteProcesses();
                }
                else
                {
                    //OutPutArea.AppendText("\n Exection end.");
                }
            }

        }

        private void Connect_control_card(string Port, int Baudrate, int databit, int stopbit, int parity)
        {
            try
            {
                Common.RecState = 1;
                Common.CurrentDevice = Models.DeviceType.ControlCard;
                RecData _recData = new RecData();
                _recData.deviceType = Models.DeviceType.ControlCard;
                _recData.PropertyName = "ControlCard";
                _recData.SessionId = Common.GetSessionNewId;
                _recData.Ch = 0;
                _recData.Indx = 0;
                _recData.Reg = 0;
                _recData.NoOfVal = 0;
                Common.GetSessionId = _recData.SessionId;
                _recData.Status = PortDataStatus.Requested;
                _recData.RqType = RQType.ModBus;
                Common.COMSelected = COMType.MODBUS;
                _CurrentActiveMenu = AppTools.Modbus;
                Common.currentReequest = _recData;
                Common.RequestDataList.Add(_recData);
                SerialPortCommunications(Port, Baudrate, databit, stopbit, parity);
            }
            catch (Exception ex)
            {
                AddOutPut("An error has occured : " + ex.Message.ToString(), (int)OutPutType.ERROR, true);
            }

        }

        #endregion

        #region Output Function
        private void AddOutPut(string Output, int MessageType, bool isBold = false)
        {
            Paragraph myParagraph = new Paragraph();
            switch (MessageType)
            {
                case (int)OutPutType.SUCCESS:
                    myParagraph.Foreground = Brushes.ForestGreen;
                    break;
                case (int)OutPutType.ERROR:
                    myParagraph.Foreground = Brushes.Red;
                    break;
                case (int)OutPutType.INFORMATION:
                    myParagraph.Foreground = Brushes.Black;
                    break;
                case (int)OutPutType.WARNING:
                    myParagraph.Foreground = Brushes.OrangeRed;
                    break;
            }
            if (isBold)
            {
                Bold myBold = new Bold(new Run(Output));
                myParagraph.Inlines.Add(myBold);
            }
            else
            {
                Run myRun = new Run(Output);
                myParagraph.Inlines.Add(myRun);
            }
            OutPutControl.Blocks.Add(myParagraph);
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddOutPut("Commands compilation started..", (int)OutPutType.INFORMATION);
            Task.Delay(100);
            //Thread.Sleep(100);
            ExecuteProcesses();
        }

        //private void Window_Activated(object sender, EventArgs e)
        //{

        //}
    }
}
