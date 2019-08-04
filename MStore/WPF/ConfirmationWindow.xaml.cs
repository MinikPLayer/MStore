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
    /// Logika interakcji dla klasy ConfirmationWindow.xaml
    /// </summary>
    public partial class ConfirmationWindow : Window
    {
        private Action<bool> userDecidedFunction;

        public bool value = false;

        public ConfirmationWindow(Action<bool> _decisionFunction)
        {
            InitializeComponent();

            userDecidedFunction = _decisionFunction;
        }

        public ConfirmationWindow()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (userDecidedFunction != null)
            {
                userDecidedFunction.Invoke(true);
            }

            value = true;

            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            if (userDecidedFunction != null)
            {
                userDecidedFunction.Invoke(false);
            }

            value = false;

            this.Close();
        }
    }
}
