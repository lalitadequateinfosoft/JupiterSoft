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
        public CreateTemplate()
        {
            bc = new BrushConverter();
            this.UElement = ElementOp.GetElementModel();
            InitializeComponent();
            this.DataContext = this.UElement;
            this.CanvasWidth = ReceiveDrop.Width;
            this.CanvasHeight = ReceiveDrop.Height;
            this.isloaded = true;
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
        private WrapPanel Get_Ten_Steps_Move()
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
            btn.Content = "10";
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


        private WrapPanel Get_Turn_Fiften_Degree_Right_Move()
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
            btn.Content = "15";
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

        private WrapPanel Get_Turn_Fiften_Degree_Left_Move()
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
            btn.Content = "15";
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

                    string filepath = saveFileDialog1.FileName.Contains(".json") ? saveFileDialog1.FileName : saveFileDialog1.FileName + ".json";

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
                            ContentText = child.Content.ToString(),
                            ContentOrder = Order,
                            ContentLeftPosition = margin.X,
                            ContentRightPosition = margin.Y
                        });
                        Order++;
                    }

                    fileSystem.fileContents = fileContent;
                    fileSystem.CreatedDate = DateTime.Now;

                    var JsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(fileSystem);
                    System.IO.File.WriteAllText(filepath, JsonContent);
                    MessageBox.Show("Saved..");

                }

            }
            else { MessageBox.Show("Could not initiate the process to save"); }
            SaveBtn.IsEnabled = true;
            SaveBtn.Content = "Save";
        }

        private void Device_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ele = sender as StackPanel;
            if (ele.Name == "MotorDrive")
            {
                Border MotorParent = VisualTreeHelper.GetParent(ele) as Border;
                MotorParent.ClearValue(UIElement.OpacityProperty);
                MotorDriveArea.Visibility = Visibility.Visible;

                Border WeightParent = VisualTreeHelper.GetParent(WeightModule) as Border;
                WeightParent.Opacity = 0.8;
                WeightModuleArea.Visibility = Visibility.Hidden;

                Border CamParent = VisualTreeHelper.GetParent(NetCamera) as Border;
                CamParent.Opacity = 0.8;
                NetCameraArea.Visibility = Visibility.Hidden;
            }
            else if (ele.Name == "WeightModule")
            {
                Border WeightParent = VisualTreeHelper.GetParent(ele) as Border;
                WeightParent.ClearValue(UIElement.OpacityProperty);
                WeightModuleArea.Visibility = Visibility.Visible;

                Border MotorParent = VisualTreeHelper.GetParent(MotorDrive) as Border;
                MotorParent.Opacity = 0.8;
                MotorDriveArea.Visibility = Visibility.Hidden;

                Border CamParent = VisualTreeHelper.GetParent(NetCamera) as Border;
                CamParent.Opacity = 0.8;
                NetCameraArea.Visibility = Visibility.Hidden;
                NetworkCameraOuputArea.Visibility = Visibility.Hidden;
            }
            else if (ele.Name == "NetCamera")
            {
                Border CamParent = VisualTreeHelper.GetParent(ele) as Border;
                CamParent.ClearValue(UIElement.OpacityProperty);
                NetCameraArea.Visibility = Visibility.Visible;
                NetworkCameraOuputArea.Visibility = Visibility.Visible;

                Border MotorParent = VisualTreeHelper.GetParent(MotorDrive) as Border;
                MotorParent.Opacity = 0.8;
                MotorDriveArea.Visibility = Visibility.Hidden;

                Border WeightParent = VisualTreeHelper.GetParent(WeightModule) as Border;
                WeightParent.Opacity = 0.8;
                WeightModuleArea.Visibility = Visibility.Hidden;
            }
        }


        private void CameraDetail_TextChanged(object sender, TextChangedEventArgs e)
        {
            validateCameraDetails();
        }

        private void validateCameraDetails()
        {
            if (!isloaded) return;
            bool IsCameraIPValid = false;
            bool IsPortValid = false;
            bool IsUserNameValid = false;
            bool IsPassword = false;

            if (!string.IsNullOrEmpty(CameraIPText.Text) && CameraIPText.Text != "Enter Camera IP" && ApplicationConstant.CheckIPValid(CameraIPText.Text) != null)
            {
                IsCameraIPValid = true;
            }

            if (!string.IsNullOrEmpty(PortNumber.Text) && PortNumber.Text != "Enter Port Number" && ApplicationConstant.IsNumeric(PortNumber.Text))
            {
                IsPortValid = true;
            }
            if (!string.IsNullOrEmpty(Username.Text) && Username.Text != "Enter Username") IsUserNameValid = true;
            if (!string.IsNullOrEmpty(Password.Text) && Password.Text != "Enter Password") IsPassword = true;

            if (IsCameraIPValid && IsPortValid && IsUserNameValid && IsPassword)
            {
                ConnectCam.IsEnabled = true;
                Networkexception.Content = "";
            }
            else
            {
                ConnectCam.IsEnabled = false;
                if (!IsCameraIPValid) { Networkexception.Content = "Invalid/Empty Camera IP"; return; }
                if (!IsPortValid) { Networkexception.Content = "Invalid/Empty Port Number"; return; }
                if (!IsUserNameValid) { Networkexception.Content = "Invalid/Empty Userame"; return; }
                if (!IsPassword) { Networkexception.Content = "Invalid/Empty Password"; return; }
            }
        }




        private void ConnectCam_Click(object sender, RoutedEventArgs e)
        {
            ConnectCam.IsEnabled = false;
            try
            {
                DisconnectRunningCamera();
                _drawingImageProvider = new DrawingImageProvider();
                _connector = new MediaConnector();
                videoViewer.SetImageProvider(_drawingImageProvider);

                string Camip = CameraIPText.Text + ":" + PortNumber.Text;
                _camera = new IPCamera(Camip, Username.Text, Password.Text);
                _connector.Connect(_camera.VideoChannel, _drawingImageProvider);
                _camera.Start();
                videoViewer.Start();
                ConnectCam.IsEnabled = false;
                ConnectCam.Content = "Connected";
                DisconnectCam.IsEnabled = true;
            }
            catch (Exception ex) { ConnectCam.IsEnabled = true; Networkexception.Content = ex.ToString(); }

        }

        private void RemoveText(object sender, RoutedEventArgs e)
        {
            var ele = sender as TextBox;
            if (ele.Name == "CameraIPText")
            {
                if (ele.Text == "Enter Camera IP")
                {
                    ele.Text = "";
                }
            }
            else if (ele.Name == "PortNumber")
            {
                if (ele.Text == "Enter Port Number") ele.Text = "";
            }
            else if (ele.Name == "Username")
            {
                if (ele.Text == "Enter Username") ele.Text = "";
            }
            else if (ele.Name == "Password")
            {
                if (ele.Text == "Enter Password") ele.Text = "";
            }

            if (ele.Foreground == Brushes.Gray)
            {
                ele.Foreground = Brushes.Black;
            }
        }

        private void AddText(object sender, RoutedEventArgs e)
        {
            var ele = sender as TextBox;
            if (ele.Name == "CameraIPText")
            {
                if (string.IsNullOrWhiteSpace(ele.Text)) { ele.Text = "Enter Camera IP"; ele.Foreground = Brushes.Gray; }
            }
            else if (ele.Name == "PortNumber")
            {
                if (string.IsNullOrWhiteSpace(ele.Text)) { ele.Text = "Enter Port Number"; ele.Foreground = Brushes.Gray; }
            }
            else if (ele.Name == "Username")
            {
                if (string.IsNullOrWhiteSpace(ele.Text)) { ele.Text = "Enter Username"; ele.Foreground = Brushes.Gray; }
            }
            else if (ele.Name == "Password")
            {
                if (string.IsNullOrWhiteSpace(ele.Text)) { ele.Text = "Enter Password"; ele.Foreground = Brushes.Gray; }
            }


        }

        private void DisconnectCam_Click(object sender, RoutedEventArgs e)
        {
            _camera.Stop();
            _camera.Disconnect();
            _camera.Dispose();
            _drawingImageProvider.Dispose();
            _connector.Disconnect(_camera.VideoChannel, _drawingImageProvider);
            _connector.Dispose();
            videoViewer.Stop();
            videoViewer.Background = Brushes.Black;
            ConnectCam.Content = "Connect";
            DisconnectCam.IsEnabled = false;
            validateCameraDetails();
        }

        private void ConnectUSBCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DisconnectRunningCamera();
                _webCamera = new WebCamera();
                _drawingImageProvider = new DrawingImageProvider();
                _connector = new MediaConnector();

                _connector.Connect(_webCamera.VideoChannel, _drawingImageProvider);
                videoViewer.SetImageProvider(_drawingImageProvider);
                _webCamera.Start();
                videoViewer.Start();
                ConnectUSBCamera.IsEnabled = false;
                ConnectUSBCamera.Content = "Connected";
                DisconnectUSBCam.IsEnabled = true;
            }
            catch (Exception ex)
            {
                USBCam_error.Content = ex.ToString();
            }
        }

        private void DisconnectUSBCam_Click(object sender, RoutedEventArgs e)
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

        private void RTSP_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RTSP_GotFocus(object sender, RoutedEventArgs e)
        {
            var ele = sender as TextBox;
            if (ele.Name == "RTSPUrl")
            {
                if (ele.Text == "Enter Camera RTSP URL")
                {
                    ele.Text = "";
                }
            }
            else if (ele.Name == "RTSPUsername")
            {
                if (ele.Text == "Enter Username") ele.Text = "";
            }
            else if (ele.Name == "RTSPPassword")
            {
                if (ele.Text == "Enter Password") ele.Text = "";
            }

            if (ele.Foreground == Brushes.Gray)
            {
                ele.Foreground = Brushes.Black;
            }
        }

        private void RTSP_LostFocus(object sender, RoutedEventArgs e)
        {
            var ele = sender as TextBox;
            if (ele.Name == "RTSPUrl")
            {
                if (string.IsNullOrWhiteSpace(ele.Text)) { ele.Text = "Enter Camera RTSP URL"; ele.Foreground = Brushes.Gray; }
            }
            else if (ele.Name == "RTSPUsername")
            {
                if (string.IsNullOrWhiteSpace(ele.Text)) { ele.Text = "Enter Username"; ele.Foreground = Brushes.Gray; }
            }
            else if (ele.Name == "RTSPPassword")
            {
                if (string.IsNullOrWhiteSpace(ele.Text)) { ele.Text = "Enter Password"; ele.Foreground = Brushes.Gray; }
            }
        }

        private void DisconnectRunningCamera()
        {
            if (!string.IsNullOrEmpty(_runningCamera))
            {
                if (_runningCamera == "ONVIF")
                {
                    _camera.Stop();
                    _camera.Disconnect();
                    _camera.Dispose();
                    _drawingImageProvider.Dispose();
                    _connector.Disconnect(_camera.VideoChannel, _drawingImageProvider);
                    _connector.Dispose();
                    videoViewer.Stop();
                    videoViewer.Background = Brushes.Black;
                    ConnectCam.Content = "Connect";
                    DisconnectCam.IsEnabled = false;
                    validateCameraDetails();
                }
                else if (_runningCamera == "USB")
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
                else if (_runningCamera=="RTSP")
                {

                }
            }
        }
    }
}
