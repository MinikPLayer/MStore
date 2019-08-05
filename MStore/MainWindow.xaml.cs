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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MStore
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void App_DisplayLog(object sender, string e)
        {
            LogText.Text += e;
            
        }

        public MainWindow()
        {
            InitializeComponent();

            App.DisplayLogOnWindow += App_DisplayLog;

            Login.LoginWindow loginWindow = new Login.LoginWindow();



            //Debugging V

            //loginWindow.Show();
            if (!Login.LoginWindow.debug)
            {
                loginWindow.ShowDialog();
            }
            //this.Visibility = Visibility.Hidden;
            


            //Login.LoginWindow
            //Menu.LibraryButton_Click += SetPage;

            //Menu menu = new Menu();
           
        }

        public void OpenLibrary()
        {
            SetPage(new Library().Content);
        }

        public void SetPage(object sender)
        {
            this.Content = sender;
        }
    }

    public static class PageManager
    {
        public static void SetPage(object page)
        {
            Application.Current.MainWindow.Content = page;
        }
    }
}
