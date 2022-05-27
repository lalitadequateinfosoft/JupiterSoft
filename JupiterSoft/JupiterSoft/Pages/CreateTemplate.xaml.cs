using JupiterSoft.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ozeki.Media;
using Ozeki.Camera;
using Ozeki;
using System.IO;
using Newtonsoft.Json;
using System.IO.Ports;
using Components;
using Util;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls.Primitives;

namespace JupiterSoft.Pages
{
    /// <summary>
    /// Interaction logic for CreateTemplate.xaml
    /// </summary>
    public partial class CreateTemplate : Page
    {

        private bool write = false;
        const int REC_BUF_SIZE = 10000;
        byte[] recBuf = new byte[REC_BUF_SIZE];
        byte[] recBufParse = new byte[REC_BUF_SIZE];
        List<byte> recBuflist = new List<byte>();

        Byte[] _MbTgmBytes;
        //bool IsActive = false;
        //bool IsAck = false;
        internal UInt32 TotalReceiveSize = 0;
        bool IsComplete = false;
        //int recIdx;
        //int recState = 1;
        bool IsLinkBoxTry = false;


        System.Timers.Timer TimerCheckReceiveData = new System.Timers.Timer();
        internal bool _UpdatePB = true;
        internal bool UseThisPage = true;
        static readonly object _object = new object();
        string _CurrentActiveMenu = "Modbus";

        //Custom Property Declaration.
        public static readonly DependencyProperty IsChildHitTestVisibleProperty =
            DependencyProperty.Register("IsChildHitTestVisible", typeof(bool), typeof(CreateTemplate),
                new PropertyMetadata(true));

        public bool IsChildHitTestVisible
        {
            get { return (bool)GetValue(IsChildHitTestVisibleProperty); }
            set { SetValue(IsChildHitTestVisibleProperty, value); }
        }

        //Custom Properties Declaration End.

        Point Offset;
        WrapPanel dragObject;
        bool isDragged = false;
        bool isloaded = false;
        private string _FileDirectory = ApplicationConstant._FileDirectory;

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
        private byte[] buffer;
        private Dispatcher _dispathcer;
        private BackgroundWorker worker;
        private int readIndex = 0;
        private bool initialized = false;
        int toggled = 0;
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


            //TimerCheckReceiveData.Elapsed += TimerCheckReceiveData_Elapsed;
            //TimerCheckReceiveData.Interval = 1000 * 1;
            //TimerCheckReceiveData.Enabled = true;
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
            //Saved project.
            _CurrentFile = _filename;
            this.ProjectName.Content = System.IO.Path.GetFileName(_filename).Split('.')[0];
            LoadFile();

            deviceInfo = DeviceInformation.GetConnectedDevices();
            ConnectedDevices();
            LoadSystemSound();
        }

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
            var data = e.Data.GetData(typeof(string));
            if (data != null)
            {
                Point dropPosition = e.GetPosition(ReceiveDrop);
                double NewTop = dropPosition.Y;
                double NewLeft = dropPosition.X;
                WrapPanel ele = new WrapPanel();
                switch (Convert.ToInt32(data))
                {
                    case (int)ElementConstant.Ten_Steps_Move:
                        getNewPosition(Ten_Steps_Move.Width, Ten_Steps_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Ten_Steps_Move();
                        break;
                    case (int)ElementConstant.Turn_Fiften_Degree_Right_Move:
                        getNewPosition(Turn_Fiften_Degree_Right_Move.Width, Turn_Fiften_Degree_Right_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Turn_Fiften_Degree_Right_Move();
                        break;
                    case (int)ElementConstant.Turn_Fiften_Degree_Left_Move:
                        getNewPosition(Turn_Fiften_Degree_Left_Move.Width, Turn_Fiften_Degree_Left_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Turn_Fiften_Degree_Left_Move();
                        break;
                    case (int)ElementConstant.Pointer_State_Move:
                        getNewPosition(Pointer_State_Move.Width, Pointer_State_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Pointer_State_Move();
                        break;
                    case (int)ElementConstant.Rotation_Style_Move:
                        getNewPosition(Rotation_Style_Move.Width, Rotation_Style_Move.Height, ref NewLeft, ref NewTop);
                        ele = Get_Rotation_Style_Move();
                        break;
                    case (int)ElementConstant.Start_Event:
                        getNewPosition(Start_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;
                    case (int)ElementConstant.Clicked_Event:
                        getNewPosition(Clicked_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;
                    case (int)ElementConstant.Connect_Event:
                        getNewPosition(Connect_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;
                    case (int)ElementConstant.Disconnect_Event:
                        getNewPosition(Disconnect_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;
                    case (int)ElementConstant.Space_Key_pressed_Event:
                        getNewPosition(Space_Key_pressed_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;
                    case (int)ElementConstant.Send_Message_Event:
                        getNewPosition(Send_Message_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;
                    case (int)ElementConstant.Receive_Message_Event:
                        getNewPosition(Receive_Message_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;
                    case (int)ElementConstant.BroadCast_Message_Event:
                        getNewPosition(BroadCast_Message_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;
                    case (int)ElementConstant.BroadCast_Message_Wait_Event:
                        getNewPosition(BroadCast_Message_Wait_Event.Width, Clicked_Event.Height, ref NewLeft, ref NewTop);
                        ele = Get_EventStyle_Move(Convert.ToInt32(data));
                        break;

                }

                Canvas.SetLeft(ele, NewLeft);
                Canvas.SetTop(ele, NewTop);

                try
                {
                    if (ele != null) ReceiveDrop.Children.Add(ele);
                }
                catch { MessageBox.Show("Element can not be copied due to an error!"); }
                isDragged = false;
                e.Handled = true;
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
            this.ReceiveDrop.Children.Remove(CloseBtn.Parent as WrapPanel);

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
        private WrapPanel Get_Ten_Steps_Move(string content = "")
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
            //closeBtn.Margin = new Thickness(-5, 0, 0, 0);
            //closeBtn.Padding = new Thickness(1);
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            //wrap.Background = new SolidColorBrush(Colors.Blue);
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }


        private WrapPanel Get_Turn_Fiften_Degree_Right_Move(string content = "")
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
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            //closeBtn.Margin = new Thickness(-5, 0, 0, 0);
            //closeBtn.Padding = new Thickness(1);
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            //wrap.Background = new SolidColorBrush(Colors.Blue);
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_Turn_Fiften_Degree_Left_Move(string content = "")
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
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            //closeBtn.Margin = new Thickness(-5, 0, 0, 0);
            //closeBtn.Padding = new Thickness(1);
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            //wrap.Background = new SolidColorBrush(Colors.Blue);
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_Pointer_State_Move()
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
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            //closeBtn.Margin = new Thickness(-5, 0, 0, 0);
            //closeBtn.Padding = new Thickness(1);
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            //wrap.Background = new SolidColorBrush(Colors.Blue);
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_Rotation_Style_Move()
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
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            //closeBtn.Margin = new Thickness(-5, 0, 0, 0);
            //closeBtn.Padding = new Thickness(1);
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

        private WrapPanel Get_EventStyle_Move(int evEnum)
        {
            string content = string.Empty;
            switch (evEnum)
            {
                case (int)ElementConstant.Start_Event:
                    content = "Start";
                    break;
                case (int)ElementConstant.Clicked_Event:
                    content = "Click";
                    break;
                case (int)ElementConstant.Connect_Event:
                    content = "Connect";
                    break;
                case (int)ElementConstant.Disconnect_Event:
                    content = "Disconnect";
                    break;
                case (int)ElementConstant.Space_Key_pressed_Event:
                    content = "Space key Pressed";
                    break;
                case (int)ElementConstant.Send_Message_Event:
                    content = "Send Message";
                    break;
                case (int)ElementConstant.Receive_Message_Event:
                    content = "Receive Message";
                    break;
                case (int)ElementConstant.BroadCast_Message_Event:
                    content = "Broadcast Message";
                    break;
                case (int)ElementConstant.BroadCast_Message_Wait_Event:
                    content = "Broadcast Message Wait";
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
            btn.Content = content;
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
            closeBtn.Style = this.FindResource("Closebtn") as Style;
            closeBtn.Click += CloseBtn_Click;
            Random random = new Random();
            WrapPanel wrap = new WrapPanel();
            wrap.Width = Double.NaN;
            wrap.Height = Double.NaN;
            wrap.Children.Add(btn);
            wrap.Children.Add(closeBtn);

            return wrap;
        }

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
            _runningCamera = string.Empty;

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

        private void LoadFile()
        {
            using (StreamReader r = new StreamReader(_CurrentFile))
            {
                string json = r.ReadToEnd();
                FileSystemModel items = JsonConvert.DeserializeObject<FileSystemModel>(json);

                foreach (var item in items.fileContents.OrderBy(x => x.ContentOrder))
                {
                    WrapPanel ele = new WrapPanel();
                    switch (Convert.ToInt32(item.ContentType))
                    {
                        case (int)ElementConstant.Ten_Steps_Move:
                            ele = Get_Ten_Steps_Move();
                            break;
                        case (int)ElementConstant.Turn_Fiften_Degree_Right_Move:
                            ele = Get_Turn_Fiften_Degree_Right_Move();
                            break;
                        case (int)ElementConstant.Turn_Fiften_Degree_Left_Move:
                            ele = Get_Turn_Fiften_Degree_Left_Move();
                            break;
                        case (int)ElementConstant.Pointer_State_Move:
                            ele = Get_Pointer_State_Move();
                            break;
                        case (int)ElementConstant.Rotation_Style_Move:
                            ele = Get_Rotation_Style_Move();
                            break;
                        case (int)ElementConstant.Start_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Clicked_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Connect_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Disconnect_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Space_Key_pressed_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Send_Message_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.Receive_Message_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.BroadCast_Message_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                        case (int)ElementConstant.BroadCast_Message_Wait_Event:
                            ele = Get_EventStyle_Move(Convert.ToInt32(item.ContentType));
                            break;
                    }

                    Canvas.SetLeft(ele, item.ContentLeftPosition);
                    Canvas.SetTop(ele, item.ContentTopPosition);
                    ReceiveDrop.Children.Add(ele);
                }

            }
        }


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

        #endregion

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

        private void Run_Click(object sender, RoutedEventArgs e)
        {

            //try
            //{
            //    //Load all command.
            //    var contentElement = ReceiveDrop.Children.OfType<WrapPanel>().ToList();
            //    List<userCommands> commands = new List<userCommands>();
            //    int Order = 1;
            //    foreach (var item in contentElement)
            //    {
            //        var child = item.Children.OfType<Button>().FirstOrDefault();
            //        commands.Add(new userCommands
            //        {
            //            ContentId = Guid.NewGuid().ToString("N"),
            //            ContentType = Convert.ToInt32(child.Tag),
            //            ContentText = child.Content != null ? child.Content.ToString() : null,
            //            ContentOrder = Order
            //        });
            //        Order++;
            //    }

            //    List<SelectedDevices> _devices = new List<SelectedDevices>();
            //    _devices = getConfiguredDevices();


            //    if (commands != null || commands.Count() == 0)
            //    {
            //        MessageBox.Show("No logical commands found to execute..");
            //        return;
            //    }

            //    if (_devices != null && _devices.Count() == 0)
            //    {
            //        MessageBox.Show("No devices has been configured..");
            //        return;
            //    }
            //    userCommandLogic.sDevices = _devices;
            //    userCommandLogic.uCommands = commands;

            //    FontAwesome.WPF.ImageAwesome spinner = new FontAwesome.WPF.ImageAwesome { Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner, Spin = true, Foreground = (Brush)bc.ConvertFrom("#fff") };
            //    Run.Content = "";
            //    Run.Content = spinner;
            //    Run.IsEnabled = false;
            //}
            //catch
            //{
            //    Run.Content = "";
            //    Run.Content = "Run";
            //    Run.IsEnabled = true;
            //}

            if (!string.IsNullOrEmpty(ComPortWeight.SelectedValue.ToString()) && ComPortWeight.SelectedValue.ToString() != "0")
            {
                ReadWeight(ComPortWeight.SelectedValue.ToString());
                Stop.Visibility = Visibility.Visible;
                Run.Visibility = Visibility.Hidden;
                return;
            }

            if (!string.IsNullOrEmpty(ComPortControl.SelectedValue.ToString()) && ComPortControl.SelectedValue.ToString() != "0")
            {
                var suctom = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == ComPortControl.SelectedValue.ToString()).FirstOrDefault();
                string Port = suctom.PortName;
                ReadAllControCardInputOutput(Port);
                Stop.Visibility = Visibility.Visible;
                Run.Visibility = Visibility.Hidden;
                return;
            }

            if (!string.IsNullOrEmpty(ComPortMotor.SelectedValue.ToString()) && ComPortMotor.SelectedValue.ToString() != "0")
            {
                //ReadWeight(ComPortWeight.SelectedValue.ToString());
                Stop.Visibility = Visibility.Visible;
                Run.Visibility = Visibility.Hidden;
                return;
            }



        }



        public void ExecuteUserCommands()
        {
            if (userCommandLogic == null) return;

            foreach (var _command in userCommandLogic.uCommands.OrderBy(x => x.ContentOrder))
            {
                //var suctom = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == selectedval).FirstOrDefault();
                //Port = suctom.PortName;
                //SerialPortCommunications(Port, Baudrate, databit, stopbit, parity);
            }
        }

        public List<SelectedDevices> getConfiguredDevices()
        {
            List<SelectedDevices> _devices = new List<SelectedDevices>();
            int Baudrate = 38400;
            int databit = 8;
            int stopbit = 1;

            if (ComPortMotor.SelectedValue.ToString() != "0")
            {
                _devices.Add(new SelectedDevices
                {
                    deviceId = ComPortMotor.SelectedValue.ToString(),
                    TypeOfDevice = (int)userDeviceType.MotorDerive,
                    Baudrate = Baudrate,
                    databit = databit,
                    stopbit = stopbit,
                    parity = (int)Parity.None
                });
            }

            if (ComPortWeight.SelectedValue.ToString() != "0")
            {
                _devices.Add(new SelectedDevices
                {
                    deviceId = ComPortWeight.SelectedValue.ToString(),
                    TypeOfDevice = (int)userDeviceType.WeightModule,
                    Baudrate = Baudrate,
                    databit = databit,
                    stopbit = stopbit,
                    parity = (int)Parity.None
                });
            }

            if (ComPortControl.SelectedValue.ToString() != "0")
            {
                _devices.Add(new SelectedDevices
                {
                    deviceId = ComPortControl.SelectedValue.ToString(),
                    TypeOfDevice = (int)userDeviceType.ControlCard,
                    Baudrate = Baudrate,
                    databit = databit,
                    stopbit = stopbit,
                    parity = (int)Parity.None
                });
            }

            return _devices;
        }

        private void SerialPortCommunications(string port = "", int baudRate = 0, int databit = 0, int stopBit = 0, int parity = 0)
        {
            if (!this.SerialDevice.IsOpen)
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
                    if (_reply.CheckCrc(recBufParse, Convert.ToInt32(_reply.length)) || _CurrentActiveMenu == AppTools.UART || write==true || write==false)  // SB1 Check CRC
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
                                    if (_IsTgmErr || write==true)
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
                                        ReadControlCardResponse();
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
                        //Common.RecIdx += serialPort1.Read(recBuf, Common.RecIdx, serialPort1.BytesToRead);                                                      

                        recBuf = new byte[this.SerialDevice.BytesToRead];
                        this.SerialDevice.Read(recBuf, 0, recBuf.Length);
                        //while (SerialDevice.BytesToRead > 0)
                        //{
                        //    byte[] rec = new byte[1];
                        //    Common.RecIdx += SerialDevice.Read(rec, 0, 1);
                        //    recBuf[i] = rec[0];
                        //    i++;
                        //}

                        IsComplete = true;
                        Common.ReceiveBufferQueue.Enqueue(recBuf);


                    }
                    else if (Common.COMSelected == COMType.MODBUS)
                    {
                        Common.RecIdx = 0;
                        Common.RecState = 2;
                        //Common.RecIdx += serialPort1.Read(recBuf, Common.RecIdx, serialPort1.BytesToRead);                                                      

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
                    //recIdx += serialPort1.Read(recBuf, recIdx, serialPort1.BytesToRead);
                    //Common.RecIdx += serialPort1.Read(recBuf, Common.RecIdx, serialPort1.BytesToRead);    

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

        private void ReadWeight(string deviceId)
        {
            Common.RecState = 1;
            Common.CurrentDevice = Models.DeviceType.WeightModule;

            //int TypeOfDevice = (int)userDeviceType.WeightModule;
            int Baudrate = 9600;  //38400 for control card.
            int databit = 8;
            int stopbit = 1;
            int parity = (int)Parity.None;

            var suctom = deviceInfo.CustomDeviceInfos.Where(x => x.DeviceID == deviceId).FirstOrDefault();
            string Port = suctom.PortName;
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
            SerialPortCommunications(Port, Baudrate, databit, stopbit, parity);


        }

        private void StopWeight()
        {
            this.SerialDevice.DtrEnable = false;
            this.SerialDevice.RtsEnable = false;
            this.SerialDevice.DiscardInBuffer();
            this.SerialDevice.DiscardOutBuffer();
            //this.SerialDevice.DataReceived -= SerialDevice_DataReceived;
            //this.SerialDevice.Dispose();
            //this.SerialDevice.Close();

            Stop.Visibility = Visibility.Hidden;
            Run.Visibility = Visibility.Visible;
            WeightContent.Content = "8888888";
            WeightUnitKG.Content = "kg";
            //TimerCheckReceiveData.Elapsed -= TimerCheckReceiveData_Elapsed;
            //TimerCheckReceiveData.Enabled = false;
        }

        private void ReadAllControCardInputOutput(string Comport)
        {
            //BindData.Enabled = true;
            //CancelCount = 0;              

            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            _CurrentActiveMenu = AppTools.Modbus;
            SerialPortCommunications(Comport, 38400, 8, 1, 0);
            obj.GetMultiSendorValueFM3(1, 0, SerialDevice, 0, 30, "ControlCard", 1, 0, Models.DeviceType.ControlCard);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        private void ReadControCardState(int reg, string Comport)
        {
            //BindData.Enabled = true;
            //CancelCount = 0;              

            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            _CurrentActiveMenu = AppTools.Modbus;
            SerialPortCommunications(Comport, 38400, 8, 1, 0);
            obj.GetMultiSendorValueFM3(1, 0, SerialDevice, reg, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        private void WriteControCardState(int reg, int val)
        {
            //BindData.Enabled = true;
            //CancelCount = 0;              
            write = true;
            MODBUSComnn obj = new MODBUSComnn();
            Common.COMSelected = COMType.MODBUS;
            int[] _val = new int[2] { 0, val };
            //SerialPortCommunications("COM7", 38400, 8, 1, 0);
            obj.SetMultiSendorValueFM16(1, 0, SerialDevice, reg+1, 1, "ControlCard", 1, 0, Models.DeviceType.ControlCard, _val, false);   // GetSoftwareVersion(Common.Address, Common.Parity, sp, _ValueType);

        }

        #region UI Interactive Function
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


        #endregion

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopWeight();
        }

        private void Toggle_Checked(object sender, RoutedEventArgs e)
        {
            var inp = sender as ToggleButton;
            toggled = Convert.ToInt32(inp.Content.ToString());
            if(inp.IsChecked!=null && inp.IsChecked.Value)
            {
                if (Convert.ToInt32(inp.Content) < 31)
                {
                    WriteControCardState(Convert.ToInt32(inp.Content), 0);
                }
            }
            else if(inp.IsChecked != null && !inp.IsChecked.Value)
            {
                if (Convert.ToInt32(inp.Content) < 31)
                {
                    WriteControCardState(Convert.ToInt32(inp.Content), 1);
                }
            }
            

            // ReadAllControCardInputOutput();
        }

       
    }
}




