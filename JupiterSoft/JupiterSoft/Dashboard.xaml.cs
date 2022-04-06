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
            if (item.Tag.ToString().ToLower() == "new")
            {
                ChildPage = new CreateTemplate();
                this.frame.Content = null;
                ChildPage.ParentWindow = this;
                this.frame.Content = ChildPage;
            }
            else if (item.Tag.ToString().ToLower() == "open")
            {
                var FileData = GetFileSystems();
                if (FileData != null && FileData.Count() > 0)
                {
                    FiledataGrid.ItemsSource = null;
                    FiledataGrid.ItemsSource = FileData.ToList();
                    FiledataGrid.Visibility = Visibility.Visible;
                }
                else { FileMessage.Visibility = Visibility.Visible; }

                Filepopup.IsOpen = true;
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

        private List<FileSystemModel> GetFileSystems()
        {
            List<FileSystemModel> files = new List<FileSystemModel>();
            if (System.IO.Directory.Exists(_FileDirectory))
            {
                DirectoryInfo d = new DirectoryInfo(_FileDirectory);

                FileInfo[] Files = d.GetFiles("*.json");
                if (Files != null && Files.Length > 0)
                {
                    foreach (FileInfo file in Files)
                    {
                        if (file.Name.Contains('_'))
                        {
                            string[] FileSpl = file.Name.Split('_');
                            if (FileSpl.Last().ToString().ToLower() == "default.json")
                            {
                                files.Add(new FileSystemModel
                                {
                                    FileName = file.FullName,
                                    FileId = file.Name.Substring(0, file.Name.Length - "_default.json".Length),
                                    CreatedDate = file.CreationTime
                                });
                            }
                            else
                            {
                                files.Add(new FileSystemModel
                                {
                                    FileName = file.FullName,
                                    FileId = file.Name.Split('.')[0],
                                    CreatedDate = file.CreationTime
                                });
                            }
                        }
                        else
                        {
                            files.Add(new FileSystemModel
                            {
                                FileName = file.FullName,
                                FileId = file.Name.Split('.')[0],
                                CreatedDate = file.CreationTime
                            });
                        }


                    }
                }


            }

            return files;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            FileMessage.Visibility = Visibility.Hidden;
            Filepopup.IsOpen = false;
            FiledataGrid.Visibility = Visibility.Hidden;


            var data = sender as Button;
            data.IsEnabled = false;
            ChildPage = new CreateTemplate(data.Tag.ToString());
            this.frame.Content = null;
            ChildPage.ParentWindow = this;
            this.frame.Content = ChildPage;
        }

        private void PopupClose_Click(object sender, RoutedEventArgs e)
        {
            FileMessage.Visibility = Visibility.Hidden;
            Filepopup.IsOpen = false;
            FiledataGrid.Visibility = Visibility.Hidden;
        }
    }
}
