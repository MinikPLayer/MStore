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

using System.IO;

namespace MStore.Login
{
    /// <summary>
    /// Logika interakcji dla klasy LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool loggedIn = false;

        public const bool debug = false;

        public static string userInfoFileName = StoreClient.GetPath("user.dat");

        private void DebugAutoLog()
        {
            LoginTextBox.Text = "abcd";
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


            LoadUserInfoFromFile();

            if (loggedIn) return;


            this.ShowDialog();
            
        }

        bool autoLogin = false;
        private void ParseConfigLine(string line)
        {
            if(line.StartsWith("login="))
            {
                line = line.Remove(0, "login=".Length);

                LoginTextBox.Text = line;

                return;
            }
            else if(line.StartsWith("password="))
            {
                line = line.Remove(0, "password=".Length);

                PasswordTextBox.Password = line;

                return;
            }
            else if(line.StartsWith("autologin="))
            {
                line = line.Remove(0, "autologin=".Length);

                if(line == "true")
                {
                    autoLogin = true;
                }
                else if(line == "false")
                {
                    autoLogin = false;
                }
            }
        }

        private void LoadUserInfoFromFile()
        {
            if(!File.Exists(userInfoFileName))
            {
                return;
            }
            string[] lines = File.ReadAllLines(userInfoFileName);
            for(int i = 0;i<lines.Length;i++)
            {
                ParseConfigLine(lines[i]);
            }

            if(autoLogin)
            {
                LoginButton_Click(this, new RoutedEventArgs());
            }
        }

        private void SaveUserInfoToFile(string login = "", string password = "", bool autoLogin = false)
        {
            string dataToWrite = "";
            if(login.Length != 0)
            {
                dataToWrite += "login=" + login + '\n';
            }

            if(password.Length != 0)
            {
                dataToWrite += "password=" + password + '\n';
            }

            if(autoLogin)
            {
                dataToWrite += "autologin=true";
            }
            else
            {
                dataToWrite += "autologin=false";
            }


            File.WriteAllText(userInfoFileName, dataToWrite);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

            
            //Placeholder

            Console.WriteLine("Login: " + LoginTextBox.Text);
            Console.WriteLine("Password: " + PasswordTextBox.Password);

            if (LoginTextBox.Text.Length == 0)
            {
                MessageBox.Show("Login cannot be empty", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(PasswordTextBox.Password.Length == 0)
            {
                MessageBox.Show("Password cannot be empty", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string result = App.storeClient.Login(LoginTextBox.Text, PasswordTextBox.Password);

            Debug.Log("Login result: " + result);

            if(result == "LS:OK")
            {
                loggedIn = true;

                if(RememberMeSwitch.IsChecked == true)
                {
                    SaveUserInfoToFile(LoginTextBox.Text, PasswordTextBox.Password, true);
                }

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
            Console.WriteLine("Login: " + LoginTextBox.Text);
            Console.WriteLine("Password: " + PasswordTextBox.Password);

            if (LoginTextBox.Text.Length == 0)
            {
                MessageBox.Show("Login cannot be empty", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PasswordTextBox.Password.Length == 0)
            {
                MessageBox.Show("Password cannot be empty", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string result = App.storeClient.Register(LoginTextBox.Text, PasswordTextBox.Password);

            Debug.Log("Login result: " + result);

            if (result == "RS:OK")
            {
                if (!debug)
                {
                    //Debugging
                    MessageBox.Show("User " + LoginTextBox.Text + " successfully registered", "Register complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {   
                //return;

                string message = result;
                if (message == "RS:AR")
                {
                    message = "User " + LoginTextBox.Text + " is already registered";
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
