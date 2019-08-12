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
    public partial class VoucherWindow : Window
    {
        public string voucherCode = "";

        public VoucherWindow()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            voucherCode = VoucherBox.Text;
            for(int i = 0;i<voucherCode.Length;i++)
            {
                if(voucherCode[i] == '-')
                {
                    voucherCode = voucherCode.Remove(i, 1);
                    i--;
                }
            }

            if(voucherCode.Length > 12)
            {
                MessageBox.Show("Voucher cannot be longer than 12 characters", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                voucherCode = "";
            }
            else
            {
                Debug.Log("Closing");
                this.Close();
            }
        }
    }
}
