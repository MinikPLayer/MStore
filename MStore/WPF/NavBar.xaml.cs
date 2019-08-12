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

        public enum Modes
        {
            none,
            Library,
            Store,
            Download
        }

        public void SetMode(Modes mode)
        {
            switch (mode)
            {
                case Modes.none:
                    break;
                case Modes.Library:
                    {
                        SolidColorBrush brush = (SolidColorBrush)LibraryButton.Background;
                        brush.Color = Color.FromArgb(50, brush.Color.R, brush.Color.G, brush.Color.B);
                        break;
                    }
                case Modes.Store:
                    {
                        SolidColorBrush brush = (SolidColorBrush)StoreButton.Background;
                        brush.Color = Color.FromArgb(50, brush.Color.R, brush.Color.G, brush.Color.B);
                        break;
                    }
                case Modes.Download:
                    {
                        SolidColorBrush brush = (SolidColorBrush)DownloadMenuButton.Background;
                        brush.Color = Color.FromArgb(50, brush.Color.R, brush.Color.G, brush.Color.B);
                        break;
                    }
                default:
                    break;
            }
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            if(App.library == null)
            {
                App.library = new Library();
            }
            PageManager.SetPage(App.library.Content);
        }

        private void DownloadMenuButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new DownloadMenu().Content);
        }

        private void StoreButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new Store().Content);
        }
    }
}
