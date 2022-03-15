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
        BrushConverter bc;
        public static readonly DependencyProperty IsChildHitTestVisibleProperty =
            DependencyProperty.Register("IsChildHitTestVisible", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(true));

        UIElement dragObject = null;
        Point Offset;
        public bool IsChildHitTestVisible
        {
            get { return (bool)GetValue(IsChildHitTestVisibleProperty); }
            set { SetValue(IsChildHitTestVisibleProperty, value); }
        }
        public CreateTemplate()
        {
            bc = new BrushConverter();
            InitializeComponent();
        }

        private void ButtonGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            var sArea = sender as StackPanel;
            if(sArea.Name== "ButtonGridArea1")
            {
                buttonGrid1.Opacity = 0.5;
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if(sArea.Name== "ButtonGridArea2")
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
            var cSender=sender as Border;
            UIElement element = VisualTreeHelper.GetParent(cSender) as UIElement;
            string pName = (element as Grid).Name;
            element.Opacity = 0.5;
            if (pName== "buttonGrid1")
            {
                ButtonGridArea1.BringIntoView();
                buttonGrid2.ClearValue(UIElement.OpacityProperty);
                buttonGrid3.ClearValue(UIElement.OpacityProperty);
                buttonGrid4.ClearValue(UIElement.OpacityProperty);
                buttonGrid5.ClearValue(UIElement.OpacityProperty);
                buttonGrid6.ClearValue(UIElement.OpacityProperty);
                buttonGrid7.ClearValue(UIElement.OpacityProperty);
            }
            else if(pName== "buttonGrid2")
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
            // copy = XamlReader.Parse(XamlWriter.Save(sender)) as UIElement;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsChildHitTestVisible = false;
                DataObject dragData = new DataObject();
                dragData.SetData(DataFormats.StringFormat, sender as UIElement);
                DragDrop.DoDragDrop(this, dragData, DragDropEffects.Copy);
                IsChildHitTestVisible = true;


                //IsChildHitTestVisible = false;
                //DragDrop.DoDragDrop(copy, new DataObject(DataFormats.Serializable, copy), DragDropEffects.Copy);
                //IsChildHitTestVisible = true;

            }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is UIElement element)
            {
                //var btn = element as Button;
                //btn.PreviewMouseDown += Ch_PreviewMouseDown;
                Point dropPosition = e.GetPosition(ReceiveDrop);
                Canvas.SetLeft(element, dropPosition.X);
                Canvas.SetTop(element, dropPosition.Y);

                try
                {
                    ReceiveDrop.Children.Add(element);
                }
                catch
                {

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
    }
}
