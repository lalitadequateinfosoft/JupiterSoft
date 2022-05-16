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

namespace JupiterSoft.Pages
{
    /// <summary>
    /// Interaction logic for CreateTemplate.xaml
    /// </summary>
    public partial class CreateTemplate : Page
    {
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
        public CreateTemplate()
        {
            bc = new BrushConverter();
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
        }

        public CreateTemplate(string _filename)
        {
            bc = new BrushConverter();
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
                if (Convert.ToInt32(data) == (int)ElementConstant.Ten_Steps_Move)
                {
                    //Set Drop Position.
                    getNewPosition(Ten_Steps_Move.Width, Ten_Steps_Move.Height, ref NewLeft, ref NewTop);

                    ele = Get_Ten_Steps_Move();
                    Canvas.SetLeft(ele, NewLeft);
                    Canvas.SetTop(ele, NewTop);
                }
                else if (Convert.ToInt32(data) == (int)ElementConstant.Turn_Fiften_Degree_Right_Move)
                {
                    //Set Drop Position.
                    getNewPosition(Turn_Fiften_Degree_Right_Move.Width, Turn_Fiften_Degree_Right_Move.Height, ref NewLeft, ref NewTop);
                    ele = Get_Turn_Fiften_Degree_Right_Move();
                    Canvas.SetLeft(ele, NewLeft);
                    Canvas.SetTop(ele, NewTop);
                }
                else if (Convert.ToInt32(data) == (int)ElementConstant.Turn_Fiften_Degree_Left_Move)
                {
                    getNewPosition(Turn_Fiften_Degree_Left_Move.Width, Turn_Fiften_Degree_Left_Move.Height, ref NewLeft, ref NewTop);
                    ele = Get_Turn_Fiften_Degree_Left_Move();
                    Canvas.SetLeft(ele, NewLeft);
                    Canvas.SetTop(ele, NewTop);
                }
                else if (Convert.ToInt32(data) == (int)ElementConstant.Pointer_State_Move)
                {
                    getNewPosition(Pointer_State_Move.Width, Pointer_State_Move.Height, ref NewLeft, ref NewTop);
                    ele = Get_Pointer_State_Move();
                    Canvas.SetLeft(ele, NewLeft);
                    Canvas.SetTop(ele, NewTop);
                }
                else if (Convert.ToInt32(data) == (int)ElementConstant.Rotation_Style_Move)
                {
                    getNewPosition(Rotation_Style_Move.Width, Rotation_Style_Move.Height, ref NewLeft, ref NewTop);
                    ele = Get_Rotation_Style_Move();
                    Canvas.SetLeft(ele, NewLeft);
                    Canvas.SetTop(ele, NewTop);
                }

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
                    ContentText = child.Content!=null?child.Content.ToString():null,
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
            DeviceModels.Add(new DeviceModel { DeviceId = Guid.NewGuid().ToString("N"), Name = discovered.Name, DeviceIP = discovered.Host, DevicePort = discovered.Port,ConnectMessage="Connect",DisconnectMessage="Disconnect",Disconnected=true,Connected=false });
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
                    if(Convert.ToInt32(item.ContentType) == (int)ElementConstant.Ten_Steps_Move)
                    {
                        ele = Get_Ten_Steps_Move(item.ContentText);
                        Canvas.SetLeft(ele, item.ContentLeftPosition);
                        Canvas.SetTop(ele, item.ContentTopPosition);
                        ReceiveDrop.Children.Add(ele);
                    }
                    else if (Convert.ToInt32(item.ContentType) == (int)ElementConstant.Turn_Fiften_Degree_Right_Move)
                    {
                        ele = Get_Turn_Fiften_Degree_Right_Move(item.ContentText);
                        Canvas.SetLeft(ele, item.ContentLeftPosition);
                        Canvas.SetTop(ele, item.ContentTopPosition);
                        ReceiveDrop.Children.Add(ele);
                    }
                    else if (Convert.ToInt32(item.ContentType) == (int)ElementConstant.Turn_Fiften_Degree_Left_Move)
                    {
                        ele = Get_Turn_Fiften_Degree_Left_Move(item.ContentText);
                        Canvas.SetLeft(ele, item.ContentLeftPosition);
                        Canvas.SetTop(ele, item.ContentTopPosition);
                        ReceiveDrop.Children.Add(ele);
                    }
                    else if (Convert.ToInt32(item.ContentType) == (int)ElementConstant.Pointer_State_Move)
                    {
                        ele = Get_Pointer_State_Move();
                        Canvas.SetLeft(ele, item.ContentLeftPosition);
                        Canvas.SetTop(ele, item.ContentTopPosition);
                        ReceiveDrop.Children.Add(ele);
                    }
                    else if (Convert.ToInt32(item.ContentType) == (int)ElementConstant.Rotation_Style_Move)
                    {
                        ele = Get_Rotation_Style_Move();
                        Canvas.SetLeft(ele, item.ContentLeftPosition);
                        Canvas.SetTop(ele, item.ContentTopPosition);
                        ReceiveDrop.Children.Add(ele);
                    }
                }

            }
        }


        #region Custom Operation
        public void ConnectedDevices()
        {
            if(deviceInfo!=null && deviceInfo.CustomDeviceInfos!=null && deviceInfo.CustomDeviceInfos.Count()>0)
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

        public SerialPort ConnectPort(string comPort)
        {
            SerialPort SerialDevice = new SerialPort(comPort);
            SerialDevice.BaudRate = 2400;
            SerialDevice.Parity = Parity.None;
            SerialDevice.StopBits = StopBits.One;
            SerialDevice.DataBits = 7;
            SerialDevice.Handshake = Handshake.None;
            SerialDevice.Encoding = ASCIIEncoding.ASCII;
            SerialDevice.DataReceived += OnDataReceived;

            SerialDevice.Open();
            return SerialDevice;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void StreamUSBCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ip = ipAddressText.Text;
                var port = PortText.Text;

                OzConf_MJPEGStreamServer ozConf_ = new OzConf_MJPEGStreamServer(int.Parse(port),25);
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
            if(listSound!=null && listSound.Count()>0)
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
                    BorderThickness=new Thickness(0,0,0,1),
                    BorderBrush=(Brush)bc.ConvertFrom("#eeeeee")
                };
              
                StackPanel iconParent = new StackPanel();
                iconParent.Children.Add(new FontAwesome.WPF.ImageAwesome{Icon=FontAwesome.WPF.FontAwesomeIcon.Flag, VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center,Foreground=(Brush)bc.ConvertFrom("#95d0f1"),Width=35,Height=35 });
                iconBorder.Child = iconParent;

                Ori.Children.Add(iconBorder);
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1,GridUnitType.Star) });

                var child1 = new Grid();
                child1.Children.Add(new Label {
                Content="Flag",
                HorizontalAlignment=HorizontalAlignment.Center,
                VerticalAlignment=VerticalAlignment.Center
                });
                grid.Children.Add(child1);
                Grid.SetRow(child1, 0);
                Grid.SetColumn(child1, 0);

                var child2 = new Grid();
                var iconButton = new Button
                {
                    Width=20,
                    Height=20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin=new Thickness(2),
                    Padding=new Thickness(2),
                    Background=(Brush)bc.ConvertFrom("#fff"),
                    BorderBrush= (Brush)bc.ConvertFrom("#95d0f1"),
                    BorderThickness=new Thickness(1),
                    ToolTip="Select"
                };

                StackPanel innerpanel = new StackPanel {Orientation=Orientation.Horizontal };
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


    }
}
