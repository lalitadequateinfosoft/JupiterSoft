using Components;
using JupiterSoft.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Ozeki;
using Ozeki.Camera;
using Ozeki.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Util;
using System.Threading;
using System.Threading.Tasks;
using JupiterSoft.CustomDailog;

namespace JupiterSoft.Pages
{
    /// <summary>
    /// Interaction logic for CreateTemplate.xaml
    /// </summary>
    public partial class CreateTemplate : Page
    {

        #region Custom Property Declaration.
        public static readonly DependencyProperty IsChildHitTestVisibleProperty =
            DependencyProperty.Register("IsChildHitTestVisible", typeof(bool), typeof(CreateTemplate),
                new PropertyMetadata(true));

        public bool IsChildHitTestVisible
        {
            get { return (bool)GetValue(IsChildHitTestVisibleProperty); }
            set { SetValue(IsChildHitTestVisibleProperty, value); }
        }

        #endregion

        #region Global Variables

        private bool write = false;
        const int REC_BUF_SIZE = 10000;
        byte[] recBuf = new byte[REC_BUF_SIZE];
        byte[] recBufParse = new byte[REC_BUF_SIZE];

        public Byte[] _MbTgmBytes;
        internal UInt32 TotalReceiveSize = 0;
        bool IsComplete = false;


        System.Timers.Timer TimerCheckReceiveData = new System.Timers.Timer();
        System.Timers.Timer MotorTimer = new System.Timers.Timer();
        internal bool _UpdatePB = true;
        internal bool UseThisPage = true;
        static readonly object _object = new object();
        string _CurrentActiveMenu = "Modbus";

        Point Offset;
        WrapPanel dragObject;
        bool isDragged = false;
        bool isloaded = false;
        string _FileDirectory = ApplicationConstant._FileDirectory;
        string _VideoDirectory = ApplicationConstant._VideoDirectory;

        BrushConverter bc;
        ElementModel UElement;
        double CanvasWidth;
        double CanvasHeight;


        Dashboard parentWindow;
        public Dashboard ParentWindow
        {
            get { return parentWindow; }
            set { parentWindow = value; }
        }

        // Camera variables.
        private IIPCamera _camera;
        private DrawingImageProvider _drawingImageProvider;
        private MediaConnector _connector;
        private IWebCamera _webCamera;
        private static string _runningCamera = null;
        private MJPEGStreamer _streamer;
        private IVideoSender _videoSender;

        private string _CurrentFile = null;
        private List<DiscoveredDeviceInfo> devices;
        private List<DeviceModel> DeviceModels;
        private DeviceInfo deviceInfo;

        //Serial Port Com
        private CommandExecutionModel userCommandLogic;
        private SerialPort SerialDevice;
        private Dispatcher _dispathcer;
        private int readIndex = 0;
        private MPEG4Recorder _mpeg4Recorder;

        private List<LogicalCommand> Commands;

        #endregion

        private float _currentAngle;
        private float motorspeed = 0;
        private bool? isMotorRunning;


        public CreateTemplate()
        {
            bc = new BrushConverter();
            userCommandLogic = new CommandExecutionModel();
            this.UElement = ElementOp.GetElementModel();
            InitializeComponent();

            this.DataContext = this.UElement;
            this.CanvasWidth = ReceiveDrop.Width;
            this.CanvasHeight = ReceiveDrop.Height;
            this.isloaded = true;

            devices = new List<DiscoveredDeviceInfo>();
            DeviceModels = new List<DeviceModel>();

            deviceInfo = DeviceInformation.GetConnectedDevices();
            ConnectedDevices();
            LoadSystemSound();

            _dispathcer = Dispatcher.CurrentDispatcher;
            this.SerialDevice = new SerialPort();

            Commands = new List<LogicalCommand>();
        }
        public CreateTemplate(string _filename)
        {
            bc = new BrushConverter();
            userCommandLogic = new CommandExecutionModel();
            this.UElement = ElementOp.GetElementModel();
            InitializeComponent();

            this.DataContext = this.UElement;
            this.CanvasWidth = ReceiveDrop.Width;
            this.CanvasHeight = ReceiveDrop.Height;
            this.isloaded = true;


            devices = new List<DiscoveredDeviceInfo>();
            DeviceModels = new List<DeviceModel>();

            deviceInfo = DeviceInformation.GetConnectedDevices();
            ConnectedDevices();
            LoadSystemSound();

            Commands = new List<LogicalCommand>();

            //Saved project.
            _CurrentFile = _filename;
            this.ProjectName.Content = System.IO.Path.GetFileName(_filename).Split('.')[0];
            LoadFile();
        }

        #region UI Functions
        private void ButtonGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            var sArea = sender as StackPanel;
            if (sArea.Name == "ButtonGridArea1")
            {
                buttonGrid1.Opacity = 0.5;
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (sArea.Name == "ButtonGridArea2")
            {
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.Opacity = 0.5;
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (sArea.Name == "ButtonGridArea3")
            {
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.Opacity = 0.5;
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (sArea.Name == "ButtonGridArea4")
            {
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.Opacity = 0.5;
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (sArea.Name == "ButtonGridArea5")
            {
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.Opacity = 0.5;
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (sArea.Name == "ButtonGridArea6")
            {
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.Opacity = 0.5;
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (sArea.Name == "ButtonGridArea7")
            {
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.Opacity = 0.5;
            }
            else
            {
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
        }

        private void ButtonGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonGrid1.ClearValue(UIElement.OpacityProperty);
            buttonGrid2.ClearValue(UIElement.OpacityProperty);
            buttonGrid3.ClearValue(UIElement.OpacityProperty);
            buttonGrid4.ClearValue(UIElement.OpacityProperty);
            buttonGrid5.ClearValue(UIElement.OpacityProperty);
            buttonGrid6.ClearValue(UIElement.OpacityProperty);
            buttonGrid7.ClearValue(UIElement.OpacityProperty);

        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cSender = sender as Border;
            UIElement element = VisualTreeHelper.GetParent(cSender) as UIElement;
            string pName = (element as Grid).Name;
            element.Opacity = 0.5;
            if (pName == "buttonGrid1")
            {
                ButtonGridArea1.BringIntoView();
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (pName == "buttonGrid2")
            {
                ButtonGridArea2.BringIntoView();
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (pName == "buttonGrid3")
            {
                ButtonGridArea3.BringIntoView();
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                //buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (pName == "buttonGrid4")
            {
                ButtonGridArea4.BringIntoView();
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                //buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (pName == "buttonGrid5")
            {
                ButtonGridArea5.BringIntoView();
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                //buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (pName == "buttonGrid6")
            {
                ButtonGridArea6.BringIntoView();
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                //buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if (pName == "buttonGrid7")
            {
                ButtonGridArea7.BringIntoView();
                buttonGrid1.ClearValue(UIElement.OpacityProperty);
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                //buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
        }
        #endregion

        #region Drag and Drop Function

        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UIElement copy = sender as UIElement;
                var propdata = copy.GetValue(FrameworkElement.TagProperty);
                if (propdata != null)
                {
                    IsChildHitTestVisible = false;
                    DataObject dragData = new DataObject();
                    dragData.SetData(DataFormats.StringFormat, propdata.ToString());
                    DragDrop.DoDragDrop(this, dragData, DragDropEffects.Copy);
                    IsChildHitTestVisible = true;
                    isDragged = true;

                }

            }

            e.Handled = true;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (!isDragged) return;
            bool ShouldAdd = false;
            var data = e.Data.GetData(typeof(string));
            if (data != null)
            {
                Point dropPosition = e.GetPosition(ReceiveDrop);
                double NewTop = dropPosition.Y;
                double NewLeft = dropPosition.X;
                WrapPanel ele = new WrapPanel();
                var contentId = Guid.NewGuid().ToString("N");
                switch (Convert.ToInt32(data))
                {
                    case (int)ElementConstant.Ten_Steps_Move:
                        getNewPosition(Ten_Steps_Move.Width, Ten_Steps_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Ten_Steps_Move(contentId);
                        break;
                    case (int)ElementConstant.Turn_Fiften_Degree_Right_Move:
                        getNewPosition(Turn_Fiften_Degree_Right_Move.Width, Turn_Fiften_Degree_Right_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Turn_Fiften_Degree_Right_Move(contentId);
                        break;
                    case (int)ElementConstant.Turn_Fiften_Degree_Left_Move:
                        getNewPosition(Turn_Fiften_Degree_Left_Move.Width, Turn_Fiften_Degree_Left_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Turn_Fiften_Degree_Left_Move(contentId);
                        break;
                    case (int)ElementConstant.Pointer_State_Move:
                        getNewPosition(Pointer_State_Move.Width, Pointer_State_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Pointer_State_Move(contentId);
                        break;
                    case (int)ElementConstant.Rotation_Style_Move:
                        getNewPosition(Rotation_Style_Move.Width, Rotation_Style_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Rotation_Style_Move(contentId);
                        break;
                    case (int)ElementConstant.Start_Event:
                        getNewPosition(Start_Event.Width, Start_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        ShouldAdd = true;
                        break;
                    case (int)ElementConstant.Connect_Motor_Event:
                        getNewPosition(Connect_Motor_Event.Width, Connect_Motor_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        ConfigurationDailog Motordailog = new ConfigurationDailog("Set Motor Configuration", deviceInfo.CustomDeviceInfos);
                        Motordailog.ShowDialog();
                        if (!Motordailog.Canceled)
                        {
                            if (Commands.Count() == 0)
                            {
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration
                                    {
                                        deviceDetail = new DeviceSettings
                                        {
                                            DeviceId = Motordailog.DeviceId,
                                            PortName = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == Motordailog.DeviceId).FirstOrDefault().PortName,
                                            BaudRate = Motordailog.BaudRate,
                                            DataBit = Motordailog.Databit,
                                            StopBit = Motordailog.Stopbit,
                                            Parity = Motordailog.ParityValue
                                        },
                                        cameraDetail = new CameraSettings()
                                    },
                                    CommandText = "Motor Drive"
                                };
                                Commands.Add(command);
                            }
                            else
                            {
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration
                                    {
                                        deviceDetail = new DeviceSettings
                                        {
                                            DeviceId = Motordailog.DeviceId,
                                            PortName = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == Motordailog.DeviceId).FirstOrDefault().PortName,
                                            BaudRate = Motordailog.BaudRate,
                                            DataBit = Motordailog.Databit,
                                            StopBit = Motordailog.Stopbit,
                                            Parity = Motordailog.ParityValue
                                        },
                                        cameraDetail = new CameraSettings()
                                    },
                                    CommandText = "Motor Drive"
                                };
                                Commands.Add(command);
                            }
                            ShouldAdd = true;
                        }
                        break;
                    case (int)ElementConstant.Disconnect_Motor_Event:
                        getNewPosition(Disconnect_Motor_Event.Width, Disconnect_Motor_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        ShouldAdd = true;
                        break;
                    case (int)ElementConstant.Connect_Camera_Event:
                        getNewPosition(Connect_Camera_Event.Width, Connect_Camera_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        ShouldAdd = true;
                        break;
                    case (int)ElementConstant.Disconnect_Camera_Event:
                        getNewPosition(Disconnect_Camera_Event.Width, Disconnect_Camera_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        ShouldAdd = true;
                        break;
                    case (int)ElementConstant.Connect_Weight_Event:
                        getNewPosition(Connect_Weight_Event.Width, Connect_Weight_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        ConfigurationDailog dailog = new ConfigurationDailog("Set Weight Module Configuration", deviceInfo.CustomDeviceInfos);
                        dailog.ShowDialog();
                        if (!dailog.Canceled)
                        {
                            if (Commands.Count() == 0)
                            {
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration
                                    {
                                        deviceDetail = new DeviceSettings
                                        {
                                            DeviceId = dailog.DeviceId,
                                            PortName = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == dailog.DeviceId).FirstOrDefault().PortName,
                                            BaudRate = dailog.BaudRate,
                                            DataBit = dailog.Databit,
                                            StopBit = dailog.Stopbit,
                                            Parity = dailog.ParityValue
                                        },
                                        cameraDetail = new CameraSettings()
                                    },
                                    CommandText = "Connect Weight"
                                };
                                Commands.Add(command);
                            }
                            else
                            {
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration
                                    {
                                        deviceDetail = new DeviceSettings
                                        {
                                            DeviceId = dailog.DeviceId,
                                            PortName = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == dailog.DeviceId).FirstOrDefault().PortName,
                                            BaudRate = dailog.BaudRate,
                                            DataBit = dailog.Databit,
                                            StopBit = dailog.Stopbit,
                                            Parity = dailog.ParityValue
                                        },
                                        cameraDetail = new CameraSettings()
                                    },
                                    CommandText = "Connect Weight"
                                };
                                Commands.Add(command);
                            }
                            ShouldAdd = true;
                        }
                        break;
                    case (int)ElementConstant.Disconnect_Weight_Event:
                        getNewPosition(Disconnect_Weight_Event.Width, Disconnect_Weight_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        if (Commands.Count() == 0)
                        {
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = "Disconnect Weight"
                            };
                            Commands.Add(command);
                        }
                        else
                        {
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = "Connect Weight"
                            };
                            Commands.Add(command);
                        }
                        ShouldAdd = true;
                        break;
                    case (int)ElementConstant.Connect_ControlCard_Event:
                        getNewPosition(Connect_ControlCard_Event.Width, Connect_ControlCard_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        ConfigurationDailog controldailog = new ConfigurationDailog("Set Control Card Configuration", deviceInfo.CustomDeviceInfos);
                        controldailog.ShowDialog();
                        if (!controldailog.Canceled)
                        {
                            if (Commands.Count() == 0)
                            {
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration
                                    {
                                        deviceDetail = new DeviceSettings
                                        {
                                            DeviceId = controldailog.DeviceId,
                                            PortName = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == controldailog.DeviceId).FirstOrDefault().PortName,
                                            BaudRate = controldailog.BaudRate,
                                            DataBit = controldailog.Databit,
                                            StopBit = controldailog.Stopbit,
                                            Parity = controldailog.ParityValue
                                        },
                                        cameraDetail = new CameraSettings()
                                    },
                                    CommandText = "Connect Control Card"
                                };
                                Commands.Add(command);
                            }
                            else
                            {
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration
                                    {
                                        deviceDetail = new DeviceSettings
                                        {
                                            DeviceId = controldailog.DeviceId,
                                            PortName = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == controldailog.DeviceId).FirstOrDefault().PortName,
                                            BaudRate = controldailog.BaudRate,
                                            DataBit = controldailog.Databit,
                                            StopBit = controldailog.Stopbit,
                                            Parity = controldailog.ParityValue
                                        },
                                        cameraDetail = new CameraSettings()
                                    },
                                    CommandText = "Connect Control Card"
                                };
                                Commands.Add(command);
                            }
                            ShouldAdd = true;
                        }
                        break;
                    case (int)ElementConstant.Disconnect_ControlCard_Event:
                        getNewPosition(Disconnect_ControlCard_Event.Width, Disconnect_ControlCard_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle(contentId, Convert.ToInt32(data));
                        if (Commands.Count() == 0)
                        {
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = "Disconnect Control Card"
                            };
                            Commands.Add(command);
                        }
                        else
                        {
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = "Disconnect Control Card"
                            };
                            Commands.Add(command);
                        }
                        ShouldAdd = true;
                        break;
                    case (int)ElementConstant.Read_Motor_Frequency:
                        getNewPosition(Read_Motor_Frequency.Width, Read_Motor_Frequency.Height, ref NewLeft, ref NewTop);

                        NameVariableDialog variableDialog = new NameVariableDialog("Set Fucntion Name");
                        variableDialog.ShowDialog();
                        if (!variableDialog.Canceled)
                        {
                            if (Commands.Count() == 0)
                            {
                                ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = variableDialog.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            else
                            {
                                ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = variableDialog.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            ShouldAdd = true;
                        }
                        break;

                    case (int)ElementConstant.Change_Motor_Frequency:
                        getNewPosition(Change_Motor_Frequency.Width, Change_Motor_Frequency.Height, ref NewLeft, ref NewTop);

                        NameVariableDialog variableDialog1 = new NameVariableDialog("Set Fucntion Name");
                        variableDialog1.ShowDialog();
                        if (!variableDialog1.Canceled)
                        {
                            if (Commands.Count() == 0)
                            {
                                ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog1.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = variableDialog1.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            else
                            {
                                ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog1.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = variableDialog1.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            ShouldAdd = true;
                        }
                        break;
                    case (int)ElementConstant.Turn_ON_Motor:
                        getNewPosition(Turn_ON_Motor.Width, Turn_ON_Motor.Height, ref NewLeft, ref NewTop);
                        ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), "");
                        NameVariableDialog variableDialog2 = new NameVariableDialog("Set Fucntion Name");
                        variableDialog2.ShowDialog();

                        if (Commands.Count() == 0)
                        {

                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = variableDialog2.VariableName.Text
                            };
                            Commands.Add(command);
                        }
                        else
                        {
                            ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog2.VariableName.Text);
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = variableDialog2.VariableName.Text
                            };
                            Commands.Add(command);
                        }
                        ShouldAdd = true;

                        break;

                    case (int)ElementConstant.Turn_OFF_Motor:
                        getNewPosition(Turn_OFF_Motor.Width, Turn_OFF_Motor.Height, ref NewLeft, ref NewTop);
                        ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), "");

                        if (Commands.Count() == 0)
                        {

                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                               // CommandText = variableDialog3.VariableName.Text
                            };
                            Commands.Add(command);
                        }
                        else
                        {
                            //ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog3.VariableName.Text);
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                               // CommandText = variableDialog3.VariableName.Text
                            };
                            Commands.Add(command);
                        }
                        ShouldAdd = true;

                        break;

                    case (int)ElementConstant.Read_All_Card_In_Out:
                        getNewPosition(Read_All_Card_In_Out.Width, Read_All_Card_In_Out.Height, ref NewLeft, ref NewTop);

                        NameVariableDialog variableDialog4 = new NameVariableDialog("Set Fucntion Name");
                        variableDialog4.ShowDialog();
                        if (!variableDialog4.Canceled)
                        {
                            if (Commands.Count() == 0)
                            {
                                ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog4.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = variableDialog4.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            else
                            {
                                ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog4.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = variableDialog4.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            ShouldAdd = true;
                        }
                        break;

                    case (int)ElementConstant.Write_Card_Out:
                        getNewPosition(Write_Card_Out.Width, Write_Card_Out.Height, ref NewLeft, ref NewTop);

                        RegisterCommand registerCommand = new RegisterCommand();
                        if(!registerCommand.Canceled)
                        {
                            var functionName = registerCommand.RegisterOutput.ToString().ToLower() == "on" ? "ON" : "OFF" + " Register " + registerCommand.RegisterNumber;
                            ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), functionName);
                            if (Commands.Count() == 0)
                            {

                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandData=new RegisterWriteCommand
                                    {
                                        RegisterNumber= Convert.ToInt32(registerCommand.RegisterNumber),
                                        RegisterOutput= registerCommand.RegisterOutput.ToString().ToLower()=="on"?1:0
                                    }
                                    //CommandText = variableDialog5.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            else
                            {
                                //ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog5.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandData = new RegisterWriteCommand
                                    {
                                        RegisterNumber = Convert.ToInt32(registerCommand.RegisterNumber),
                                        RegisterOutput = registerCommand.RegisterOutput.ToString().ToLower() == "on" ? 1 : 0
                                    }
                                    //CommandText = variableDialog5.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            ShouldAdd = true;
                        }
                        
                        break;

                    case (int)ElementConstant.Read_Weight:
                        getNewPosition(Read_Weight.Width, Read_Weight.Height, ref NewLeft, ref NewTop);

                        NameVariableDialog variableDialog5 = new NameVariableDialog("Set Function Name");
                        variableDialog5.ShowDialog();
                        if (!variableDialog5.Canceled)
                        {
                            if (Commands.Count() == 0)
                            {
                                ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog5.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = variableDialog5.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            else
                            {
                                ele = Get_FunctionStyle(contentId, Convert.ToInt32(data), variableDialog5.VariableName.Text);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = variableDialog5.VariableName.Text
                                };
                                Commands.Add(command);
                            }
                            ShouldAdd = true;
                        }
                        break;


                    case (int)ElementConstant.If_Condition_Start:
                        getNewPosition(If_Condition_Start.Width, If_Condition_Start.Height, ref NewLeft, ref NewTop);
                        
                        if (Commands.Count() > 0)
                        {
                            ConditionsDialog conditions = new ConditionsDialog(Commands.Where(x => x.CommandType == (int)ElementConstant.Read_Weight).ToList());
                            conditions.ShowDialog();
                            if (!conditions.Canceled)
                            {
                                ConditionDataModel conditionData = new ConditionDataModel();
                                conditionData.ComparisonVariable = conditions.ComparisonVariable;
                                conditionData.ComparisonCondition = conditions.ComparisonCondition;
                                conditionData.ComparisonValue = conditions.ComparisonValue;
                                ele = Get_ControlStyle(contentId, Convert.ToInt32(data), conditionData, conditions.ConditionTextName);
                                var command = new LogicalCommand
                                {
                                    CommandId = contentId,
                                    CommandType = Convert.ToInt32(data),
                                    Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                    ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                    Configuration = new DeviceConfiguration(),
                                    CommandText = conditions.ConditionTextName,
                                    InputData = conditionData
                                };
                                Commands.Add(command);
                                ShouldAdd = true;
                            }
                        }
                        break;
                    case (int)ElementConstant.End_Scope:
                        getNewPosition(End_Scope.Width, End_Scope.Height, ref NewLeft, ref NewTop);
                        ele = Get_ControlStyle(contentId, Convert.ToInt32(data), new ConditionDataModel(), "");
                        if (Commands.Count() > 0)
                        {
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = "End Scope",
                                ParentCommandId = Commands.Where(x => x.CommandType == (int)ElementConstant.If_Condition_Start || x.CommandType == (int)ElementConstant.Else_If_Start || x.CommandType == (int)ElementConstant.Else_Start || x.CommandType == (int)ElementConstant.Repeat_Control).OrderBy(x => x.Order).ToList().LastOrDefault().CommandId
                            };
                            Commands.Add(command);
                            ShouldAdd = true;
                        }
                        break;
                    case (int)ElementConstant.Repeat_Control:
                        getNewPosition(Repeat_Control.Width, Repeat_Control.Height, ref NewLeft, ref NewTop);
                        ele = Get_ControlStyle(contentId, Convert.ToInt32(data), new ConditionDataModel(), "");
                        if (Commands.Count() > 0)
                        {
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = "Repeat Control"
                            };
                            Commands.Add(command);
                            ShouldAdd = true;
                        }
                        break;
                    case (int)ElementConstant.Stop_Repeat:
                        getNewPosition(Stop_Repeat.Width, Stop_Repeat.Height, ref NewLeft, ref NewTop);
                        ele = Get_ControlStyle(contentId, Convert.ToInt32(data), new ConditionDataModel(), "");
                        if (Commands.Count() > 0)
                        {
                            var command = new LogicalCommand
                            {
                                CommandId = contentId,
                                CommandType = Convert.ToInt32(data),
                                Order = Commands.OrderByDescending(x => x.Order).FirstOrDefault().Order + 1,
                                ExecutionStatus = (int)ExecutionStage.Not_Executed,
                                Configuration = new DeviceConfiguration(),
                                CommandText = "Repeat Control"
                            };
                            Commands.Add(command);
                            //ShouldAdd = true;
                        }
                        break;

                }

                if (ShouldAdd)
                {
                    Canvas.SetLeft(ele, NewLeft);
                    Canvas.SetTop(ele, NewTop);

                    try
                    {
                        if (ele != null) ReceiveDrop.Children.Add(ele);
                    }
                    catch { MessageBox.Show("Element can not be copied due to an error!"); }
                    isDragged = false;
                    e.Handled = true;
                }
                else
                {
                    isDragged = false;
                    e.Handled = true;
                }
                //checkElement();
            }
        }


        private void getNewPosition(double width, double height, ref double Newleft, ref double NewTop)
        {
            if (NewTop < 0)
            {
                NewTop = height;
            }
            else if (NewTop > (this.CanvasHeight - height))
            {
                NewTop = this.CanvasHeight - height;
            }

            if (Newleft < 0)
            {
                Newleft = width;
            }
            else if (Newleft > (this.CanvasWidth - width + 20))
            {
                Newleft = this.CanvasWidth - (width + 20);
            }
        }

        private int getElementCount()
        {
            return ReceiveDrop.Children.OfType<WrapPanel>().Count();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            var CloseBtn = sender as Button;
            var commandId = CloseBtn.Parent.GetValue(FrameworkElement.TagProperty);
            var command = Commands.Where(x => x.CommandId == commandId).FirstOrDefault();
            this.Commands.Remove(command);
            this.ReceiveDrop.Children.Remove(CloseBtn.Parent as WrapPanel);

        }
        private void CloseifStatement_Click(object sender, RoutedEventArgs e)
        {
            WrapPanel element = VisualTreeHelper.GetParent(sender as Button) as WrapPanel;
            //var commandId = CloseBtn.Parent.GetValue(FrameworkElement.TagProperty);
            //var command = Commands.Where(x => x.CommandId == commandId).FirstOrDefault();
            //this.Commands.Remove(command);
            //this.ReceiveDrop.Children.Remove(CloseBtn.Parent as WrapPanel);

        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            isDragged = false;
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            isDragged = false;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ReceiveDrop.Children.Clear();
        }

        private void ReceiveDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))

                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Ch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            dragObject = VisualTreeHelper.GetParent(sender as UIElement) as WrapPanel;
            Offset = e.GetPosition(VisualTreeHelper.GetParent(sender as UIElement) as WrapPanel);
            //this.Offset.Y = Canvas.GetTop(this.dragObject);
            //this.Offset.X = Canvas.GetLeft(this.dragObject);
            this.ReceiveDrop.CaptureMouse();
        }

        private void ReceiveDrop_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (dragObject == null)
                return;
            var position = e.GetPosition(ReceiveDrop);
            Canvas.SetTop(dragObject, position.Y - Offset.Y);
            Canvas.SetLeft(dragObject, position.X - Offset.X);
        }

        private void ReceiveDrop_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            dragObject = null;
            ReceiveDrop.ReleaseMouseCapture();
        }

        // Copy Element of Defined Type.
        private WrapPanel Get_Ten_Steps_Move(string ContentId, string content = "")
        {
            Button btn = new Button();
            btn.Margin = new Thickness(12, 5, 0, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.BorderBrush = (Brush)bc.ConvertFrom("#2a6a8e");
            btn.Background = (Brush)bc.ConvertFrom("#0082ca");
            btn.BorderThickness = new Thickness(2);
            btn.FontSize = 10;
            btn.Foreground = (Brush)bc.ConvertFrom("#fff");
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Georgia, serif;");
            btn.Content = string.IsNullOrEmpty(content) ? "10" : content;
            btn.Style = this.FindResource("BlueMove10") as Style;
            btn.Width = 121;
            btn.Height = 42;

            btn.Tag = (int)ElementConstant.Ten_Steps_Move;
            btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);
            var closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(-12, -4, 0, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Tag = ContentId;
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_Turn_Fiften_Degree_Right_Move(string ContentId, string content = "")
        {
            Button btn = new Button();
            btn.Margin = new Thickness(12, 5, 0, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.BorderBrush = (Brush)bc.ConvertFrom("#2a6a8e");
            btn.Background = (Brush)bc.ConvertFrom("#0082ca");
            btn.BorderThickness = new Thickness(2);
            btn.FontSize = 10;
            btn.Foreground = (Brush)bc.ConvertFrom("#fff");
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Georgia, serif;");
            btn.Content = string.IsNullOrEmpty(content) ? "15" : content;
            btn.Style = this.FindResource("BlueMoveRight") as Style;
            btn.Width = 150;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Turn_Fiften_Degree_Right_Move;
            btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);

            var closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(-12, -4, 0, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Tag = ContentId;
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_Turn_Fiften_Degree_Left_Move(string ContentId, string content = "")
        {
            Button btn = new Button();
            btn.Margin = new Thickness(12, 5, 0, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.BorderBrush = (Brush)bc.ConvertFrom("#2a6a8e");
            btn.Background = (Brush)bc.ConvertFrom("#0082ca");
            btn.BorderThickness = new Thickness(2);
            btn.FontSize = 10;
            btn.Foreground = (Brush)bc.ConvertFrom("#fff");
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Georgia, serif;");
            btn.Content = string.IsNullOrEmpty(content) ? "15" : content;
            btn.Style = this.FindResource("BlueMoveLeft") as Style;
            btn.Width = 150;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Turn_Fiften_Degree_Left_Move;

            btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);
            var closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(-12, -4, 0, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Tag = ContentId;
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_Pointer_State_Move(string ContentId)
        {
            Button btn = new Button();
            btn.Margin = new Thickness(12, 5, 0, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.BorderBrush = (Brush)bc.ConvertFrom("#2a6a8e");
            btn.Background = (Brush)bc.ConvertFrom("#0082ca");
            btn.BorderThickness = new Thickness(2);
            btn.FontSize = 10;
            btn.Foreground = (Brush)bc.ConvertFrom("#fff");
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Georgia, serif;");
            //btn.Content = "10";
            btn.Style = this.FindResource("BlueMovePointer") as Style;
            btn.Width = 200;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Pointer_State_Move;

            btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);
            var closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(-12, -4, 0, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;

            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Tag = ContentId;
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;

            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_Rotation_Style_Move(string ContentId)
        {
            Button btn = new Button();
            btn.Margin = new Thickness(12, 5, 0, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.BorderBrush = (Brush)bc.ConvertFrom("#2a6a8e");
            btn.Background = (Brush)bc.ConvertFrom("#0082ca");
            btn.BorderThickness = new Thickness(2);
            btn.FontSize = 10;
            btn.Foreground = (Brush)bc.ConvertFrom("#fff");
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Georgia, serif;");
            //btn.Content = "15";
            btn.Style = this.FindResource("BlueMoveRotation") as Style;
            btn.Width = 175;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Rotation_Style_Move;

            btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);
            var closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(-12, -4, 0, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();

            WrapPanel wrap = new WrapPanel();
            wrap.Tag = ContentId;
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_EventStyle(string ContentId, int evEnum, string variablename = "")
        {
            string content = string.Empty;
            switch (evEnum)
            {
                case (int)ElementConstant.Start_Event:
                    content = "Start";
                    break;
                case (int)ElementConstant.Connect_Motor_Event:
                    content = "Connect Motor";
                    break;
                case (int)ElementConstant.Disconnect_Motor_Event:
                    content = "Disconnect Motor";
                    break;
                case (int)ElementConstant.Connect_Weight_Event:
                    content = "Connect Weight";
                    break;
                case (int)ElementConstant.Disconnect_Weight_Event:
                    content = "Disconnect Weight";
                    break;
                case (int)ElementConstant.Connect_ControlCard_Event:
                    content = "Connect Control Card";
                    break;
                case (int)ElementConstant.Disconnect_ControlCard_Event:
                    content = "Disconnect Control Card";
                    break;
                case (int)ElementConstant.Read_Motor_Frequency:
                    content = "Read Motor Frequency";
                    break;
                case (int)ElementConstant.Connect_Camera_Event:
                    content = "Connect Camera";
                    break;
                case (int)ElementConstant.Disconnect_Camera_Event:
                    content = "Disconnect Camera";
                    break;
            }
            Button btn = new Button();
            btn.Margin = new Thickness(12, 5, 0, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.BorderBrush = (Brush)bc.ConvertFrom("#AA336A");
            btn.Background = (Brush)bc.ConvertFrom("#df88f9");
            btn.BorderThickness = new Thickness(0.5);
            btn.FontSize = 12;
            btn.Foreground = (Brush)bc.ConvertFrom("#fff");
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Georgia, serif;");
            if (!string.IsNullOrEmpty(variablename))
            {
                btn.Content = variablename;
            }
            else
            {
                btn.Content = content;
            }

            btn.Style = this.FindResource("EventButtonStyle") as Style;
            btn.Width = 200;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = evEnum;

            btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);
            var closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(-12, -4, 0, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Tag = ContentId;
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_FunctionStyle(string ContentId, int evEnum, string variablename = "")
        {
            string content = string.Empty;
            switch (evEnum)
            {
                case (int)ElementConstant.Read_Motor_Frequency:
                    content = "Read_Motor Frequency";
                    break;
                case (int)ElementConstant.Change_Motor_Frequency:
                    content = "Change Motor Frequency";
                    break;
                case (int)ElementConstant.Turn_ON_Motor:
                    content = "Turn ON Motor";
                    break;
                case (int)ElementConstant.Turn_OFF_Motor:
                    content = "Turn OFF Motor";
                    break;
                case (int)ElementConstant.Read_All_Card_In_Out:
                    content = "Read Card I/O";
                    break;
                case (int)ElementConstant.Write_Card_Out:
                    content = "Write Card Out";
                    break;
                case (int)ElementConstant.Read_Weight:
                    content = "Read Weight";
                    break;
            }
            Button btn = new Button();
            btn.Margin = new Thickness(12, 5, 0, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.BorderBrush = (Brush)bc.ConvertFrom("#ffc3c8");
            btn.Background = (Brush)bc.ConvertFrom("#f97279");
            btn.BorderThickness = new Thickness(0.5);
            btn.FontSize = 12;
            btn.Foreground = (Brush)bc.ConvertFrom("#fff");
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Georgia, serif;");
            if (!string.IsNullOrEmpty(variablename))
            {
                btn.Content = variablename;
            }
            else
            {
                btn.Content = content;
            }

            btn.Style = this.FindResource("FunctionButtonStyle") as Style;
            btn.Width = 200;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = evEnum;

            btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);
            var closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(-12, -4, 0, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Tag = ContentId;
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_ControlStyle(string ContentId, int evEnum, ConditionDataModel conditionData, string conditionName = "")
        {
            WrapPanel wrap = new WrapPanel();
            switch (evEnum)
            {
                case (int)ElementConstant.If_Condition_Start:
                    wrap = CreateIfStartElement(conditionName, conditionData);
                    wrap.Tag = ContentId;
                    wrap.Width = Double.NaN;
                    wrap.Height = Double.NaN;
                    break;
                case (int)ElementConstant.End_Scope:
                    wrap = CreateStandarControl(ContentId, evEnum,"End Scope");
                    break;
                case (int)ElementConstant.Repeat_Control:
                    wrap = CreateStandarControl(ContentId, evEnum, "Repeat Control");
                    break;
                case (int)ElementConstant.Stop_Repeat:
                    wrap = CreateStandarControl(ContentId, evEnum, "Stop Control");
                    break;
            }
            return wrap;
        }

        private WrapPanel CreateIfStartElement(string ConditionUniqueName, ConditionDataModel conditionData)
        {
            WrapPanel wrap = new WrapPanel();
            Button closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(0, -2, -2, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            wrap.Children.Add(closeBtn);
            Border parent = new Border();
            parent.Width = 340;
            parent.Height = Double.NaN;
            parent.HorizontalAlignment = HorizontalAlignment.Stretch;
            parent.VerticalAlignment = VerticalAlignment.Stretch;
            parent.BorderBrush = (Brush)bc.ConvertFrom("#ffdab2");
            parent.Background = (Brush)bc.ConvertFrom("#ffa94c");
            parent.BorderThickness = new Thickness(0.5);
            parent.CornerRadius = new CornerRadius(10, 1, 10, 1);
            parent.Padding = new Thickness(5);
            parent.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);
            parent.IsHitTestVisible = IsChildHitTestVisible;

            Grid parentGrid = new Grid();
            //RowDefinition gridRow1 = new RowDefinition();
            //gridRow1.Height = new GridLength(1, GridUnitType.Auto);
            RowDefinition gridRow2 = new RowDefinition();
            gridRow2.Height = new GridLength(1, GridUnitType.Star);
            //parentGrid.RowDefinitions.Add(gridRow1);
            parentGrid.RowDefinitions.Add(gridRow2);



            //parentGrid.Children.Add(closeBtn);
            //Grid.SetRow(closeBtn, 0);

            Grid childGrid = new Grid();
            childGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            childGrid.VerticalAlignment = VerticalAlignment.Stretch;
            RowDefinition childRow1 = new RowDefinition();
            childRow1.Height = new GridLength(1, GridUnitType.Auto);
            RowDefinition childRow2 = new RowDefinition();
            childRow2.Height = new GridLength(1, GridUnitType.Star);

            childGrid.RowDefinitions.Add(childRow1);
            childGrid.RowDefinitions.Add(childRow2);
            Grid.SetRow(childGrid, 0);

            Grid childFirst = new Grid();
            childFirst.HorizontalAlignment = HorizontalAlignment.Stretch;
            childFirst.VerticalAlignment = VerticalAlignment.Stretch;
            childFirst.Margin = new Thickness(8, 5, 8, 5);
            Grid.SetRow(childFirst, 0);

            Label Condition = new Label();
            Condition.Content = "IF Statement";
            Condition.HorizontalAlignment = HorizontalAlignment.Left;
            Condition.FontSize = 16;
            Condition.FontWeight = FontWeights.Bold;

            Label ConditionName = new Label();
            ConditionName.Content = ConditionUniqueName;
            ConditionName.HorizontalAlignment = HorizontalAlignment.Right;
            ConditionName.FontSize = 16;
            ConditionName.FontWeight = FontWeights.Bold;
            childFirst.Children.Add(Condition);
            childFirst.Children.Add(ConditionName);
            childGrid.Children.Add(childFirst);

            Grid childSecond = new Grid();
            childSecond.HorizontalAlignment = HorizontalAlignment.Stretch;
            childSecond.VerticalAlignment = VerticalAlignment.Stretch;
            Grid.SetRow(childSecond, 1);
            ColumnDefinition column1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition column2 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition column3 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            childSecond.ColumnDefinitions.Add(column1);
            childSecond.ColumnDefinitions.Add(column2);
            childSecond.ColumnDefinitions.Add(column3);

            Grid fColmn = new Grid();
            fColmn.HorizontalAlignment = HorizontalAlignment.Stretch;
            fColmn.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(fColmn, 0);
            fColmn.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(10),
                BorderBrush = (Brush)bc.ConvertFrom("#0082ca"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Background = Brushes.White,
                Child = new TextBlock
                {
                    Text = conditionData.ComparisonVariable,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = Brushes.Transparent,
                    Foreground = (Brush)bc.ConvertFrom("#0082ca"),
                    FontSize = 16
                }
            });
            childSecond.Children.Add(fColmn);

            Grid sColmn = new Grid();
            sColmn.HorizontalAlignment = HorizontalAlignment.Stretch;
            sColmn.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sColmn, 1);
            sColmn.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(10),
                BorderBrush = (Brush)bc.ConvertFrom("#0082ca"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Background = Brushes.White,
                Child = new TextBlock
                {
                    Text = ControlOp.GetConditions().Where(x => x.value == conditionData.ComparisonCondition).FirstOrDefault().Icon,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = Brushes.Transparent,
                    Foreground = (Brush)bc.ConvertFrom("#0082ca"),
                    FontSize = 16
                }
            });
            childSecond.Children.Add(sColmn);

            Grid tColmn = new Grid();
            tColmn.HorizontalAlignment = HorizontalAlignment.Stretch;
            tColmn.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(tColmn, 2);
            tColmn.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(10),
                BorderBrush = (Brush)bc.ConvertFrom("#0082ca"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Background = Brushes.White,
                Child = new TextBlock
                {
                    Text = conditionData.ComparisonValue,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = Brushes.Transparent,
                    Foreground = (Brush)bc.ConvertFrom("#0082ca"),
                    FontSize = 16
                }
            });
            childSecond.Children.Add(tColmn);
            childGrid.Children.Add(childSecond);
            parentGrid.Children.Add(childGrid);
            parent.Child = parentGrid;
            wrap.Children.Add(parent);

            return wrap;
        }

        private WrapPanel CreateStandarControl(string ContentId, int evEnum, string Content)
        {
            Button btn = new Button();
            btn.Margin = new Thickness(12, 5, 0, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.BorderBrush = (Brush)bc.ConvertFrom("#ffdab2");
            btn.Background = (Brush)bc.ConvertFrom("#ffa94c");
            btn.BorderThickness = new Thickness(0.5);
            btn.FontSize = 12;
            btn.Foreground = (Brush)bc.ConvertFrom("#fff");
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Georgia, serif;");
            btn.Content = Content;
            btn.Style = this.FindResource("ControlButtonStyle") as Style;
            btn.Width = 200;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = evEnum;

            btn.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Ch_PreviewMouseDown);
            var closeBtn = new Button();
            closeBtn.Foreground = new SolidColorBrush(Colors.White);
            closeBtn.Background = new SolidColorBrush(Colors.Red);
            closeBtn.Content = "X";
            closeBtn.FontSize = 10;
            closeBtn.VerticalAlignment = VerticalAlignment.Top;
            closeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            closeBtn.Margin = new Thickness(-12, -4, 0, 0);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Tag = ContentId;
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        #endregion

        #region Save and Re-Open Functions

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveInitiated();
        }

        public void SaveInitiated()
        {
            SaveBtn.IsEnabled = false;
            SaveBtn.Content = "Processing";
            if (getElementCount() > 0)
            {
                if (string.IsNullOrEmpty(_CurrentFile))
                {
                    if (!System.IO.Directory.Exists(_FileDirectory))
                    {
                        System.IO.Directory.CreateDirectory(_FileDirectory);
                    }

                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.InitialDirectory = _FileDirectory + @"\";
                    saveFileDialog1.Title = "Save Your File";
                    saveFileDialog1.CheckPathExists = true;
                    saveFileDialog1.DefaultExt = "json";
                    saveFileDialog1.Filter = "Jupiter Files (*.json)|*.json";
                    saveFileDialog1.FilterIndex = 1;
                    if (saveFileDialog1.ShowDialog() == true)
                    {
                        if (saveFileDialog1.FileName.ToString().Contains(" "))
                        {
                            MessageBox.Show("File name should not contain white space."); SaveInitiated(); return;
                        }

                        SaveFile(saveFileDialog1.FileName);
                    }
                }
                else
                {
                    SaveFile(_CurrentFile);
                }


            }
            else { MessageBox.Show("Could not initiate the process to save"); }
            SaveBtn.IsEnabled = true;
            SaveBtn.Content = "Save";
        }

        private void SaveFile(string _Filepath)
        {
            string filepath = _Filepath;

            FileSystemModel fileSystem = new FileSystemModel();
            fileSystem.FileId = System.IO.Path.GetFileName(filepath);

            var contentElement = ReceiveDrop.Children.OfType<WrapPanel>().ToList();
            List<FileContentModel> fileContent = new List<FileContentModel>();
            int Order = 1;
            foreach (var item in contentElement)
            {
                var child = item.Children.OfType<Button>().FirstOrDefault();
                Point margin = item.TransformToAncestor(ReceiveDrop)
                  .Transform(new Point(0, 0));
                fileContent.Add(new FileContentModel
                {
                    ContentId = Guid.NewGuid().ToString("N"),
                    ContentType = Convert.ToInt32(child.Tag),
                    ContentText = child.Content != null ? child.Content.ToString() : null,
                    ContentOrder = Order,
                    ContentLeftPosition = margin.X,
                    ContentTopPosition = margin.Y
                });
                Order++;
            }

            fileSystem.fileContents = fileContent;
            fileSystem.CreatedDate = DateTime.Now;

            var JsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(fileSystem);
            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }
            System.IO.File.WriteAllText(filepath, JsonContent);
            _CurrentFile = filepath;
            MessageBox.Show("Saved..");
        }

        private void LoadFile()
        {
            using (StreamReader r = new StreamReader(_CurrentFile))
            {
                string json = r.ReadToEnd();
                FileSystemModel items = JsonConvert.DeserializeObject<FileSystemModel>(json);

                foreach (var item in items.fileContents.OrderBy(x => x.ContentOrder))
                {
                    WrapPanel ele = new WrapPanel();
                    string contentId = Guid.NewGuid().ToString("N");
                    switch (Convert.ToInt32(item.ContentType))
                    {
                        case (int)ElementConstant.Ten_Steps_Move:
                            ele = Get_Ten_Steps_Move(contentId);
                            break;
                        case (int)ElementConstant.Turn_Fiften_Degree_Right_Move:
                            ele = Get_Turn_Fiften_Degree_Right_Move(contentId);
                            break;
                        case (int)ElementConstant.Turn_Fiften_Degree_Left_Move:
                            ele = Get_Turn_Fiften_Degree_Left_Move(contentId);
                            break;
                        case (int)ElementConstant.Pointer_State_Move:
                            ele = Get_Pointer_State_Move(contentId);
                            break;
                        case (int)ElementConstant.Rotation_Style_Move:
                            ele = Get_Rotation_Style_Move(contentId);
                            break;
                        case (int)ElementConstant.Start_Event:
                            ele = Get_EventStyle(contentId, Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Connect_Motor_Event:
                            ele = Get_EventStyle(contentId, Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Disconnect_Motor_Event:
                            ele = Get_EventStyle(contentId, Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Connect_Weight_Event:
                            ele = Get_EventStyle(contentId, Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Disconnect_Weight_Event:
                            ele = Get_EventStyle(contentId, Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Read_Motor_Frequency:
                            ele = Get_EventStyle(contentId, Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Change_Motor_Frequency:
                            ele = Get_EventStyle(contentId, Convert.ToInt32(item.ContentType));
                            break;
                    }

                    Canvas.SetLeft(ele, item.ContentLeftPosition);
                    Canvas.SetTop(ele, item.ContentTopPosition);
                    ReceiveDrop.Children.Add(ele);
                }

            }
        }

        private void Device_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ele = sender as StackPanel;
            if (ele.Name == "MotorDrive")
            {
                Border MotorParent = VisualTreeHelper.GetParent(ele) as Border;
                MotorParent.ClearValue(UIElement.OpacityProperty);
                MotorParent.Background = (Brush)bc.ConvertFrom("#4eaee5");
                MotorDriveArea.Visibility = Visibility.Visible;



                Border WeightParent = VisualTreeHelper.GetParent(WeightModule) as Border;
                WeightParent.Opacity = 0.8;
                WeightParent.Background = Brushes.White;
                WeightModuleArea.Visibility = Visibility.Hidden;

                Border CamParent = VisualTreeHelper.GetParent(NetCamera) as Border;
                CamParent.Opacity = 0.8;
                CamParent.Background = Brushes.White;
                NetCameraArea.Visibility = Visibility.Hidden;
                NetworkCameraOuputArea.Visibility = Visibility.Hidden;

                Border ControlParent = VisualTreeHelper.GetParent(ControlBoard) as Border;
                ControlParent.Opacity = 0.8;
                ControlParent.Background = Brushes.White;
                ControlBoardArea.Visibility = Visibility.Hidden;
            }
            else if (ele.Name == "WeightModule")
            {
                Border WeightParent = VisualTreeHelper.GetParent(ele) as Border;
                WeightParent.ClearValue(UIElement.OpacityProperty);
                WeightModuleArea.Visibility = Visibility.Visible;
                WeightParent.Background = (Brush)bc.ConvertFrom("#4eaee5");



                Border MotorParent = VisualTreeHelper.GetParent(MotorDrive) as Border;
                MotorParent.Opacity = 0.8;
                MotorParent.Background = Brushes.White;
                MotorDriveArea.Visibility = Visibility.Hidden;


                Border CamParent = VisualTreeHelper.GetParent(NetCamera) as Border;
                CamParent.Opacity = 0.8;
                CamParent.Background = Brushes.White;
                NetCameraArea.Visibility = Visibility.Hidden;
                NetworkCameraOuputArea.Visibility = Visibility.Hidden;

                Border ControlParent = VisualTreeHelper.GetParent(ControlBoard) as Border;
                ControlParent.Opacity = 0.8;
                ControlParent.Background = Brushes.White;
                ControlBoardArea.Visibility = Visibility.Hidden;
            }
            else if (ele.Name == "NetCamera")
            {
                Border CamParent = VisualTreeHelper.GetParent(ele) as Border;
                CamParent.ClearValue(UIElement.OpacityProperty);
                CamParent.Background = (Brush)bc.ConvertFrom("#4eaee5");
                NetCameraArea.Visibility = Visibility.Visible;
                NetworkCameraOuputArea.Visibility = Visibility.Visible;



                Border MotorParent = VisualTreeHelper.GetParent(MotorDrive) as Border;
                MotorParent.Opacity = 0.8;
                MotorParent.Background = Brushes.White;
                MotorDriveArea.Visibility = Visibility.Hidden;

                Border WeightParent = VisualTreeHelper.GetParent(WeightModule) as Border;
                WeightParent.Opacity = 0.8;
                WeightParent.Background = Brushes.White;
                WeightModuleArea.Visibility = Visibility.Hidden;

                Border ControlParent = VisualTreeHelper.GetParent(ControlBoard) as Border;
                ControlParent.Opacity = 0.8;
                ControlParent.Background = Brushes.White;
                ControlBoardArea.Visibility = Visibility.Hidden;
            }
            else if (ele.Name == "ControlBoard")
            {
                Border ControlParent = VisualTreeHelper.GetParent(ele) as Border;
                ControlParent.ClearValue(UIElement.OpacityProperty);
                ControlParent.Background = (Brush)bc.ConvertFrom("#4eaee5");
                ControlBoardArea.Visibility = Visibility.Visible;



                Border MotorParent = VisualTreeHelper.GetParent(MotorDrive) as Border;
                MotorParent.Opacity = 0.8;
                MotorParent.Background = Brushes.White;
                MotorDriveArea.Visibility = Visibility.Hidden;

                Border WeightParent = VisualTreeHelper.GetParent(WeightModule) as Border;
                WeightParent.Opacity = 0.8;
                WeightParent.Background = Brushes.White;
                WeightModuleArea.Visibility = Visibility.Hidden;

                Border CamParent = VisualTreeHelper.GetParent(NetCamera) as Border;
                CamParent.Opacity = 0.8;
                CamParent.Background = Brushes.White;
                NetCameraArea.Visibility = Visibility.Hidden;
                NetworkCameraOuputArea.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region USB Camera


        private void ConnectUSBCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DisconnectRunningCamera();
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
                    _videoSender = _webCamera.VideoChannel;
                    StreamUSBCamera.IsEnabled = true;

                    ConnectUSBCamera.IsEnabled = false;
                    ConnectUSBCamera.Content = "Connected";
                    DisconnectUSBCam.IsEnabled = true;
                    StartRecording.IsEnabled = true;
                    StartRecording.Content = "Start Recording";
                    _runningCamera = "USB";
                }
                else { MessageBox.Show("No USB Camera found."); }
            }
            catch (Exception ex)
            {
                StreamUSBCamera.IsEnabled = false;
                USBCam_error.Content = ex.ToString();
            }
        }

        private void DisconnectUSBCam_Click(object sender, RoutedEventArgs e)
        {
            videoViewer.Stop();
            videoViewer.Background = Brushes.Black;
            _webCamera.Stop();
            _webCamera.Dispose();
            _drawingImageProvider.Dispose();
            _connector.Disconnect(_webCamera.VideoChannel, _drawingImageProvider);
            _connector.Dispose();
            ConnectUSBCamera.IsEnabled = true;
            ConnectUSBCamera.Content = "Connect";
            DisconnectUSBCam.IsEnabled = false;

            UnstreamUSBCam.IsEnabled = false;
            StreamUSBCamera.IsEnabled = false;
            StartRecording.IsEnabled = false;
            _runningCamera = string.Empty;
            //StopVideoCapture();
        }

        private void StartCameraRecording(object sender, RoutedEventArgs e)
        {

            try
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    if (!string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        DisconnectUSBCam.IsEnabled = false;
                        StartRecording.IsEnabled = false;
                        StopRecording.IsEnabled = true;
                        StartVideoCapture(dialog.SelectedPath);
                    }

                }
            }
            catch
            {
                MessageBox.Show("An error has occured. Recording will be saved at default location : " + _VideoDirectory);
                StartVideoCapture(_VideoDirectory);
            }
        }

        private void StopCameraRecording(object sender, RoutedEventArgs e)
        {
            DisconnectUSBCam.IsEnabled = true;
            StartRecording.IsEnabled = true;
            StopRecording.IsEnabled = false;
            StopVideoCapture();

        }

        private void ConnectionUSB()
        {
            try
            {
                DisconnectRunningCamera();
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
                    if (!System.IO.Directory.Exists(_VideoDirectory))
                    {
                        System.IO.Directory.CreateDirectory(_VideoDirectory);
                    }

                    //_webCamera.VideoChannel

                    StreamUSBCamera.IsEnabled = true;

                    ConnectUSBCamera.IsEnabled = false;
                    ConnectUSBCamera.Content = "Connected";
                    DisconnectUSBCam.IsEnabled = true;
                    _runningCamera = "USB";
                    StartVideoCapture(_VideoDirectory);
                }
                else { MessageBox.Show("No USB Camera found."); }
            }
            catch (Exception ex)
            {
                StreamUSBCamera.IsEnabled = false;
                USBCam_error.Content = ex.ToString();
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
                    ConnectUSBCamera.IsEnabled = true;
                    ConnectUSBCamera.Content = "Connect";
                    DisconnectUSBCam.IsEnabled = false;

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
                MessageBox.Show("Failed to connect IP Camera..");
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

        #region Discover and Connect Device or Camera
        void GetIpCameras()
        {
            IPCameraFactory.DeviceDiscovered += IPCameraFactory_DeviceDiscovered;
            IPCameraFactory.DiscoverDevices();
        }

        private void IPCameraFactory_DeviceDiscovered(object sender, DiscoveryEventArgs e)
        {
            GuiThread(() => AddDevices(e.Device));
        }

        private void GuiThread(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        private void DiscoverDevice_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.Content = "Working..";
            btn.IsEnabled = false;
            StopDiscovery.IsEnabled = true;
            devices.Clear();
            DeviceModels.Clear();
            IPCameraFactory.DeviceDiscovered -= IPCameraFactory_DeviceDiscovered;
            GetIpCameras();
        }

        private void AddDevices(DiscoveredDeviceInfo discovered)
        {
            devices.Add(discovered);
            DeviceModels.Add(new DeviceModel { DeviceId = Guid.NewGuid().ToString("N"), Name = discovered.Name, DeviceIP = discovered.Host, DevicePort = discovered.Port, ConnectMessage = "Connect", DisconnectMessage = "Disconnect", Disconnected = true, Connected = false });
            DiscoveredDeviceList.ItemsSource = null;
            DiscoveredDeviceList.ItemsSource = DeviceModels;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            var deviceId = (sender as Button).Tag.ToString();
            foreach (var item in DeviceModels)
            {
                if (item.DeviceId == deviceId)
                {
                    item.Connected = true;
                    item.Disconnected = false;
                    var devicedetail = devices.Where(x => x.Host == item.DeviceIP && x.Port == item.DevicePort).FirstOrDefault();
                    ConnectIPCamera(devicedetail.Uri.AbsoluteUri.ToString(), "", "");
                }
                else
                {
                    item.Connected = false;
                    item.Disconnected = false;
                }
            }
            DiscoveredDeviceList.ItemsSource = null;
            DiscoveredDeviceList.ItemsSource = DeviceModels;
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in DeviceModels)
            {
                if (item.Connected)
                {
                    DisconnectIPCamera();
                }
                item.Connected = false;
                item.ConnectMessage = "Connect";
                item.Disconnected = true;
                item.DisconnectMessage = "Disconnect";
            }
            DiscoveredDeviceList.ItemsSource = null;
            DiscoveredDeviceList.ItemsSource = DeviceModels;
        }

        private void StopDiscovery_Click(object sender, RoutedEventArgs e)
        {
            IPCameraFactory.DeviceDiscovered -= IPCameraFactory_DeviceDiscovered;
            StopDiscovery.IsEnabled = false;
            DiscoverDevice.Content = "Discover";
            DiscoverDevice.IsEnabled = true;
        }

        #endregion

        #region Camera Streaming

        private void StreamUSBCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ip = ipAddressText.Text;
                var port = PortText.Text;

                OzConf_MJPEGStreamServer ozConf_ = new OzConf_MJPEGStreamServer(int.Parse(port), 25);
                ozConf_.Name = ipAddressText.Text.ToString();
                _streamer = new MJPEGStreamer(ozConf_);

                _connector.Connect(_videoSender, _streamer.VideoChannel);

                _streamer.ClientConnected += _streamer_ClientConnected;
                _streamer.ClientDisconnected += _streamer_ClientDisconnected;
                _streamer.Start();

                OpenInBrowserButton.IsEnabled = true;
                UnstreamUSBCam.IsEnabled = true;
                StreamUSBCamera.IsEnabled = false;
            }
            catch
            {
                OpenInBrowserButton.IsEnabled = false;
                UnstreamUSBCam.IsEnabled = false;
                StreamUSBCamera.IsEnabled = true;
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

        private void UnstreamUSBCam_Click(object sender, RoutedEventArgs e)
        {
            _streamer.Stop();
            _connector.Disconnect(_videoSender, _streamer.VideoChannel);
            OpenInBrowserButton.IsEnabled = false;
            UnstreamUSBCam.IsEnabled = false;
            StreamUSBCamera.IsEnabled = true;
        }

        private void OpenInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var ip = ipAddressText.Text;
            var port = PortText.Text;
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

        #region Sound Area

        private void LoadSystemSound()
        {
            var listSound = DeviceInformation.GetSystemSound();
            if (listSound != null && listSound.Count() > 0)
            {
                //Ist child.
                StackPanel wrapper = new StackPanel();

                //IInd child.
                Border outer = new Border
                {
                    Background = Brushes.White,
                    Width = 75,
                    Height = 75,
                    BorderThickness = new Thickness(0.6),
                    BorderBrush = (Brush)bc.ConvertFrom("#0082ca"),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(4),
                    VerticalAlignment = VerticalAlignment.Top
                };

                StackPanel Ori = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(2)
                };

                Border iconBorder = new Border
                {
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    BorderBrush = (Brush)bc.ConvertFrom("#eeeeee")
                };

                StackPanel iconParent = new StackPanel();
                iconParent.Children.Add(new FontAwesome.WPF.ImageAwesome { Icon = FontAwesome.WPF.FontAwesomeIcon.Flag, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = (Brush)bc.ConvertFrom("#95d0f1"), Width = 35, Height = 35 });
                iconBorder.Child = iconParent;

                Ori.Children.Add(iconBorder);
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var child1 = new Grid();
                child1.Children.Add(new Label
                {
                    Content = "Flag",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
                grid.Children.Add(child1);
                Grid.SetRow(child1, 0);
                Grid.SetColumn(child1, 0);

                var child2 = new Grid();
                var iconButton = new Button
                {
                    Width = 20,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2),
                    Padding = new Thickness(2),
                    Background = (Brush)bc.ConvertFrom("#fff"),
                    BorderBrush = (Brush)bc.ConvertFrom("#95d0f1"),
                    BorderThickness = new Thickness(1),
                    ToolTip = "Select"
                };

                StackPanel innerpanel = new StackPanel { Orientation = Orientation.Horizontal };
                innerpanel.Children.Add(new FontAwesome.WPF.ImageAwesome { Icon = FontAwesome.WPF.FontAwesomeIcon.Check, Foreground = (Brush)bc.ConvertFrom("#95d0f1"), Width = 15, Height = 15 });
                iconButton.Content = innerpanel;
                child2.Children.Add(iconButton);
                grid.Children.Add(child2);
                Grid.SetRow(child2, 0);
                Grid.SetColumn(child2, 1);

                Ori.Children.Add(grid);
                outer.Child = Ori;
                wrapper.Children.Add(outer);
                SoundArea.Children.Add(wrapper);

                //wrapper.Children.Add();

            }

        }


        #endregion

        #region Motor Area Test Function
        void ReadMotorResponse()
        {
            RecData _recData = new RecData();
            _recData = Common.ReceiveDataQueue.Dequeue();
            if (_recData.MbTgm.Length > 0 && _recData.MbTgm.Length > readIndex)
            {
                //To Read Function Code response.
                if (_recData.MbTgm[1] == (int)COM_Code.three)
                {
                    UInt32 ii = ByteArrayConvert.ToUInt16(_recData.MbTgm, 3);
                    float _i0 = ii / 100f;

                    motorspeed = ii / 100f;

                    if (isMotorRunning == null || isMotorRunning == false)
                    {
                        run_motor();
                    }
                    _dispathcer.Invoke(new Action(() =>
                    {
                        MotorSpeed.Content = "Current Speed : " + motorspeed + " Hz";
                    }));


                }

            }
        }
        private void TestMotor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ComPortMotor.SelectedValue.ToString()) && ComPortMotor.SelectedValue.ToString() != "0")
                {
                    TestMotor.IsEnabled = false;
                    StopMotor.IsEnabled = true;
                    Disable_RunTimeButton();
                    Connect_Motor();
                    //run_motor();


                    return;
                }

                MessageBox.Show("Please Select COM Port and configuration..");
            }
            catch (Exception ex)
            {
                TestMotor.IsEnabled = true;
                StopMotor.IsEnabled = false;
                Enable_RunTimeButton();
                StopPortCommunication();
            }

        }

        private void run_motor()
        {
            isMotorRunning = true;
            MotorTimer.Elapsed += MotorTimer_Elapsed;
            MotorTimer.Interval = 100 * 1;
            MotorTimer.Enabled = true;
            MotorTimer.Start();
        }

        private void MotorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            _dispathcer.Invoke(new Action(() =>
            {
                _currentAngle = _currentAngle + motorspeed;
                RotateTransform rotateTransform = new RotateTransform(_currentAngle);
                MotorContainer.RenderTransform = rotateTransform;

            }));
        }

        private void stop_motor()
        {
            _currentAngle = 0;
            MotorTimer.Enabled = false;
            MotorTimer.Stop();
            isMotorRunning = false;
            _dispathcer.Invoke(new Action(() =>
            {
                RotateTransform rotateTransform = new RotateTransform(0);
                MotorContainer.RenderTransform = rotateTransform;
                MotorSpeed.Content = "Current Speed : " + 0 + " Hz";
            }));
        }

        private void Connect_Motor()
        {
            try
            {
                string deviceId = ComPortMotor.SelectedValue.ToString();
                int Baudrate = Convert.ToInt32(BaudRateMotor.SelectionBoxItem.ToString());
                int databit = Convert.ToInt32(DataBitMotor.SelectionBoxItem.ToString());
                int stopbit = Convert.ToInt32(StopBitMotor.SelectionBoxItem.ToString());
                int parity = (int)Parity.None;
                switch (ParityMotor.SelectionBoxItem.ToString().ToLower())
                {
                    case "even":
                        parity = (int)Parity.Even;
                        break;
                    case "mark":
                        parity = (int)Parity.Mark;
                        break;
                    case "none":
                        parity = (int)Parity.None;
                        break;
                    case "odd":
                        parity = (int)Parity.Odd;
                        break;
                    case "space":
                        parity = (int)Parity.Space;
                        break;
                }



                var suctom = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == deviceId).FirstOrDefault();
                string Port = suctom.PortName;

                Common.RecState = 1;
                Common.CurrentDevice = Models.DeviceType.MotorDerive;


                RecData _recData = new RecData();
                _recData.deviceType = Models.DeviceType.MotorDerive;
                _recData.PropertyName = "MotorDrive";
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
                Common.RequestDataList.Add(_recData);
                TestPortCommunications(Port, Baudrate, databit, stopbit, parity);
                ReadFrequency.IsEnabled = true;
                WriteFrequency.IsEnabled = true;
                // TestRunMotor();
            }
            catch (Exception ex)
            {

            }
            //EnableControlInput();
        }

        private void StopMotor_Click(object sender, RoutedEventArgs e)
        {
            TestMotor.IsEnabled = true;
            StopMotor.IsEnabled = false;
            stop_motor();
            Enable_RunTimeButton();
            StopPortCommunication();
            ReadFrequency.IsEnabled = false;
            WriteFrequency.IsEnabled = false;
        }

        private void WriteFrequency_Click(object sender, RoutedEventArgs e)
        {
            NumberInputDialog inputDialog = new NumberInputDialog();
            inputDialog.ShowDialog();
            if (!inputDialog.Canceled && SerialDevice.IsOpen)
            {
                int frequency = Convert.ToInt32(inputDialog.VariableName.Text);
                WriteMotorState(8193, frequency * 100);
            }

        }

        private void ReadFrequency_Click(object sender, RoutedEventArgs e)
        {
            ReadMotorFrequency();
        }

        private void ReadMotorFrequency()
        {

            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            _CurrentActiveMenu = AppTools.Modbus;
            //SerialPortCommunications(Comport, 38400, 8, 1, 0);
            obj.GetMultiSendorValueFM3(2, (int)Parity.Even, SerialDevice, 8193, 10, "MotorDrive", 1, 0, Models.DeviceType.MotorDerive);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }
        private void WriteMotorState(int reg, int val)
        {

            write = true;
            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            byte[] _val1 = BitConverter.GetBytes(val);
            //byte[] _val = ConvertMisc.ConvertUInt32BcdToByteArray((UInt32)val);
            //int[] _val = val.ToString().Select(o => Convert.ToInt32(o) - 48).ToArray();
            int[] _val = new int[2] { _val1[1], _val1[0] };
            obj.SetMultiSendorValueFM16(2, 0, SerialDevice, reg + 1, 1, "MotorDrive", 1, 0, Models.DeviceType.MotorDerive, _val, false);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        #endregion

        #region Weight Module Test Function
        private void TestWeight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ComPortWeight.SelectedValue.ToString()) && ComPortWeight.SelectedValue.ToString() != "0")
                {
                    TestWeight.IsEnabled = false;
                    StopWeights.IsEnabled = true;
                    Disable_RunTimeButton();

                    string deviceId = ComPortWeight.SelectedValue.ToString();
                    int Baudrate = Convert.ToInt32(BaudRateWeight.SelectedValue.ToString());
                    int databit = Convert.ToInt32(DataBitWeight.SelectedValue.ToString());
                    int stopbit = Convert.ToInt32(StopBitWeight.SelectedValue.ToString());
                    int parity = 0;

                    switch (ParityWeight.SelectedValue.ToString().ToLower())
                    {
                        case "even":
                            parity = (int)Parity.Even;
                            break;
                        case "mark":
                            parity = (int)Parity.Mark;
                            break;
                        case "none":
                            parity = (int)Parity.None;
                            break;
                        case "odd":
                            parity = (int)Parity.Odd;
                            break;
                        case "space":
                            parity = (int)Parity.Space;
                            break;
                    }

                    var suctom = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == deviceId).FirstOrDefault();
                    string Port = suctom.PortName;

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
                    Common.RequestDataList.Add(_recData);
                    TestPortCommunications(Port, Baudrate, databit, stopbit, parity);
                    return;
                }

                MessageBox.Show("Please Select COM Port and configuration..");
            }
            catch
            {
                Enable_RunTimeButton();
            }
        }

        private void StopWeight_Click(object sender, RoutedEventArgs e)
        {
            TestWeight.IsEnabled = true;
            StopWeights.IsEnabled = false;
            Enable_RunTimeButton();
            WeightContent.Content = "8888888";
            WeightUnitKG.Content = "kg";
            StopPortCommunication();
        }

        void showWeightModuleResponse()
        {
            RecData _recData = new RecData();
            _recData = Common.ReceiveDataQueue.Dequeue();

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

        #endregion

        #region Control Card Test Function
        private void TestControlCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ComPortControl.SelectedValue.ToString()) && ComPortWeight.SelectedValue.ToString() != "0")
                {
                    TestControlCard.IsEnabled = false;
                    StopControlCard.IsEnabled = true;
                    Disable_RunTimeButton();

                    string deviceId = ComPortControl.SelectedValue.ToString();
                    int Baudrate = Convert.ToInt32(BaudRateControlCard.SelectedValue.ToString());
                    int databit = Convert.ToInt32(DataBitControlCard.SelectedValue.ToString());
                    int stopbit = Convert.ToInt32(StopBitControlCard.SelectedValue.ToString());
                    int parity = Convert.ToInt32(ParityControlCard.SelectedValue.ToString());

                    var suctom = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == deviceId).FirstOrDefault();
                    string Port = suctom.PortName;

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
                    Common.RequestDataList.Add(_recData);
                    TestPortCommunications(Port, Baudrate, databit, stopbit, parity);
                    TestReadAllInputOutput();
                    EnableControlInput();
                }
            }
            catch
            {
                TestControlCard.IsEnabled = true;
                StopControlCard.IsEnabled = false;
                Enable_RunTimeButton();
                DisableControlInput();
                StopPortCommunication();
            }
        }
        private void StopControlCard_Click(object sender, RoutedEventArgs e)
        {
            TestControlCard.IsEnabled = true;
            StopControlCard.IsEnabled = false;
            Enable_RunTimeButton();
            DisableControlInput();
            StopPortCommunication();
        }

        private void EnableControlInput()
        {
            Toggle16.IsEnabled = true;
            Toggle17.IsEnabled = true;
            Toggle18.IsEnabled = true;
            Toggle19.IsEnabled = true;
            Toggle20.IsEnabled = true;
            Toggle21.IsEnabled = true;
            Toggle22.IsEnabled = true;
            Toggle23.IsEnabled = true;
            Toggle24.IsEnabled = true;
            Toggle25.IsEnabled = true;
            Toggle26.IsEnabled = true;
            Toggle27.IsEnabled = true;
            Toggle28.IsEnabled = true;
            Toggle29.IsEnabled = true;
            Toggle30.IsEnabled = true;
            Toggle31.IsEnabled = true;
        }

        private void DisableControlInput()
        {
            Toggle16.IsEnabled = false;
            Toggle17.IsEnabled = false;
            Toggle18.IsEnabled = false;
            Toggle19.IsEnabled = false;
            Toggle20.IsEnabled = false;
            Toggle21.IsEnabled = false;
            Toggle22.IsEnabled = false;
            Toggle23.IsEnabled = false;
            Toggle24.IsEnabled = false;
            Toggle25.IsEnabled = false;
            Toggle26.IsEnabled = false;
            Toggle27.IsEnabled = false;
            Toggle28.IsEnabled = false;
            Toggle29.IsEnabled = false;
            Toggle30.IsEnabled = false;
            Toggle31.IsEnabled = false;
        }

        private void Toggle_Checked(object sender, RoutedEventArgs e)
        {
            var inp = sender as ToggleButton;

            if (inp.IsChecked != null && inp.IsChecked.Value)
            {
                if (Convert.ToInt32(inp.Content) < 31)
                {
                    WriteControCardState(Convert.ToInt32(inp.Content), 0);
                }
            }
            else if (inp.IsChecked != null && !inp.IsChecked.Value)
            {
                if (Convert.ToInt32(inp.Content) < 31)
                {
                    WriteControCardState(Convert.ToInt32(inp.Content), 1);
                }
            }


            // ReadAllControCardInputOutput();
        }

        private void TestReadAllInputOutput()
        {

            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            _CurrentActiveMenu = AppTools.Modbus;
            //SerialPortCommunications(Comport, 38400, 8, 1, 0);
            obj.GetMultiSendorValueFM3(1, 0, SerialDevice, 0, 30, "ControlCard", 1, 0, Models.DeviceType.ControlCard);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        void ReadControlCardResponse()
        {
            RecData _recData = new RecData();
            _recData = Common.ReceiveDataQueue.Dequeue();
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
            //SerialPortCommunications(Comport, 38400, 8, 1, 0);
            obj.GetMultiSendorValueFM3(1, 0, SerialDevice, reg, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        private void WriteControCardState(int reg, int val)
        {

            write = true;
            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            int[] _val = new int[2] { 0, val };
            obj.SetMultiSendorValueFM16(1, 0, SerialDevice, reg + 1, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard, _val, false);   

        }


        #endregion

        #region Enable Disable Runtime Function
        private void Enable_Run()
        {
            Run.IsEnabled = true;
        }

        private void Disable_Run()
        {
            Run.IsEnabled = false;
        }

        private void Enable_Reset()
        {
            Reset.IsEnabled = true;
        }

        private void Disable_Reset()
        {
            Reset.IsEnabled = false;
        }

        private void Enable_Save()
        {
            SaveBtn.IsEnabled = true;
        }
        private void Disable_Save()
        {
            SaveBtn.IsEnabled = false;
        }

        private void Enable_RunTimeButton()
        {
            Enable_Run();
            Enable_Reset();
            Enable_Save();
        }
        private void Disable_RunTimeButton()
        {
            Disable_Run();
            Disable_Reset();
            Disable_Save();
        }
        #endregion

        #region Port Communication Function Design Mode
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
            }

        }

        private void TestPortCommunications(string port = "", int baudRate = 0, int databit = 0, int stopBit = 0, int parity = 0)
        {
            if (!this.SerialDevice.IsOpen && this.SerialDevice != null)
            {
                try
                {
                    this.SerialDevice = new SerialPort(port);
                    this.SerialDevice.BaudRate = baudRate;
                    this.SerialDevice.DataBits = databit;
                    this.SerialDevice.StopBits = stopBit == 0 ? StopBits.None : (stopBit == 1 ? StopBits.One : (stopBit == 2 ? StopBits.Two : StopBits.OnePointFive));
                    switch (parity)
                    {
                        case 0:
                            this.SerialDevice.Parity = Parity.None;
                            break;
                        case 1:
                            this.SerialDevice.Parity = Parity.Odd;
                            break;
                        case 2:
                            this.SerialDevice.Parity = Parity.Even;
                            break;
                        case 3:
                            this.SerialDevice.Parity = Parity.Mark;
                            break;
                        case 4:
                            this.SerialDevice.Parity = Parity.Space;
                            break;
                        default:
                            this.SerialDevice.Parity = Parity.None;
                            break;

                    }

                    this.SerialDevice.Handshake = Handshake.None;
                    this.SerialDevice.Encoding = ASCIIEncoding.ASCII;
                    this.SerialDevice.DataReceived += TestDevice_DataReceived;
                    this.SerialDevice.Open();
                }
                catch (Exception ex)
                {

                }

            }
        }

        private void TestDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
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
                    TimerCheckReceiveData_Elapsed();
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
                    TimerCheckReceiveData_Elapsed();
                    //TimerCheckReceiveData.Enabled = true;
                    break;
            }
        }

        private void TimerCheckReceiveData_Elapsed()
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
                if (_RqRsDiff.TotalMilliseconds > Common.SessionTimeOut)  // Timeout
                {
                    RecData _recData = Common.RequestDataList.Where(a => a.SessionId == Common.GetSessionId).FirstOrDefault();
                    if (_recData != null)
                    {
                        //_SB1Request.EndSession(serialPort1); // End Session  
                        //UpdateRequestStatus(PortDataStatus.SessionEnd, Common.GetSessionId);
                        //Thread.Sleep(500);                                              
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

                            //Common.RecState = 0;
                            //Common.ReceiveDataQueue.Enqueue(_recData);
                        }

                        //if (_CurrentActiveMenu != AppTools.UART) Common.RequestDataList.Clear();

                        //Thread.Sleep(100);
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
                            //ToolTipStatus = ComStatus.OK;
                            //IsActive = false;
                            //RecData _recData = Common.RequestDataList.Where(a => a.SessionId == _reply.sesId).FirstOrDefault();
                            if (_recData != null && _reply.sesId == _recData.SessionId)
                            {
                                if (Common.COMSelected == COMType.UART)
                                {
                                    _recData.MbTgm = recBufParse;
                                    _recData.Status = PortDataStatus.Received;
                                    Common.GoodTmgm++;
                                    Common.ReceiveDataQueue.Enqueue(_recData);

                                    showWeightModuleResponse();
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
                                    //RecData _recData = Common.RequestDataList.Where(a => a.SessionId == _reply.sesId).FirstOrDefault();
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

                                        if (Common.CurrentDevice == Models.DeviceType.ControlCard)
                                        {
                                            ReadControlCardResponse(); // to reflect on the devices
                                        }
                                        else if (Common.CurrentDevice == Models.DeviceType.MotorDerive)
                                        {
                                            ReadMotorResponse();
                                        }



                                        return;
                                        //Common.IsClear = false;                                           
                                    }
                                }
                                else if (_reply.ack == 5)
                                {
                                    //UpdateRequestStatus(PortDataStatus.Ack, _reply.sesId);
                                    //IsAck = true;
                                    //ToolTipStatus = ComStatus.OK;
                                    if (_recData != null)
                                        _recData.Status = PortDataStatus.AckOkWait;
                                }
                                else
                                {
                                    //UpdateRequestStatus(PortDataStatus.Ack, _reply.sesId);
                                    //IsAck = true;
                                    //ToolTipStatus = ComStatus.OK;
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
                                //UpdateRequestStatus(PortDataStatus.SessionEnd, _reply.sesId);
                                //_SB1Request.EndSession(serialPort1);
                                //IsActive = false;
                                //ToolTipStatus = ComStatus.WrongSession;
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
                            // Check send/receive time diff.
                            //UpdateRequestStatus(PortDataStatus.Busy, _reply.sesId);
                            //IsActive = true;
                            //ToolTipStatus = ComStatus.Busy;
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Busy;

                                _SB1Request.EndSession(SerialDevice);

                            }
                        }
                        else if (_reply.ack == 2)  // IllegalCommand
                        {
                            _SB1Request.EndSession(SerialDevice);
                            //UpdateRequestStatus(PortDataStatus.SessionEnd, _reply.sesId);
                            //IsActive = false;
                            //ToolTipStatus = ComStatus.IllegalCommand;
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
                            //UpdateRequestStatus(PortDataStatus.SessionEnd, _reply.sesId);
                            //IsActive = false;
                            //Common.CRCFaults++;  //SB1 CRC 
                            //ToolTipStatus = ComStatus.CRC;
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Normal;
                                if (_recData.SessionId > 0)
                                {
                                    Common.ReceiveDataQueue.Enqueue(_recData);
                                    // Common.ReceiveDataQueueEventBased.Enqueue(_recData);
                                }
                            }
                        }
                        else if (_reply.ack == 4)  //Tgm Fault
                        {
                            _SB1Request.EndSession(SerialDevice);
                            //UpdateRequestStatus(PortDataStatus.SessionEnd, _reply.sesId);
                            //IsActive = false;
                            //Common.CRCFaults++;  //Tgm Fault
                            // ToolTipStatus = ComStatus.Tgmfault;
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Normal;
                                if (_recData.SessionId > 0)
                                {
                                    Common.ReceiveDataQueue.Enqueue(_recData);
                                    // Common.ReceiveDataQueueEventBased.Enqueue(_recData);
                                }
                            }
                        }
                    }
                    else
                    {
                        _UpdatePB = true;
                        _SB1Request.EndSession(SerialDevice);
                        //UpdateRequestStatus(PortDataStatus.SessionEnd, Common.GetSessionId);
                        //IsActive = false;
                        //ToolTipStatus = ComStatus.CRC;
                        if (_recData != null)
                        {
                            _recData.Status = PortDataStatus.Normal;
                            if (_recData.SessionId > 0)
                            {
                                Common.ReceiveDataQueue.Enqueue(_recData);
                                //ReadControlCardResponse();
                                // Common.ReceiveDataQueueEventBased.Enqueue(_recData);
                            }
                        }
                        // Common.CRCFaults++; // SB1 CRC

                    }
                }

            }

            TimerCheckReceiveData.Enabled = true;
        }

        #endregion

        #region Custom Operation
        public void ConnectedDevices()
        {
            if (deviceInfo != null && deviceInfo.CustomDeviceInfos != null && deviceInfo.CustomDeviceInfos.Count() > 0)
            {
                var devices = deviceInfo.CustomDeviceInfos;
                devices.Add(new CustomDeviceInfo { DeviceID = "0", PortName = "-Select-" });
                ComPortMotor.ItemsSource = devices;
                ComPortMotor.SelectedValuePath = "DeviceID";
                ComPortMotor.DisplayMemberPath = "PortName";
                ComPortMotor.SelectedValue = "0";

                ComPortWeight.ItemsSource = devices;
                ComPortWeight.SelectedValuePath = "DeviceID";
                ComPortWeight.DisplayMemberPath = "PortName";
                ComPortWeight.SelectedValue = "0";

                ComPortControl.ItemsSource = devices;
                ComPortControl.SelectedValuePath = "DeviceID";
                ComPortControl.DisplayMemberPath = "PortName";
                ComPortControl.SelectedValue = "0";
            }
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

                }

            }
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
                        //_SB1Request.EndSession(serialPort1); // End Session  
                        //UpdateRequestStatus(PortDataStatus.SessionEnd, Common.GetSessionId);
                        //Thread.Sleep(500);                                              
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

                            //Common.RecState = 0;
                            //Common.ReceiveDataQueue.Enqueue(_recData);
                        }

                        //if (_CurrentActiveMenu != AppTools.UART) Common.RequestDataList.Clear();

                        //Thread.Sleep(100);
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
                            //ToolTipStatus = ComStatus.OK;
                            //IsActive = false;
                            //RecData _recData = Common.RequestDataList.Where(a => a.SessionId == _reply.sesId).FirstOrDefault();
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
                                    //RecData _recData = Common.RequestDataList.Where(a => a.SessionId == _reply.sesId).FirstOrDefault();
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
                                        //Common.IsClear = false;                                           
                                    }
                                }
                                else if (_reply.ack == 5)
                                {
                                    //UpdateRequestStatus(PortDataStatus.Ack, _reply.sesId);
                                    //IsAck = true;
                                    //ToolTipStatus = ComStatus.OK;
                                    if (_recData != null)
                                        _recData.Status = PortDataStatus.AckOkWait;
                                }
                                else
                                {
                                    //UpdateRequestStatus(PortDataStatus.Ack, _reply.sesId);
                                    //IsAck = true;
                                    //ToolTipStatus = ComStatus.OK;
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
                                //UpdateRequestStatus(PortDataStatus.SessionEnd, _reply.sesId);
                                //_SB1Request.EndSession(serialPort1);
                                //IsActive = false;
                                //ToolTipStatus = ComStatus.WrongSession;
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
                            // Check send/receive time diff.
                            //UpdateRequestStatus(PortDataStatus.Busy, _reply.sesId);
                            //IsActive = true;
                            //ToolTipStatus = ComStatus.Busy;
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Busy;

                                _SB1Request.EndSession(SerialDevice);

                            }
                        }
                        else if (_reply.ack == 2)  // IllegalCommand
                        {
                            _SB1Request.EndSession(SerialDevice);
                            //UpdateRequestStatus(PortDataStatus.SessionEnd, _reply.sesId);
                            //IsActive = false;
                            //ToolTipStatus = ComStatus.IllegalCommand;
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
                            //UpdateRequestStatus(PortDataStatus.SessionEnd, _reply.sesId);
                            //IsActive = false;
                            //Common.CRCFaults++;  //SB1 CRC 
                            //ToolTipStatus = ComStatus.CRC;
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Normal;
                                if (_recData.SessionId > 0)
                                {
                                    Common.ReceiveDataQueue.Enqueue(_recData);
                                    // Common.ReceiveDataQueueEventBased.Enqueue(_recData);
                                }
                            }
                        }
                        else if (_reply.ack == 4)  //Tgm Fault
                        {
                            _SB1Request.EndSession(SerialDevice);
                            //UpdateRequestStatus(PortDataStatus.SessionEnd, _reply.sesId);
                            //IsActive = false;
                            //Common.CRCFaults++;  //Tgm Fault
                            // ToolTipStatus = ComStatus.Tgmfault;
                            if (_recData != null)
                            {
                                _recData.Status = PortDataStatus.Normal;
                                if (_recData.SessionId > 0)
                                {
                                    Common.ReceiveDataQueue.Enqueue(_recData);
                                    // Common.ReceiveDataQueueEventBased.Enqueue(_recData);
                                }
                            }
                        }
                    }
                    else
                    {
                        _UpdatePB = true;
                        _SB1Request.EndSession(SerialDevice);
                        //UpdateRequestStatus(PortDataStatus.SessionEnd, Common.GetSessionId);
                        //IsActive = false;
                        //ToolTipStatus = ComStatus.CRC;
                        if (_recData != null)
                        {
                            _recData.Status = PortDataStatus.Normal;
                            if (_recData.SessionId > 0)
                            {
                                Common.ReceiveDataQueue.Enqueue(_recData);
                                //ReadControlCardResponse();
                                // Common.ReceiveDataQueueEventBased.Enqueue(_recData);
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

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.parentWindow.Hide();
                HMIDialoge dialoge = new HMIDialoge(Commands, deviceInfo);
                dialoge.ShowDialog();
                if (dialoge.IsClosed)
                {
                    this.parentWindow.Show();
                    MessageBox.Show("Execution Stopped");
                }
            }
            catch (Exception ex)
            {
                Run.Content = "";
                Run.Content = "Run";
                Run.IsEnabled = true;
            }
        }

        private void Connect_control_card(string Port, int Baudrate, int databit, int stopbit, int parity)
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

        #endregion
    }
}




