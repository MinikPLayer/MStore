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

namespace MStore
{
    /// <summary>
    /// Logika interakcji dla klasy DownloadMenu.xaml
    /// </summary>
    public partial class AdminMenu : Window
    {
        
        public AdminMenu()
        {
            InitializeComponent();

            NavBar.SetMode(Controls.NavBar.Modes.Admin);
        }

        private void UsersMenuButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new AdminUserMenu().Content);
        }

        private void VouchersMenuButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new AdminVoucherMenu().Content);
        }
    }
}
