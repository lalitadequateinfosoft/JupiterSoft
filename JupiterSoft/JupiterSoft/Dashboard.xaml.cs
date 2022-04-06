using System;
using System.Collections.Generic;
using System.IO;
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
using JupiterSoft.Models;
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
        private string _FileDirectory = ApplicationConstant._FileDirectory;
        public Dashboard()
        {  
            InitializeComponent();
            this.DataContext = MyCommand;
            MyCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control)); 
            ChildPage = new CreateTemplate();
            this.frame.Content = null;
            ChildPage.ParentWindow = this;
            this.frame.Content = ChildPage;
        }

        public Dashboard(string _defaultFile)
        {
            InitializeComponent();
            this.DataContext = MyCommand;
            MyCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            ChildPage = new CreateTemplate(_defaultFile);
            this.frame.Content = null;
            ChildPage.ParentWindow = this;
            this.frame.Content = ChildPage;
        }

        public void MyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ChildPage.SaveInitiated();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if(item.Tag.ToString().ToLower()=="new")
            {
                ChildPage = new CreateTemplate();
                this.frame.Content = null;
                ChildPage.ParentWindow = this;
                this.frame.Content = ChildPage;
            }
            else if (item.Tag.ToString().ToLower() == "open")
            {

            }
            else if (item.Tag.ToString().ToLower() == "close")
            {
                var dashForm = new MainWindow();
                dashForm.Show();
                this.Close();
            }
            else if (item.Tag.ToString().ToLower() == "save")
            {
                ChildPage.SaveInitiated();
            }
            else if (item.Tag.ToString().ToLower() == "exit")
            {
                this.Close();
            }
        }

        //private List<FileSystemModel> GetFileSystems()
        //{
        //    List<FileSystemModel> files = new List<FileSystemModel>();
        //    if (System.IO.Directory.Exists(_FileDirectory))
        //    {
        //        DirectoryInfo d = new DirectoryInfo(_FileDirectory);

        //        FileInfo[] Files = d.GetFiles("*.json");
        //        if (Files != null && Files.Length > 0)
        //        {
        //            foreach (FileInfo file in Files)
        //            {
        //                if (file.Name.Contains('_'))
        //                {
        //                    string[] FileSpl = file.Name.Split('_');
        //                    if (FileSpl.Last().ToString().ToLower() == "default.json")
        //                    {
        //                        var dashForm = new Dashboard(file.FullName.ToString());
        //                        dashForm.Show();
        //                        this.Close();
        //                        break;
        //                    }
        //                }

        //            }
        //        }


        //    }
        //}
    }
}
