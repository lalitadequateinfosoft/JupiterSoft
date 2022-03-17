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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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


        //Custom Variables
        UIElement dragObject = null;
        Point Offset;
        BrushConverter bc;
        ElementModel UElement;

        public CreateTemplate()
        {
            bc = new BrushConverter();
            this.UElement = ElementOp.GetElementModel();
            InitializeComponent();
            this.DataContext = this.UElement;
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
            //UIElement copy = XamlReader.Parse(XamlWriter.Save(sender)) as UIElement;
            //UIElement copy = new UIElement();
            // copy = XamlReader.Parse(XamlWriter.S2ave(sender)) as UIElement;
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

                }
                //IsChildHitTestVisible = false;
                //DragDrop.DoDragDrop(copy, new DataObject(DataFormats.Serializable, copy), DragDropEffects.Copy);
                //IsChildHitTestVisible = true;

            }

            e.Handled = true;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(string));
            if (data != null)
            {
                if (Convert.ToInt32(data) == (int)ElementConstant.Ten_Steps_Move)
                {
                    var btn = Get_Ten_Steps_Move();
                    Point dropPosition = e.GetPosition(ReceiveDrop);
                    Canvas.SetLeft(btn, dropPosition.X);
                    Canvas.SetTop(btn, dropPosition.Y);
                    try
                    {
                        ReceiveDrop.Children.Add(btn);
                    }
                    catch
                    {

                    }
                }
                else if (Convert.ToInt32(data) == (int)ElementConstant.Turn_Fiften_Degree_Right_Move)
                {
                    var btn = Get_Turn_Fiften_Degree_Right_Move();
                    Point dropPosition = e.GetPosition(ReceiveDrop);
                    Canvas.SetLeft(btn, dropPosition.X);
                    Canvas.SetTop(btn, dropPosition.Y);

                    try
                    {
                        ReceiveDrop.Children.Add(btn);
                    }
                    catch
                    {

                    }
                }
                else if (Convert.ToInt32(data) == (int)ElementConstant.Turn_Fiften_Degree_Left_Move)
                {
                    var btn = Get_Turn_Fiften_Degree_Left_Move();
                    Point dropPosition = e.GetPosition(ReceiveDrop);
                    Canvas.SetLeft(btn, dropPosition.X);
                    Canvas.SetTop(btn, dropPosition.Y);

                    try
                    {
                        ReceiveDrop.Children.Add(btn);
                    }
                    catch
                    {

                    }
                }
                else if (Convert.ToInt32(data) == (int)ElementConstant.Pointer_State_Move)
                {
                    var btn = Get_Pointer_State_Move();
                    Point dropPosition = e.GetPosition(ReceiveDrop);
                    Canvas.SetLeft(btn, dropPosition.X);
                    Canvas.SetTop(btn, dropPosition.Y);

                    try
                    {
                        ReceiveDrop.Children.Add(btn);
                    }
                    catch
                    {

                    }
                }
                else if (Convert.ToInt32(data) == (int)ElementConstant.Rotation_Style_Move)
                {
                    var btn = Get_Rotation_Style_Move();
                    Point dropPosition = e.GetPosition(ReceiveDrop);
                    Canvas.SetLeft(btn, dropPosition.X);
                    Canvas.SetTop(btn, dropPosition.Y);

                    try
                    {
                        ReceiveDrop.Children.Add(btn);
                    }
                    catch
                    {

                    }
                }
            }


            e.Handled = true;
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
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
            this.dragObject = sender as UIElement;
            this.Offset = e.GetPosition(this.ReceiveDrop);
            this.Offset.Y -= Canvas.GetTop(this.dragObject);
            this.Offset.X -= Canvas.GetLeft(this.dragObject);
            this.ReceiveDrop.CaptureMouse();
        }

        private void ReceiveDrop_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.dragObject == null)
                return;
            var position = e.GetPosition(sender as IInputElement);
            Canvas.SetTop(this.dragObject, position.Y - this.Offset.Y);
            Canvas.SetLeft(this.dragObject, position.X - this.Offset.X);
        }

        private void ReceiveDrop_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.dragObject = null;
            this.ReceiveDrop.ReleaseMouseCapture();
        }

        public void VerifyCloseElement()
        {
            var child = ReceiveDrop.Children.OfType<Button>();
            foreach (var ch in child)
            {
                if (ch.Content == null)
                {
                    TextBlock closeButton = new TextBlock();
                    closeButton.Background = new SolidColorBrush(Colors.Transparent);
                    closeButton.FontSize = 14;
                    closeButton.FontWeight = FontWeights.Bold;
                    closeButton.Width = 15;
                    closeButton.Height = 15;
                    closeButton.Margin = new Thickness(35, -55, -20, 0);
                    closeButton.Foreground = new SolidColorBrush(Colors.Black);
                    closeButton.Visibility = Visibility.Visible;
                    closeButton.Text = "X";
                    ch.Content = closeButton;
                    //ch.PreviewMouseMove += Ch_PreviewMouseMove;
                    //ch.PreviewMouseUp += Ch_PreviewMouseUp;
                    ch.PreviewMouseDown += Ch_PreviewMouseDown;

                }
                else
                {

                    TextBlock txtbk = (TextBlock)ch.Content;
                    ch.Content = null;
                    ch.PreviewMouseDown += Ch_PreviewMouseDown;
                }
            }
        }


        // Copy Element of Defined Type.
        private Button Get_Ten_Steps_Move()
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
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Ten_Steps_Move;

            return btn;
        }

        private Button Get_Turn_Fiften_Degree_Right_Move()
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
            btn.Style = this.FindResource("BlueMoveRight") as Style;
            btn.Width = 150;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Turn_Fiften_Degree_Right_Move;

            return btn;
        }

        private Button Get_Turn_Fiften_Degree_Left_Move()
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
            btn.Style = this.FindResource("BlueMoveLeft") as Style;
            btn.Width = 150;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Turn_Fiften_Degree_Left_Move;

            return btn;
        }

        private Button Get_Pointer_State_Move()
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
            btn.Style = this.FindResource("BlueMovePointer") as Style;
            btn.Width = 200;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Pointer_State_Move;

            return btn;
        }

        private Button Get_Rotation_Style_Move()
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
            btn.Style = this.FindResource("BlueMoveRotation") as Style;
            btn.Width = 175;
            btn.Height = 42;
            btn.IsHitTestVisible = IsChildHitTestVisible;
            btn.Tag = (int)ElementConstant.Rotation_Style_Move;

            return btn;
        }
    }
}
