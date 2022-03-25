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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JupiterSoft.Pages;

namespace JupiterSoft
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public static RoutedCommand MyCommand = new RoutedCommand();
        CreateTemplate ChildPage; 
        public Dashboard()
        {  
            InitializeComponent();
            this.DataContext = MyCommand;
            MyCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            //frame.NavigationService.Navigate(new CreateTemplate());
            ChildPage = new CreateTemplate();
            this.frame.Content = null;
            ChildPage.ParentWindow = this;
            this.frame.Content = ChildPage;
        }

        public void MyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ChildPage.SaveInitiated();
        }
    }
}
