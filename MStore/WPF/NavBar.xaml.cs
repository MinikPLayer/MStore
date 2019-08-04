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

namespace MStore.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy NavBar.xaml
    /// </summary>
    public partial class NavBar : UserControl
    {
        public NavBar()
        {
            InitializeComponent();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new Menu().Content);
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new Library().Content);
        }

        private void DownloadMenuButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new DownloadMenu().Content);
        }
    }
}
