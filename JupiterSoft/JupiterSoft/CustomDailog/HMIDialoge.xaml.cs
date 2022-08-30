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
using Microsoft.Expression.Encoder.Devices;
using System.Collections.ObjectModel;

namespace JupiterSoft.CustomDailog
{
    /// <summary>
    /// Interaction logic for HMIDialoge.xaml
    /// </summary>
    public partial class HMIDialoge : Window
    {
        #region Function Variablle
        const int REC_BUF_SIZE = 10000;
        byte[] recBuf = new byte[REC_BUF_SIZE];
        byte[] recBufParse = new byte[REC_BUF_SIZE];

        public Byte[] _MbTgmBytes;
        internal UInt32 TotalReceiveSize = 0;
        bool IsComplete = false;


        private List<RegisterOutputStatus> registerOutputStatuses;
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
        private DeviceInfo deviceInfo;
        private SerialPort SerialDevice;
        private Dispatcher _dispathcer;
        private int readIndex = 0;
        private MPEG4Recorder _mpeg4Recorder;

        private List<LogicalCommand> Commands;
        private List<ConnectedDevices> connectedDevices;
        #endregion

        public bool IsClosed { get; set; }
        public Collection<EncoderDevice> VideoDevices { get; set; }
        public Collection<EncoderDevice> AudioDevices { get; set; }

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

        public string SnapshotDirectory { get; set; }
        bool isCameraRunning = false;

        public HMIDialoge(List<LogicalCommand> _Commands, DeviceInfo _deviceInfo)
        {
            InitializeComponent();

            _dispathcer = Dispatcher.CurrentDispatcher;
            this.SerialDevice = new SerialPort();
            this.Commands = _Commands;
            this.Commands.ForEach(x => x.ExecutionStatus = (int)ExecutionStage.Not_Executed);
            this.deviceInfo = _deviceInfo;
            registerOutputStatuses = new List<RegisterOutputStatus>();
            connectedDevices = new List<ConnectedDevices>();



            VideoDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video);
            AudioDevices = EncoderDevices.FindDevices(EncoderDeviceType.Audio);
            SnapshotDirectory = ApplicationConstant._SnapShotDirectory;
            VideoFileDirectory = _VideoDirectory;
            if (!Directory.Exists(VideoFileDirectory))
            {
                Directory.CreateDirectory(VideoFileDirectory);
            }
            this.DataContext = this;
        }

        private void StopProcess_Click(object sender, RoutedEventArgs e)
        {
            if (WebcamViewer.IsRecording)
            {
                StopUSBCamRecording();
            }

            if (isCameraRunning)
            {
                StopUSBCamera();
            }
            //DisconnectCamera();
            if (connectedDevices != null)
            {
                if(connectedDevices.Count()>0)
                {
                    foreach (var item in connectedDevices)
                    {
                        StopPortCommunication(item.DeviceType);
                    }
                }
                
            }


            IsClosed = true;
            Close();
        }

        #region Camera Function
        private void StartCameraRecording()
        {

            try
            {
                StartVideoCapture(_VideoDirectory);
            }
            catch
            {
                StartVideoCapture(_VideoDirectory);
            }
        }

        private void StopCameraRecording()
        {
            StopVideoCapture();
        }

        private bool ConnectionUSB()
        {
            bool isStarted = false;
            try
            {

                _webCamera = new WebCamera();
                _drawingImageProvider = new DrawingImageProvider();
                _connector = new MediaConnector();

                _webCamera = WebCameraFactory.GetDefaultDevice();

                if (_webCamera != null)
                {

                    _webCamera = new WebCamera();
                    _connector.Connect(_webCamera.VideoChannel, _drawingImageProvider);
                    //videoViewer.SetImageProvider(_drawingImageProvider);
                    _webCamera.Start();
                    //videoViewer.Start();
                    // _videoSender = _webCamera.VideoChannel;

                    isStarted = true;


                    _runningCamera = "USB";
                    AddOutPut("Camera has started..", (int)OutPutType.INFORMATION, true);
                }
                else
                {
                    AddOutPut("No USB Camera found.", (int)OutPutType.INFORMATION);
                }
            }
            catch (Exception ex)
            {
                AddOutPut("An error has occurred while stating camera..", (int)OutPutType.ERROR);
            }
            return isStarted;
        }

        private void DisconnectCamera()
        {
            if (!string.IsNullOrEmpty(_runningCamera))
            {
                // CameraArea.Visibility = Visibility.Hidden;
                if (_runningCamera == "USB")
                {
                    DisconnectUSBCamera();

                }
                else
                {
                    DisconnectIPCamera();
                }

            }
        }

        private void DisconnectUSBCamera()
        {
            if (_webCamera.Capturing)
            {
                StopVideoCapture();
                //videoViewer.Stop();
                //videoViewer.Background = Brushes.Black;
                _webCamera.Stop();
                _webCamera.Dispose();
                _drawingImageProvider.Dispose();
                _connector.Disconnect(_webCamera.VideoChannel, _drawingImageProvider);
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

        private void StartUSBCamera()
        {
            try
            {
                // Display webcam video
                WebcamViewer.StartPreview();



                isCameraRunning = true;
            }
            catch (Microsoft.Expression.Encoder.SystemErrorException ex)
            {
                AddOutPut("Camera is in use by another application..", (int)OutPutType.INFORMATION, true);
                //MessageBox.Show("Device is in use by another application");
            }
        }

        private void StopUSBCamera()
        {

            // Stop the display of webcam video.
            _dispathcer.Invoke(new Action(() => { WebcamViewer.StopPreview(); }));

            isCameraRunning = false;
        }

        private void StartUSBCamRecording()
        {
            //Start Capturing Video.
            if (!WebcamViewer.IsRecording)
            {
                _dispathcer.Invoke(new Action(() => { WebcamViewer.StartRecording(); }));
            }

        }

        private void StopUSBCamRecording()
        {
            // Stop recording of webcam video to harddisk.
            if (WebcamViewer.IsRecording)
            {
                _dispathcer.Invoke(new Action(() => { WebcamViewer.StopRecording(); }));
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

            }
            catch
            {
            }
        }

        private void DisconnectIPCamera()
        {
            _camera.Stop();
            _camera.Dispose();
            _drawingImageProvider.Dispose();
            _connector.Disconnect(_camera.VideoChannel, _drawingImageProvider);
            _connector.Dispose();
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

                string actualdata = Regex.Replace(str, @"[^\t\r\n -~]", "_").RemoveWhitespace().Trim();
                string[] data = actualdata.Split('_');
                var lastitem = data[data.Length - 1];
                var outP = lastitem.ToLower().ToString();
                double weight = 0;
                if (outP.Contains("kg"))
                {
                    if (outP.Contains("kgg"))
                    {
                        string builder = outP.Replace("kgg", "");
                        weight = Convert.ToInt32(builder) - 3067;
                        weight = Convert.ToDouble(weight / 260.4);
                        weight = weight * 0.453592;
                        weight = Math.Round(weight, 2);
                        _dispathcer.Invoke(new Action(() => { WeightContent.Content = weight.ToString(); }));
                        WeightUnitKG.Content = weightUnit.KGG.ToString().ToLower();
                    }
                    else if (outP.Contains("kgn"))
                    {
                        string builder = outP.Replace("kgn", "");
                        weight = Convert.ToInt32(builder) - 3067;
                        weight = Convert.ToDouble(weight / 260.4);
                        weight = weight * 0.453592;
                        weight = Math.Round(weight, 2);
                        _dispathcer.Invoke(new Action(() =>
                        {
                            WeightContent.Content = weight.ToString();
                            WeightUnitKG.Content = weightUnit.KGN.ToString().ToLower();
                        }));

                    }
                    else
                    {
                        string builder = outP.Replace("kg", "");
                        weight = Convert.ToInt32(builder) - 3067;
                        weight = Convert.ToDouble(weight / 260.4);
                        weight = weight * 0.453592;
                        weight = Math.Round(weight, 2);
                        _dispathcer.Invoke(new Action(() =>
                        {
                            WeightContent.Content = weight.ToString();
                            WeightUnitKG.Content = weightUnit.KG.ToString().ToLower();
                        }));

                    }

                }
                else if (outP.Contains("lb"))
                {

                    Regex re = new Regex(@"([a-zA-Z]+)(\d+)");
                    Match result = re.Match(outP);

                    string alphaPart = result.Groups[1].Value;
                    string numberPart = result.Groups[2].Value;
                    weight = Convert.ToInt32(numberPart) - 3067;
                    weight = Convert.ToDouble(weight / 260.4);
                    weight = Math.Round(weight, 2);
                    _dispathcer.Invoke(new Action(() =>
                    {
                        WeightContent.Content = weight.ToString();
                        WeightUnitKG.Content = weightUnit.LBS.ToString().ToLower();
                    }));

                    //weight = weight * 0.453592;
                }
                else
                {
                    weight = Convert.ToInt32(outP) - 3067;
                    weight = Convert.ToDouble(weight / 260.4);
                    weight = weight * 0.453592;
                    weight = Math.Round(weight, 2);
                    _dispathcer.Invoke(new Action(() =>
                    {
                        WeightContent.Content = weight.ToString();
                        WeightUnitKG.Content = weightUnit.KG.ToString().ToLower();
                    }));

                    //weight = 0;
                }



            }
            //TimerCheckReceiveData.Enabled = true;
        }

        bool validateResponse(RecData _recData)
        {
            bool isvalide = false;
            try
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
                            isvalide = true;
                        }

                    }

                }

            }
            catch { }
            return isvalide;
        }

        void ReadControlCardResponse(RecData _recData)
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
            }
        }

        void WriteControlCardResponse(int register, int val)
        {
            if (register == 1)
            {
                if (val == 1)
                {
                    _dispathcer.Invoke(new Action(() => { ReadInput0.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));
                }
                else
                {
                    _dispathcer.Invoke(new Action(() => { ReadInput0.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));
                }

            }
            else if (register == 2)
            {
                if (val == 1)
                {
                    _dispathcer.Invoke(new Action(() => { ReadInput1.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));
                }
                else
                {
                    _dispathcer.Invoke(new Action(() => { ReadInput1.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));
                }

            }
            else if (register == 3)
            {
                if (val == 1)
                {
                    _dispathcer.Invoke(new Action(() => { ReadInput2.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));
                }
                else
                {
                    _dispathcer.Invoke(new Action(() => { ReadInput2.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));
                }

            }
            else if (register == 4)
            {
                if (val == 1)
                {
                    _dispathcer.Invoke(new Action(() => { ReadInput3.Source = new BitmapImage(new Uri(@"/assets/CtrlOn.png", UriKind.Relative)); }));
                }
                else
                {
                    _dispathcer.Invoke(new Action(() => { ReadInput3.Source = new BitmapImage(new Uri(@"/assets/CtrlOff.png", UriKind.Relative)); }));
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




                    //set register state.
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 1, RegisterStatus = _i0 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 2, RegisterStatus = _i1 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 3, RegisterStatus = _i2 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 4, RegisterStatus = _i3 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 5, RegisterStatus = _i4 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 6, RegisterStatus = _i5 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 7, RegisterStatus = _i6 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 8, RegisterStatus = _i7 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 9, RegisterStatus = _i8 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 10, RegisterStatus = _i9 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 11, RegisterStatus = _i10 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 12, RegisterStatus = _i11 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 13, RegisterStatus = _i12 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 14, RegisterStatus = _i13 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 15, RegisterStatus = _i14 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 16, RegisterStatus = _i15 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 17, RegisterStatus = _i16 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 18, RegisterStatus = _i17 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 19, RegisterStatus = _i18 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 20, RegisterStatus = _i19 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 21, RegisterStatus = _i20 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 22, RegisterStatus = _i21 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 23, RegisterStatus = _i22 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 24, RegisterStatus = _i23 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 25, RegisterStatus = _i24 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 26, RegisterStatus = _i25 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 27, RegisterStatus = _i26 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 28, RegisterStatus = _i27 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 29, RegisterStatus = _i28 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 30, RegisterStatus = _i29 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 31, RegisterStatus = 0 });
                    registerOutputStatuses.Add(new RegisterOutputStatus { Register = 32, RegisterStatus = 0 });


                }

            }
        }
        #endregion
        #region Custom Operation
        private void StopPortCommunication(Models.DeviceType device)
        {

            var serialP = connectedDevices.Where(x => x.DeviceType == device).FirstOrDefault();
            if (serialP == null)
            {
                AddOutPut("No " + device.ToString() + " found..", (int)OutPutType.ERROR);
                return;
            }
            if (serialP.SerialDevice.IsOpen)
            {
                var pName = serialP.SerialDevice.PortName;
                AddOutPut("Closing Port " + pName + "..", (int)OutPutType.WARNING);
                serialP.SerialDevice.DtrEnable = false;
                serialP.SerialDevice.RtsEnable = false;
                switch (device)
                {
                    case Models.DeviceType.WeightModule:
                        serialP.SerialDevice.DataReceived -= WeightDevice_DataReceived;
                        break;
                }
                serialP.SerialDevice.DiscardInBuffer();
                serialP.SerialDevice.DiscardOutBuffer();
                serialP.SerialDevice.Close();
                AddOutPut("Serial Port " + pName + " is closed..", (int)OutPutType.WARNING);

                connectedDevices.Remove(serialP);
            }
        }

        private void SerialPortCommunications(ref SerialPort serialPort, int TypeofDevice = 0, string port = "", int baudRate = 0, int databit = 0, int stopBit = 0, int parity = 0)
        {
            if (TypeofDevice > 0)
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
                    switch (TypeofDevice)
                    {
                        case (int)Models.DeviceType.WeightModule:
                            serialPort.DataReceived += WeightDevice_DataReceived;
                            break;
                        case (int)Models.DeviceType.ControlCard:
                            serialPort.DataReceived += ControlDevice_DataReceived;
                            break;
                    }


                    serialPort.Open();
                }
                catch (Exception ex)
                {
                    AddOutPut("An error has occured : " + ex.Message.ToString(), (int)OutPutType.ERROR, true);
                }
                return;
            }
            AddOutPut("Serial port is busy..", (int)OutPutType.WARNING, true);
        }

        private void DataReader(Models.DeviceType device, string commandId)
        {
            var cDevices = connectedDevices.Where(x => x.DeviceType == device).FirstOrDefault();

            SB1Request _SB1Request = new SB1Request();

            if (cDevices.RecState > 0 && cDevices.IsComplete)
            {
                cDevices.IsComplete = false;


                //recState = 1;
                while (cDevices.ReceiveBufferQueue.Count > 0)
                {
                    recBufParse = cDevices.ReceiveBufferQueue.Dequeue();

                    cDevices.RecState = 1;
                    SB1Reply _reply = new SB1Reply(Common.GetSessionId);
                    SB1Handler _hndl = new SB1Handler(cDevices.SerialDevice);
                    UInt32 length = 32;
                    UInt32 payLoadSize = 0;
                    Byte[] payload;
                    Byte[] RxSB1 = recBufParse;
                    Byte MbAck;
                    UInt16 MbLength;
                    UInt16 Reserved;
                    Byte[] mbTgmBytes;

                    if (device == Models.DeviceType.ControlCard || device == Models.DeviceType.MotorDerive)
                    {
                        length = (uint)recBufParse[2] + 5;
                        payLoadSize = 0;
                        RxSB1 = recBufParse;
                    }
                    RecData _recData = cDevices.CurrentRequest;

                    //if (_reply.CheckCrc(recBufParse, Convert.ToInt32(_reply.length)))  // SB1 Check CRC
                    //{

                    if (_recData != null)
                    {
                        if (device == Models.DeviceType.WeightModule)
                        {

                            Common.GoodTmgm++;
                            foreach (var item in connectedDevices.Where(x => x.DeviceType == device))
                            {
                                item.CurrentRequest.MbTgm = recBufParse;
                                item.CurrentRequest.Status = PortDataStatus.Received;

                                foreach (var command in Commands.Where(x => x.CommandId == commandId))
                                {
                                    command.OutPutData = item.CurrentRequest;
                                }

                            }



                            return;
                        }


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

                        if (Common.COMSelected == COMType.MODBUS)
                        {
                            _MbTgmBytes = RxSB1;
                            MbLength = (ushort)_reply.length;
                        }
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

                                foreach (var item in connectedDevices.Where(x => x.DeviceType == device))
                                {
                                    item.CurrentRequest.MbTgm = mbTgmBytes;
                                    item.CurrentRequest.Status = PortDataStatus.Received;

                                    foreach (var command in Commands.Where(x => x.CommandId == commandId))
                                    {
                                        command.OutPutData = item.CurrentRequest;
                                    }
                                }

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
                                foreach (var item in connectedDevices.Where(x => x.DeviceType == device))
                                {
                                    item.CurrentRequest = _recData;
                                }
                            }

                        }
                    }



                    //}

                }

            }

        }

        private void WeightDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var WeightDevie = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.WeightModule).FirstOrDefault();

            if (WeightDevie == null)
            {
                return;
            }

            switch (WeightDevie.RecState)
            {
                case 0:
                    break;
                case 1:
                    foreach (var tom in connectedDevices.Where(w => w.DeviceType == Models.DeviceType.WeightModule))
                    {
                        int i = 0;
                        tom.RecIdx = 0;
                        tom.RecState = 1;
                        recBuf = new byte[tom.SerialDevice.BytesToRead];
                        tom.SerialDevice.Read(recBuf, 0, recBuf.Length);
                        tom.ReceiveBufferQueue = new Queue<byte[]>();
                        tom.ReceiveBufferQueue.Enqueue(recBuf);
                        tom.LastResponseReceived = DateTime.Now;
                        tom.IsComplete = true;
                        recBuf = new byte[REC_BUF_SIZE];
                    }
                    break;
            }
        }

        private void ControlDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var controlDevie = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.ControlCard).FirstOrDefault();

            if (controlDevie == null)
            {
                return;
            }
            switch (controlDevie.RecState)
            {
                case 0:
                    break;
                case 1:

                    foreach (var item in connectedDevices.Where(x => x.DeviceType == Models.DeviceType.ControlCard))
                    {
                        int i = 0;
                        item.RecIdx = 0;
                        item.RecState = 2;


                        while (item.SerialDevice.BytesToRead > 0)
                        {
                            byte[] rec = new byte[1];
                            item.RecIdx += item.SerialDevice.Read(rec, 0, 1);
                            recBuf[i] = rec[0];
                            i++;
                        }
                        if (item.RecIdx > 3)
                        {
                            TotalReceiveSize = (uint)recBuf[2] + 5;
                        }
                        if (TotalReceiveSize > item.RecIdx)
                        {
                            item.IsComplete = false;
                        }
                        else
                        {
                            item.IsComplete = true;
                            item.ReceiveBufferQueue = new Queue<byte[]>();
                            item.ReceiveBufferQueue.Enqueue(recBuf);
                            recBuf = new byte[REC_BUF_SIZE];
                        }
                        item.LastResponseReceived = DateTime.Now;
                    }
                    break;
            }
        }

        private void MotorDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var motorDevie = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.MotorDerive).FirstOrDefault();

            if (motorDevie == null)
            {
                return;
            }
            switch (motorDevie.RecState)
            {
                case 0:
                    break;
                case 1:

                    foreach (var item in connectedDevices.Where(x => x.DeviceType == Models.DeviceType.ControlCard))
                    {
                        int i = 0;
                        item.RecIdx = 0;
                        item.RecState = 2;

                        while (item.SerialDevice.BytesToRead > 0)
                        {
                            byte[] rec = new byte[1];
                            item.RecIdx += item.SerialDevice.Read(rec, 0, 1);
                            recBuf[i] = rec[0];
                            i++;
                        }
                        if (item.RecIdx > 3)
                        {
                            TotalReceiveSize = (uint)recBuf[2] + 5;
                        }
                        if (TotalReceiveSize > item.RecIdx)
                        {
                            item.IsComplete = false;
                        }
                        else
                        {
                            item.IsComplete = true;
                            item.ReceiveBufferQueue = new Queue<byte[]>();
                            item.ReceiveBufferQueue.Enqueue(recBuf);
                            recBuf = new byte[REC_BUF_SIZE];
                        }
                        item.LastResponseReceived = DateTime.Now;
                    }
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

        private void ConnectWeight(string Port, int Baudrate, int databit, int stopbit, int parity)
        {
            try
            {
                ConnectedDevices WeightConfig = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.WeightModule).FirstOrDefault();

                if (WeightConfig == null)
                {
                    _dispathcer.Invoke(new Action(() =>
                    {
                        WeightModuleArea.Visibility = Visibility.Visible;
                    }));

                    WeightConfig = new ConnectedDevices();
                    WeightConfig.RecState = 1;
                    WeightConfig.PortName = Port;
                    WeightConfig.BaudRate = Baudrate;
                    WeightConfig.DataBit = databit;
                    WeightConfig.StopBit = stopbit;
                    WeightConfig.Parity = parity;
                    RecData _recData = new RecData();
                    _recData.SessionId = Common.GetSessionNewId;
                    _recData.Ch = 0;
                    _recData.Indx = 0;
                    _recData.Reg = 0;
                    _recData.NoOfVal = 0;
                    _recData.Status = PortDataStatus.Requested;
                    WeightConfig.CurrentRequest = _recData;
                    WeightConfig.DeviceType = Models.DeviceType.WeightModule;
                    SerialPort WeightDevice = new SerialPort();
                    SerialPortCommunications(ref WeightDevice, (int)Models.DeviceType.WeightModule, Port, Baudrate, databit, stopbit, parity);
                    WeightConfig.SerialDevice = WeightDevice;
                    connectedDevices.Add(WeightConfig);
                }
            }
            catch
            {

            }
        }

        private void ReadAllControCardInputOutput()
        {
            var ControlConfig = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.WeightModule).FirstOrDefault();
            if (ControlConfig == null)
            {
                AddOutPut("Control Card is not connected..", (int)OutPutType.ERROR);
                return;
            }

            foreach (var item in connectedDevices.Where(x => x.DeviceType == Models.DeviceType.WeightModule))
            {
                item.RecState = 1;
                RecData _recData = new RecData();
                _recData.SessionId = Common.GetSessionNewId;
                _recData.Ch = 0;
                _recData.Indx = 0;
                _recData.Reg = 0;
                _recData.NoOfVal = 0;
                _recData.Status = PortDataStatus.Requested;
                item.CurrentRequest = _recData;
                item.IsComplete = false;

                MODBUSComnn obj = new MODBUSComnn();
                obj.GetMultiSendorValueFM3(1, 0, item.SerialDevice, 0, 30, "ControlCard", 1, 0, Models.DeviceType.ControlCard);
                break;
            }

        }

        private void ReadControCardState(int reg)
        {
            var ControlConfig = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.ControlCard).FirstOrDefault();
            if (ControlConfig == null)
            {
                AddOutPut("Control Card is not connected..", (int)OutPutType.ERROR);
                return;
            }

            foreach (var item in connectedDevices.Where(x => x.DeviceType == Models.DeviceType.ControlCard))
            {
                item.RecState = 1;
                RecData _recData = new RecData();
                _recData.SessionId = Common.GetSessionNewId;
                _recData.Ch = 0;
                _recData.Indx = 0;
                _recData.Reg = 0;
                _recData.NoOfVal = 0;
                _recData.Status = PortDataStatus.Requested;
                item.CurrentRequest = _recData;
                item.IsComplete = false;
                MODBUSComnn obj = new MODBUSComnn();
                obj.GetMultiSendorValueFM3(1, 0, item.SerialDevice, reg, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard);
                break;
            }


        }

        private void WriteControCardState(int reg, int val)
        {
            var ControlConfig = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.ControlCard).FirstOrDefault();
            if (ControlConfig == null)
            {
                AddOutPut("Control Card is not connected..", (int)OutPutType.ERROR);
                return;
            }

            foreach (var item in connectedDevices.Where(x => x.DeviceType == Models.DeviceType.ControlCard))
            {
                item.RecState = 1;
                RecData _recData = new RecData();
                _recData.SessionId = Common.GetSessionNewId;
                _recData.Ch = 0;
                _recData.Indx = 0;
                _recData.Reg = 0;
                _recData.NoOfVal = 0;
                _recData.Status = PortDataStatus.Requested;
                item.CurrentRequest = _recData;
                item.IsComplete = false;

                MODBUSComnn obj = new MODBUSComnn();
                int[] _val = new int[2] { 0, val };
                obj.SetMultiSendorValueFM16(1, 0, item.SerialDevice, reg + 1, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard, _val, false);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);
                break;
            }

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

                                Connect_control_card(command.Configuration.deviceDetail.PortName, command.Configuration.deviceDetail.BaudRate, command.Configuration.deviceDetail.DataBit, command.Configuration.deviceDetail.StopBit, command.Configuration.deviceDetail.Parity);
                                command.ExecutionStatus = (int)ExecutionStage.Executed;

                            }
                        }

                        else if (command.CommandType == (int)ElementConstant.Disconnect_ControlCard_Event)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    ControlBoardArea.Visibility = Visibility.Hidden;
                                }));

                                //AddOutPut("Disconnecting Control Card..", (int)OutPutType.INFORMATION);
                                StopPortCommunication(Models.DeviceType.ControlCard);
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                //AddOutPut("Control card is disconnected..", (int)OutPutType.SUCCESS, true);

                            }

                        }

                        else if (command.CommandType == (int)ElementConstant.Connect_Weight_Event)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                //AddOutPut("Connecting weight module..", (int)OutPutType.INFORMATION);
                                ConnectWeight(command.Configuration.deviceDetail.PortName, command.Configuration.deviceDetail.BaudRate, command.Configuration.deviceDetail.DataBit, command.Configuration.deviceDetail.StopBit, command.Configuration.deviceDetail.Parity);
                                command.ExecutionStatus = (int)ExecutionStage.Executed;

                                //AddOutPut("weight module is connected..", (int)OutPutType.SUCCESS, true);
                                //AddOutPut("Waiting for weight module response..", (int)OutPutType.INFORMATION);
                                //Thread.Sleep(500);
                            }
                            else
                            {
                                //AddOutPut("Started Fetching weight module input..", (int)OutPutType.SUCCESS, true);
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                //AddOutPut("Fetching Stopped..", (int)OutPutType.SUCCESS, true);
                            }
                        }

                        else if (command.CommandType == (int)ElementConstant.Disconnect_Weight_Event)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                _dispathcer.Invoke(new Action(() =>
                                {
                                    WeightModuleArea.Visibility = Visibility.Hidden;
                                }));

                                //AddOutPut("Disconnecting weight module..", (int)OutPutType.WARNING, true);
                                StopPortCommunication(Models.DeviceType.WeightModule);
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                //AddOutPut("Weight module disconnected..", (int)OutPutType.SUCCESS, true);

                            }
                            else
                            {
                                //OutPutArea.AppendText("\n Executing " + command.CommandText + "..");
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                            }
                        }

                        else if (command.CommandType == (int)ElementConstant.Read_Weight)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                               // AddOutPut("Reading data..", (int)OutPutType.INFORMATION);
                                bool received = false;
                                RecData _rec = new RecData();
                                while (received == false)
                                {
                                    var cDevices = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.WeightModule).FirstOrDefault();
                                    DataReader(Models.DeviceType.WeightModule, command.CommandId);
                                    if (cDevices.CurrentRequest.MbTgm != null)
                                    {
                                        if (cDevices.CurrentRequest.MbTgm.Length > 0)
                                        {
                                            _rec = new RecData();
                                            _rec = cDevices.CurrentRequest;
                                            if (validateResponse(_rec))
                                            {
                                                received = true;
                                            }
                                        }
                                    }
                                }

                                //AddOutPut("Storing data into " + command.CommandText + "..", (int)OutPutType.INFORMATION);
                                //command.OutPutData = _rec;
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                //AddOutPut("Showing weight module response..", (int)OutPutType.INFORMATION);
                                showWeightModuleResponse(_rec);

                            }
                            else
                            {
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                            }
                        }

                        else if (command.CommandType == (int)ElementConstant.Read_All_Card_In_Out)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                AddOutPut("Fetching control card all input/output information", (int)OutPutType.INFORMATION);
                                ReadAllControCardInputOutput();
                                command.ExecutionStatus = (int)ExecutionStage.Executing;

                                AddOutPut("Reading control card all input/output information", (int)OutPutType.INFORMATION);
                                bool received = false;
                                RecData _rec = new RecData();
                                while (received == false)
                                {
                                    var cDevices = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.WeightModule).FirstOrDefault();
                                    DataReader(Models.DeviceType.ControlCard, command.CommandId);
                                    if (cDevices.ReceiveBufferQueue.Count > 0)
                                    {
                                        _rec = new RecData();
                                        _rec = cDevices.CurrentRequest;
                                        received = true;
                                    }
                                }
                                AddOutPut("Storing output..", (int)OutPutType.INFORMATION);
                                //command.OutPutData = _rec;
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                AddOutPut("Showing control card state..", (int)OutPutType.INFORMATION);
                                ReadControlCardResponse(_rec);
                            }
                            else if (command.ExecutionStatus == (int)ExecutionStage.Executing)
                            {
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                            }
                        }

                        else if (command.CommandType == (int)ElementConstant.Write_Card_Out)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                            {
                                var functioncommand = (RegisterWriteCommand)command.CommandData;
                                string commandText = functioncommand.RegisterOutput == 1 ? "ON" : "OFF";
                                AddOutPut("Writing control card register " + functioncommand.RegisterNumber + " " + commandText, (int)OutPutType.WARNING, true);
                                WriteControCardState(functioncommand.RegisterNumber, functioncommand.RegisterOutput);
                                AddOutPut("Control card register " + functioncommand.RegisterNumber + " is " + commandText, (int)OutPutType.SUCCESS, true);
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                                WriteControlCardResponse(functioncommand.RegisterNumber, functioncommand.RegisterOutput);
                            }
                            else
                            {
                                //OutPutArea.AppendText("\n Executing " + command.CommandText + "..");
                                command.ExecutionStatus = (int)ExecutionStage.Executed;
                            }
                        }

                        else if (command.CommandType == (int)ElementConstant.Stop_Repeat)
                        {
                            //var ifcommand = Commands.Where(x => (x.CommandType == (int)ElementConstant.If_Condition_Start || x.CommandType == (int)ElementConstant.Else_If_Start || x.CommandType == (int)ElementConstant.Else_Start) && x.Order < command.Order).OrderBy(x => x.Order).ToList().FirstOrDefault();

                            var repeater = Commands.Where(x => x.CommandType == (int)ElementConstant.Repeat_Control).OrderBy(x => x.Order).ToList().LastOrDefault();

                            if (repeater != null)
                            {
                                foreach (var item in Commands.Where(x => x.Order > repeater.Order && x.Order < command.Order))
                                {
                                    item.ExecutionStatus = (int)ExecutionStage.Not_Executed;
                                    command.ExecutionStatus = (int)ExecutionStage.Not_Executed;
                                    AddOutPut("Repeater Scope Ended..", (int)OutPutType.SUCCESS, true);

                                }
                                AddOutPut("Repeating again..", (int)OutPutType.SUCCESS, true);
                            }
                            else
                            {
                                AddOutPut("Ended..", (int)OutPutType.SUCCESS, true);
                            }
                            Task.Delay(200);
                        }

                        else if (command.CommandType == (int)ElementConstant.If_Condition_Start)
                        {
                            if (command.ExecutionStatus == (int)ExecutionStage.Not_Executed || command.ExecutionStatus == (int)ExecutionStage.Executing)
                            {
                                bool isConditionTrue = false;
                                command.ExecutionStatus = (int)ExecutionStage.Executing;
                                int ComparisonVariableVal = 0;
                                if (command.InputData.ComparisonVariable.Contains('-'))
                                {
                                    var input = command.InputData.ComparisonVariable.Split('-')[0];
                                    var register = Convert.ToInt32(command.InputData.ComparisonVariable.Split('-')[1]);
                                    GetControlCardInputOutputState(Commands.Where(x => x.CommandId == input).FirstOrDefault().OutPutData);

                                    ComparisonVariableVal = registerOutputStatuses.Where(x => x.Register == register).FirstOrDefault().RegisterStatus;

                                    if (ComparisonVariableVal == Convert.ToInt32(command.InputData.ComparisonValue))
                                    {
                                        isConditionTrue = true;
                                    }

                                }
                                else
                                {
                                    double weight = getWeightModuleResponse(Commands.Where(x => x.CommandId == command.InputData.ComparisonVariable).FirstOrDefault().OutPutData);
                                    isConditionTrue = CompareWeightdata(command.InputData.ComparisonCondition, weight, command.InputData.ComparisonValue);
                                }

                                var endscope = Commands.Where(x => x.ParentCommandId == command.CommandId).FirstOrDefault
                                   ().Order;

                                if (isConditionTrue)
                                {
                                    foreach (var Scopecommand in Commands.Where(x => x.Order > command.Order && x.Order < endscope && x.ExecutionStatus != (int)ExecutionStage.Executed).ToList())
                                    {
                                        if (Scopecommand.CommandType == (int)ElementConstant.Connect_ControlCard_Event)
                                        {
                                            if (Scopecommand.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                                            {

                                                Connect_control_card(Scopecommand.Configuration.deviceDetail.PortName, Scopecommand.Configuration.deviceDetail.BaudRate, Scopecommand.Configuration.deviceDetail.DataBit, Scopecommand.Configuration.deviceDetail.StopBit, Scopecommand.Configuration.deviceDetail.Parity);
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;

                                            }
                                        }

                                        else if (Scopecommand.CommandType == (int)ElementConstant.Disconnect_ControlCard_Event)
                                        {
                                            if (Scopecommand.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                                            {
                                                AddOutPut("Disconnecting Control Card..", (int)OutPutType.INFORMATION);
                                                StopPortCommunication(Models.DeviceType.ControlCard);
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                                AddOutPut("Control card is disconnected..", (int)OutPutType.SUCCESS, true);

                                            }

                                        }

                                        else if (Scopecommand.CommandType == (int)ElementConstant.Connect_Weight_Event)
                                        {
                                            if (Scopecommand.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                                            {
                                                AddOutPut("Connecting weight module..", (int)OutPutType.INFORMATION);
                                                ConnectWeight(Scopecommand.Configuration.deviceDetail.PortName, Scopecommand.Configuration.deviceDetail.BaudRate, Scopecommand.Configuration.deviceDetail.DataBit, Scopecommand.Configuration.deviceDetail.StopBit, Scopecommand.Configuration.deviceDetail.Parity);
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;

                                                AddOutPut("weight module is connected..", (int)OutPutType.SUCCESS, true);
                                                AddOutPut("Waiting for weight module response..", (int)OutPutType.INFORMATION);
                                                //Thread.Sleep(500);
                                            }
                                            else
                                            {
                                                AddOutPut("Started Fetching weight module input..", (int)OutPutType.SUCCESS, true);
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                                AddOutPut("Fetching Stopped..", (int)OutPutType.SUCCESS, true);
                                            }
                                        }

                                        else if (Scopecommand.CommandType == (int)ElementConstant.Disconnect_Weight_Event)
                                        {
                                            if (Scopecommand.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                                            {
                                                AddOutPut("Disconnecting weight module..", (int)OutPutType.WARNING, true);
                                                StopPortCommunication(Models.DeviceType.WeightModule);
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                                AddOutPut("Weight module disconnected..", (int)OutPutType.SUCCESS, true);

                                            }
                                            else
                                            {
                                                //OutPutArea.AppendText("\n Executing " + Scopecommand.CommandText + "..");
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                            }
                                        }

                                        else if (Scopecommand.CommandType == (int)ElementConstant.Read_Weight)
                                        {
                                            if (Scopecommand.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                                            {
                                                AddOutPut("Reading data..", (int)OutPutType.INFORMATION);
                                                bool received = false;
                                                RecData _rec = new RecData();
                                                while (received == false)
                                                {
                                                    var cDevices = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.WeightModule).FirstOrDefault();
                                                    DataReader(Models.DeviceType.WeightModule, Scopecommand.CommandId);
                                                    if (cDevices.ReceiveBufferQueue.Count > 0)
                                                    {
                                                        _rec = new RecData();
                                                        _rec = cDevices.CurrentRequest;
                                                        if (validateResponse(_rec))
                                                        {
                                                            received = true;
                                                        }
                                                    }
                                                }

                                                AddOutPut("Storing data into " + Scopecommand.CommandText + "..", (int)OutPutType.INFORMATION);
                                                //Scopecommand.OutPutData = _rec;
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                                AddOutPut("Showing weight module response..", (int)OutPutType.INFORMATION);
                                                showWeightModuleResponse(_rec);

                                            }
                                            else
                                            {
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                            }
                                        }

                                        else if (Scopecommand.CommandType == (int)ElementConstant.Read_All_Card_In_Out)
                                        {
                                            if (Scopecommand.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                                            {
                                                AddOutPut("Fetching control card all input/output information", (int)OutPutType.INFORMATION);
                                                ReadAllControCardInputOutput();
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executing;

                                                AddOutPut("Reading control card all input/output information", (int)OutPutType.INFORMATION);
                                                bool received = false;
                                                RecData _rec = new RecData();
                                                while (received == false)
                                                {
                                                    var cDevices = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.WeightModule).FirstOrDefault();
                                                    DataReader(Models.DeviceType.ControlCard, Scopecommand.CommandId);
                                                    if (cDevices.ReceiveBufferQueue.Count > 0)
                                                    {
                                                        _rec = new RecData();
                                                        _rec = cDevices.CurrentRequest;
                                                        received = true;
                                                    }
                                                }
                                                AddOutPut("Storing output..", (int)OutPutType.INFORMATION);
                                                //Scopecommand.OutPutData = _rec;
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                                AddOutPut("Showing control card state..", (int)OutPutType.INFORMATION);
                                                ReadControlCardResponse(_rec);
                                            }
                                            else if (Scopecommand.ExecutionStatus == (int)ExecutionStage.Executing)
                                            {
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                            }
                                        }

                                        else if (Scopecommand.CommandType == (int)ElementConstant.Write_Card_Out)
                                        {
                                            if (Scopecommand.ExecutionStatus == (int)ExecutionStage.Not_Executed)
                                            {
                                                var functionScopecommand = (RegisterWriteCommand)Scopecommand.CommandData;
                                                string ScopecommandText = functionScopecommand.RegisterOutput == 1 ? "ON" : "OFF";
                                                AddOutPut("Writing control card register " + functionScopecommand.RegisterNumber + " " + ScopecommandText, (int)OutPutType.WARNING, true);
                                                WriteControCardState(functionScopecommand.RegisterNumber, functionScopecommand.RegisterOutput);
                                                AddOutPut("Control card register " + functionScopecommand.RegisterNumber + " is " + ScopecommandText, (int)OutPutType.SUCCESS, true);
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                                WriteControlCardResponse(functionScopecommand.RegisterNumber, functionScopecommand.RegisterOutput);
                                            }
                                            else
                                            {
                                                //OutPutArea.AppendText("\n Executing " + Scopecommand.CommandText + "..");
                                                Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                            }
                                        }
                                        else if (Scopecommand.CommandType == (int)ElementConstant.Connect_Camera_Event)
                                        {
                                            //AddOutPut("Starting Camera..", (int)OutPutType.INFORMATION, true);
                                            //var isStarted = ConnectionUSB();
                                            Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                            StartUSBCamRecording();
                                            AddOutPut("Camera Recording Starts..", (int)OutPutType.INFORMATION, true);
                                            //var RecordPath = StartVideoCapture(_FileDirectory);
                                            AddOutPut("Camera is recording at " + VideoFileDirectory + " ..", (int)OutPutType.INFORMATION, true);

                                        }
                                        else if (Scopecommand.CommandType == (int)ElementConstant.Disconnect_Camera_Event)
                                        {
                                            // AddOutPut("Stoping Camera..", (int)OutPutType.INFORMATION, true);
                                            StopUSBCamRecording();
                                            AddOutPut("Camera Recording Stopped..", (int)OutPutType.SUCCESS, true);
                                            Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                            //StopVideoCapture();
                                        }

                                        else if (Scopecommand.CommandType == (int)ElementConstant.End_Scope)
                                        {
                                            var ifScopecommand = Commands.Where(x => (x.CommandType == (int)ElementConstant.If_Condition_Start || x.CommandType == (int)ElementConstant.Else_If_Start || x.CommandType == (int)ElementConstant.Else_Start) && x.Order < Scopecommand.Order).OrderBy(x => x.Order).ToList().FirstOrDefault();

                                            var repeater = Commands.Where(x => x.CommandType == (int)ElementConstant.Repeat_Control && x.Order < Scopecommand.Order).OrderBy(x => x.Order).ToList().FirstOrDefault();

                                            if (ifScopecommand != null)
                                            {
                                                foreach (var item in Commands)
                                                {
                                                    if (ifScopecommand.CommandId == item.CommandId)
                                                    {
                                                        item.ExecutionStatus = (int)ExecutionStage.Executed;
                                                        Scopecommand.ExecutionStatus = (int)ExecutionStage.Executed;
                                                        AddOutPut("If Scope Ended..", (int)OutPutType.SUCCESS, true);
                                                    }
                                                }
                                            }
                                            else if (repeater != null)
                                            {
                                                AddOutPut("Repeating again..", (int)OutPutType.SUCCESS, true);
                                            }
                                            else
                                            {
                                                AddOutPut("Ended..", (int)OutPutType.SUCCESS, true);
                                            }

                                        }


                                    }

                                    var totalcomand = Commands.Where(x => x.Order == (endscope - 1)).FirstOrDefault();
                                    if (totalcomand.ExecutionStatus == (int)ExecutionStage.Executed)
                                    {
                                        command.ExecutionStatus = (int)ExecutionStage.Executed;
                                        Commands.Where(x => x.Order > command.Order && x.Order < endscope).ToList().ForEach(x => x.ExecutionStatus = (int)ExecutionStage.Executed);

                                    }
                                    break;
                                }
                                else
                                {
                                    command.ExecutionStatus = (int)ExecutionStage.Executed;
                                    Commands.Where(x => x.Order > command.Order && x.Order < endscope).ToList().ForEach(x => x.ExecutionStatus = (int)ExecutionStage.Executed);

                                }
                            }

                        }

                        else if (command.CommandType == (int)ElementConstant.Repeat_Control)
                        {
                            //Task.Delay(2000);
                           // AddOutPut("Repeater loop started..", (int)OutPutType.INFORMATION, true);
                        }
                        else if (command.CommandType == (int)ElementConstant.Connect_Camera_Event)
                        {

                            StartUSBCamRecording();
                            command.ExecutionStatus = (int)ExecutionStage.Executed;
                            //AddOutPut("Camera Recording Starts..", (int)OutPutType.INFORMATION, true);
                            //var RecordPath = StartVideoCapture(_FileDirectory);
                           // AddOutPut("Camera is recording at " + VideoFileDirectory + " ..", (int)OutPutType.INFORMATION, true);

                        }
                        else if (command.CommandType == (int)ElementConstant.Disconnect_Camera_Event)
                        {
                            //AddOutPut("Stoping Camera..", (int)OutPutType.INFORMATION, true);
                            StopUSBCamRecording();
                            AddOutPut("Camera Recording Stopped..", (int)OutPutType.SUCCESS, true);
                            command.ExecutionStatus = (int)ExecutionStage.Executed;


                        }


                    }
                    //ExecuteProcesses();

                }


            }
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

                Regex re = new Regex(@"([a-zA-Z]+)(\d+)");
                Match result = re.Match(outP);

                string alphaPart = result.Groups[1].Value;
                string numberPart = result.Groups[2].Value;
                weight = Convert.ToInt32(numberPart) - 3067;
                weight = Convert.ToDouble(weight / 260.4);
                weight = Math.Round(weight, 2);
                return weight;


                if (outP.Contains("kg"))
                {
                    StringBuilder builder = new StringBuilder(outP);
                    builder.Replace("kg", "");
                    weight = Convert.ToInt32(builder) - 3067;
                    weight = Convert.ToDouble(weight / 260.4);
                    weight = weight * 0.453592;
                    weight = Math.Round(weight, 2);
                }
                else if (outP.Contains("kgg"))
                {
                    StringBuilder builder = new StringBuilder(outP);
                    builder.Replace("kgg", "");
                    weight = Convert.ToInt32(builder) - 3067;
                    weight = Convert.ToDouble(weight / 260.4);
                    weight = weight * 0.453592;
                    weight = Math.Round(weight, 2);
                }
                else if (outP.Contains("kgn"))
                {
                    StringBuilder builder = new StringBuilder(outP);
                    builder.Replace("kgn", "");
                    weight = Convert.ToInt32(builder) - 3067;
                    weight = Convert.ToDouble(weight / 260.4);
                    weight = weight * 0.453592;
                    weight = Math.Round(weight, 2);
                }
                else if (outP.Contains("lbs"))
                {

                    Regex regex = new Regex(@"([a-zA-Z]+)(\d+)");
                    Match result1 = regex.Match(outP);

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
                    Match result1 = regex.Match(outP);

                    string alphaPart1 = result.Groups[1].Value;
                    string numberPart1 = result.Groups[2].Value;
                    weight = Convert.ToInt32(numberPart1) - 3067;
                    weight = Convert.ToDouble(weight / 260.4);
                    weight = Math.Round(weight, 2);
                }

                //for (int i = 0; i < data.Length; i++)
                //{
                //    if (!string.IsNullOrEmpty(data[i]) || !string.IsNullOrWhiteSpace(data[i]))
                //    {
                //        if (data[i].All(char.IsDigit))
                //        {
                //            weight = Convert.ToInt32(data[i].ToString().Trim());

                //            continue;
                //        }

                //        try
                //        {
                //            var outP = data[i].ToLower().ToString();
                //            if (outP.Contains("kgg"))
                //            {
                //                StringBuilder builder = new StringBuilder(outP);
                //                builder.Replace("kgg", "");
                //                weight = Convert.ToInt32(builder.ToString());
                //            }
                //        }
                //        catch (Exception ex)
                //        {

                //        }
                //    }

                //}

            }
            return weight; //TimerCheckReceiveData.Enabled = true;
        }

        private void Connect_control_card(string Port, int Baudrate, int databit, int stopbit, int parity)
        {

            try
            {
                ConnectedDevices controlConfig = connectedDevices.Where(x => x.DeviceType == Models.DeviceType.ControlCard).FirstOrDefault();

                if (controlConfig == null)
                {
                    _dispathcer.Invoke(new Action(() =>
                    {
                        ControlBoardArea.Visibility = Visibility.Visible;
                    }));

                    AddOutPut("Initializing control card port..", (int)OutPutType.INFORMATION);
                    controlConfig = new ConnectedDevices();
                    controlConfig.RecState = 1;
                    controlConfig.PortName = Port;
                    controlConfig.BaudRate = Baudrate;
                    controlConfig.DataBit = databit;
                    controlConfig.StopBit = stopbit;
                    controlConfig.Parity = parity;
                    RecData _recData = new RecData();
                    _recData.SessionId = Common.GetSessionNewId;
                    _recData.Ch = 0;
                    _recData.Indx = 0;
                    _recData.Reg = 0;
                    _recData.NoOfVal = 0;
                    _recData.Status = PortDataStatus.Requested;
                    controlConfig.CurrentRequest = _recData;
                    controlConfig.DeviceType = Models.DeviceType.ControlCard;
                    controlConfig.RecState = 0;
                    SerialPort WeightDevice = new SerialPort();
                    SerialPortCommunications(ref WeightDevice, (int)Models.DeviceType.ControlCard, Port, Baudrate, databit, stopbit, parity);
                    controlConfig.SerialDevice = WeightDevice;
                    AddOutPut("Connecting control card..", (int)OutPutType.INFORMATION);
                    connectedDevices.Add(controlConfig);
                }
            }
            catch (Exception ex)
            {
                AddOutPut("An error has occured : " + ex.Message.ToString(), (int)OutPutType.ERROR, true);
                return;
            }

            AddOutPut("Control card is connected..", (int)OutPutType.SUCCESS, true);
        }

        private bool CompareWeightdata(int ComparisonCondition, double weightdata, string comparisonValue)
        {
            bool isConditionTrue = false;
            if (ComparisonCondition == (int)ConditionConstant.contains)
            {
                if (weightdata.ToString().Contains(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.does_not_contains)
            {
                if (!weightdata.ToString().Contains(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.is_equal_to)
            {
                if (weightdata == Convert.ToDouble(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.is_not_equal_to)
            {
                if (weightdata != Convert.ToDouble(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.is_greater_then)
            {
                if (weightdata > Convert.ToDouble(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.is_greater_then_or_equal_to)
            {
                if (weightdata >= Convert.ToDouble(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.is_less_then)
            {
                if (weightdata < Convert.ToDouble(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.is_less_then_or_equal_to)
            {
                if (weightdata <= Convert.ToDouble(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.starts_with)
            {
                if (weightdata.ToString().StartsWith(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.does_not_start_with)
            {
                if (!weightdata.ToString().StartsWith(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.ends_with)
            {
                if (weightdata.ToString().EndsWith(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }
            else if (ComparisonCondition == (int)ConditionConstant.does_not_ends_with)
            {
                if (!weightdata.ToString().EndsWith(comparisonValue))
                {
                    isConditionTrue = true;

                }
            }

            return isConditionTrue;
        }




        #endregion

        #region Output Function
        private void AddOutPut(string Output, int MessageType, bool isBold = false)
        {
            //Task.Delay(200);
            try
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
                _dispathcer.Invoke(new Action(() => { OutPutControl.Blocks.Add(myParagraph); }));
               // OutPutControl.Blocks.Add(myParagraph);
            }
            catch (Exception ex) { return; }

        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //AddOutPut("Commands Execution started..", (int)OutPutType.INFORMATION);
            //Task.Delay(1000);
            // ConnectionUSB();
            //ExecuteProcesses();
            //AddOutPut("Commands compilation completed..", (int)OutPutType.SUCCESS);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.Content = "Execution Started";
            btn.Background = Brushes.Red;
            btn.IsEnabled = false;
           //StartUSBCamera();
            //WebcamViewer.StartPreview();
            //AddOutPut("Commands Execution started..", (int)OutPutType.INFORMATION);
            ExecuteProcesses();
            //AddOutPut("Commands compilation completed..", (int)OutPutType.SUCCESS);

        }
    }
}
