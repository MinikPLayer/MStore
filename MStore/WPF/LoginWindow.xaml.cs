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

namespace MStore.Login
{
    /// <summary>
    /// Logika interakcji dla klasy LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool loggedIn = false;

        public const bool debug = false;

        private void DebugAutoLog()
        {
            LoginTextBot.Text = "abcd";
            PasswordTextBox.Password = "abcd";

            RegisterButton_Click(this, new RoutedEventArgs());
            LoginButton_Click(this, new RoutedEventArgs());
        }

        public LoginWindow()
        {
            InitializeComponent();


            if (debug)
            {
                //Speed up debugging
                DebugAutoLog();
            }

            
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

            
            //Placeholder

            Console.WriteLine("Login: " + LoginTextBot.Text);
            Console.WriteLine("Password: " + PasswordTextBox.Password);

            if (LoginTextBot.Text.Length == 0)
            {
                MessageBox.Show("Login cannot be empty", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(PasswordTextBox.Password.Length == 0)
            {
                MessageBox.Show("Password cannot be empty", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string result = App.storeClient.Login(LoginTextBot.Text, PasswordTextBox.Password);

            Debug.Log("Login result: " + result);

            if(result == "LS:OK")
            {
                loggedIn = true;

                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.OpenLibrary();

                Application.Current.MainWindow.Show();
                this.Close();
            }
            else
            {
                string message = result;
                //if(message == "LS:NR")
                //{
                //   message = "User not registered, user register button to register";
                //}

                switch (message)
                {
                    case "LS:NR":
                        message = "User not registered, user register button to register";
                        break;
                    case "LS:BP":
                        message = "Bad password";
                        break;
                }

                if (!debug)
                {
                    MessageBox.Show(message, "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

           



            
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Login: " + LoginTextBot.Text);
            Console.WriteLine("Password: " + PasswordTextBox.Password);

            if (LoginTextBot.Text.Length == 0)
            {
                MessageBox.Show("Login cannot be empty", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PasswordTextBox.Password.Length == 0)
            {
                MessageBox.Show("Password cannot be empty", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string result = App.storeClient.Register(LoginTextBot.Text, PasswordTextBox.Password);

            Debug.Log("Login result: " + result);

            if (result == "RS:OK")
            {
                if (!debug)
                {
                    //Debugging
                    MessageBox.Show("User " + LoginTextBot.Text + " successfully registered", "Register complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {   
                //return;

                string message = result;
                if (message == "RS:AR")
                {
                    message = "User " + LoginTextBot.Text + " is already registered";
                }

                if (!debug)
                {
                    //Debugging
                    MessageBox.Show(message, "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }
    }
}
