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

using System.Xml;
using System.Windows.Markup;
using System.IO;

namespace MStore
{
    /// <summary>
    /// Logika interakcji dla klasy DownloadMenu.xaml
    /// </summary>
    public partial class AdminUserMenu : Window
    {


        public enum Commands
        {
            unknown,
            ChangeMoney,
            AddMoney,
            SubstractMoney,
            AddVoucher,
            DeleteVoucher,
            ChangeVoucher,
            UserInfo,
            UserGamesList,
            UserGameRemove,
            UserGameAdd,
            userID,

        }

        public static void SendCommand(Commands command, string parameters)
        {

            switch (command)
            {
                case Commands.unknown:
                    Debug.LogError("Unknown command");
                    return;
                case Commands.ChangeMoney:
                    App.storeClient.socket._Send("ADMIN" + "MNYCH" + parameters);
                    break;
                case Commands.AddMoney:
                    App.storeClient.socket._Send("ADMIN" + "MNYAD" + parameters);
                    break;
                case Commands.SubstractMoney:
                    App.storeClient.socket._Send("ADMIN" + "MNYSB" + parameters);
                    break;
                case Commands.AddVoucher:
                    App.storeClient.socket._Send("ADMIN" + "VCHAD" + parameters);
                    break;
                case Commands.DeleteVoucher:
                    App.storeClient.socket._Send("ADMIN" + "VCHDL" + parameters);
                    break;
                case Commands.ChangeVoucher:
                    App.storeClient.socket._Send("ADMIN" + "VCHCH" + parameters);
                    break;
                case Commands.UserInfo:
                    App.storeClient.socket._Send("ADMIN" + "USRNF" + parameters);
                    break;
                case Commands.userID:
                    App.storeClient.socket._Send("ADMIN" + "USRID0;" + parameters);
                    break;

                // Parameters: {usrID};
                case Commands.UserGamesList:
                    App.storeClient.socket._Send("ADMIN" + "UGMLS" + parameters);
                    break;

                // Parameters: {usrID};{gameID};
                case Commands.UserGameAdd:
                    App.storeClient.socket._Send("ADMIN" + "UGMAD" + parameters);
                    break;

                // Parameters: {usrID};{id [i]/gameName [n]};{gameID};
                case Commands.UserGameRemove:   
                    App.storeClient.socket._Send("ADMIN" + "UGMRM" + parameters);
                    break;
                default:
                    Debug.LogError("Command " + command + " not implemented!");
                    break;
            }
        }

        public static class AdminCommands
        {
            public static string AddMoney(long userID, decimal value)
            {
                if (value == 0) return "OK";
                SendCommand(Commands.AddMoney, userID.ToString() + ";" + value.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string ChangeMoney(long userID, decimal value)
            {
                SendCommand(Commands.ChangeMoney, userID.ToString() + ";" + value.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string SubstractValue(long userID, decimal value)
            {
                if (value == 0) return "OK";
                SendCommand(Commands.SubstractMoney, userID.ToString() + ";" + value.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string AddVoucher(long gameID, int uses, string code)
            {
                if(gameID < 0 || uses == 0 || code.Length == 0)
                {
                    return "OK";
                }

                SendCommand(Commands.AddVoucher, gameID.ToString() + ";" + uses.ToString() + ";" + code);

                return App.storeClient.socket.WaitForReceive();
            }

            public static string DeleteVoucher(string code)
            {
                if (code.Length == 0)
                {
                    return "OK";
                }

                SendCommand(Commands.AddVoucher, code);

                return App.storeClient.socket.WaitForReceive();
            }

            public static string ChangeVoucher(string code, long gameID, int uses)
            {
                if (gameID < 0 || uses == 0 || code.Length == 0)
                {
                    return "OK";
                }

                SendCommand(Commands.ChangeVoucher, gameID.ToString() + ";" + uses.ToString() + ";" + code);

                return App.storeClient.socket.WaitForReceive();
            }

            public static string UserInfo(long userID)
            {
                if(userID < 0)
                {
                    return "BU";
                }

                SendCommand(Commands.UserInfo, userID.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string GetUserGamesList(long userID)
            {
                if (userID < 0)
                {
                    return "BU";
                }

                SendCommand(Commands.UserGamesList, userID.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string AddGameToUser(long userID, long gameID)
            {
                if(userID < 0)
                {
                    return "BU";
                }
                if(gameID < 0)
                {
                    return "BG";
                }

                //          Add game to suer    , userID             type:ID  gameID
                SendCommand(Commands.UserGameAdd, userID.ToString() + ";i;" + gameID.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string AddGameToUser(long userID, string gameName)
            {
                if (userID < 0)
                {
                    return "BU";
                }
                if (gameName.Length == 0)
                {
                    return "BG";
                }

                //          Add game to suer    , userID            type:name gameName
                SendCommand(Commands.UserGameAdd, userID.ToString() + ";n;" + gameName);

                return App.storeClient.socket.WaitForReceive();
            }

            public static string RemoveGameFromUser(long userID, long gameID)
            {
                if (userID < 0)
                {
                    return "BU";
                }
                if (gameID < 0)
                {
                    return "BG";
                }

                SendCommand(Commands.UserGameAdd, userID.ToString() + ";" + gameID.ToString());
                
                return App.storeClient.socket.WaitForReceive();
            }

            public static string GetUserID(string userName)
            {
                if(userName.Length == 0)
                {
                    return "BU";
                }

                SendCommand(Commands.userID, userName);

                return App.storeClient.socket.WaitForReceive();
            }
        }

        Library.User targetUser;
        List<Library.Game> targetUserGames = new List<Library.Game>();
        Library.Game targetGame;


        public static SolidColorBrush normalGameColor = Brushes.White;
        public static SolidColorBrush selectedGameColor = Brushes.Orange;
        public void OnGameClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            Debug.Log(textBlock.Text + " clicked");

            int id = -1;
            string idStr = MUtil.GetStringToSpecialChar(textBlock.Text, ')');
            if (!int.TryParse(idStr, out id))
            {
                Debug.ConversionError(idStr, "id", id);
                return;
            }

            if (id > targetUserGames.Count || id < 0)
            {
                Debug.LogError("Bad ID");
                return;
            }

            targetGame = targetUserGames[id];
            //UpdateUserGamesDisplay();

            ResetGamesListColors();

            textBlock.Foreground = selectedGameColor;
        }

        private void AddGame(Library.Game game)
        {
            StringReader reader = new StringReader(gamePrefab);
            XmlReader xmlReader = XmlReader.Create(reader);
            TextBlock newButton = (TextBlock)XamlReader.Load(xmlReader);

            newButton.Text = game.ToString();

            newButton.MouseLeftButtonDown += OnGameClick;

            GamesSP.Children.Add(newButton);
        }

        public List<Library.Game> RequestUserLibrary(long id)
        {
            string library = AdminCommands.GetUserGamesList(id); //client.RequestLibrary();

            List<Library.Game> games = new List<Library.Game>();

            int gameListSize = 0;

            for (int i = 0; i < library.Length; i++)
            {
                if (library[i] == '\n')
                {
                    gameListSize++;
                }
            }

            Int64[] gamesIDs = new Int64[gameListSize];


            string gameID = "";
            int actualGame = 0;
            for (int i = 0; i < library.Length; i++)
            {
                if (library[i] == '\n')
                {
                    if (!long.TryParse(gameID, out gamesIDs[actualGame]))
                    {
                        Debug.LogError("Cannot parse " + gameID);
                    }
                    else
                    {
                        actualGame++;
                    }



                    gameID = "";
                }
                else
                {
                    gameID += library[i];
                }
            }

            for (int i = 0; i < gameListSize; i++)
            {
                Library.Game game = Library.ParseStringToGame(App.storeClient.RequestGameInfo(gamesIDs[i]));
                if (game == null)
                {
                    Debug.LogError("Something gone desperatelly wrong, game info not found in client memory");
                }


                games.Add(game);

                //AddGameToTheList(game);
            }

            return games;
        }
        

        private void RequestUser(long id)
        {
            string response = AdminCommands.UserInfo(id);

            targetUser = Library.GetUserInfoFromString(response);

            targetUserGames = RequestUserLibrary(id);

            UpdateUserDisplay();
        }

        private void ResetGamesListColors()
        {
            for(int i = 0;i<targetUserGames.Count;i++)
            {
                TextBlock block = (TextBlock)GamesSP.Children[i];
                block.Foreground = normalGameColor;
            }
        }

        private void ClearUserInfo()
        {
            targetUser = null;
            targetUserGames.Clear();
            targetGame = null;

            GamesSP.Children.Clear();

            //ResetGamesListColors();
        }

        private void UpdateUserDisplay()
        {
            GamesSP.Children.Clear();

            if(targetUser == null)
            {
                TargetUsernameText.Text = "Error, user not found";
                return;
            }
            TargetUsernameText.Text = targetUser.userName;
            if (targetUser.coins == "Free")
            {
                CoinsTextBox.Text = "0";
            }
            else
            {
                CoinsTextBox.Text = targetUser.coins.ToString();
            }

            UpdateUserGamesDisplay();
        }

        private void UpdateUserGamesDisplay()
        {
            if(targetUserGames == null)
            {
                return;
            }

            for(int i = 0;i<targetUserGames.Count;i++)
            {
                AddGame(targetUserGames[i]);
            }
        }

        string gamePrefab = "";

        public AdminUserMenu()
        {
            InitializeComponent();

            NavBar.SetMode(Controls.NavBar.Modes.Admin);

            gamePrefab = XamlWriter.Save(GameButtonTemplate);
            GamesSP.Children.Clear();

            RequestUser(0);
            //UpdateUserDisplay();
            //Debug.Log("Command response: " + response);
            if (targetUser == null)
            {
                Debug.LogError("Target user is null");
                return;
            }
            Debug.Log("Target user: " + targetUser.userName);

           
            
        }

        private bool IsNumber(char ch)
        {
            return (ch > 47 && ch < 58 ) || ch == ',';
        }

        private void UserIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*TextBox block = (TextBox)sender;
            for(int i = 0;i<block.Text.Length;i++)
            {
                if(!IsNumber(block.Text[i]))
                {
                    block.Text = block.Text.Remove(i, 1);
                    i--;
                }
            }*/
        }

        private void RequestButton_Click(object sender, RoutedEventArgs e)
        {
            if(UserIDTextBox.Text.Length == 0)
            {
                return;
            }

            long id = 0;

            if (!IsNumber(UserIDTextBox.Text[0]))
            {
                string strId = AdminCommands.GetUserID(UserIDTextBox.Text);
                if (!long.TryParse(strId, out id))
                {
                    Debug.LogError("Cannot parse \"" + strId + "\" to userID ( long )");
                    targetUser = null;
                    UpdateUserDisplay();
                    return;
                }
            }
            else
            {

                
                if (!long.TryParse(UserIDTextBox.Text, out id))
                {
                    Debug.LogError("Cannot parse \"" + UserIDTextBox.Text + "\" to userID ( long )");
                    targetUser = null;
                    UpdateUserDisplay();
                    return;
                }
            }


            ClearUserInfo();

            //Debug.Log("Requested user with id: " + id);
            RequestUser(id);
            //UpdateUserDisplay();
        }

        private void CoinsChangeButton_Click(object sender, RoutedEventArgs e)
        {
            decimal coins = 0;
            if (!decimal.TryParse(CoinsTextBox.Text, out coins))
            {
                Debug.LogError("Cannot parse \"" + CoinsTextBox.Text + "\" to coins ( float )");
                return;
            }

            if(targetUser == null)
            {
                Debug.LogError("Cannot change target user if it's null");
                return;
            }

            AdminCommands.ChangeMoney(targetUser.id, coins);

            long id = targetUser.id;

            ClearUserInfo();
            RequestUser(id);
            UpdateUserDisplay();
        }

        private void CoinsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox block = (TextBox)sender;
            for (int i = 0; i < block.Text.Length; i++)
            {
                if (!IsNumber(block.Text[i]))
                {
                    block.Text = block.Text.Remove(i, 1);
                    i--;
                }
            }
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            if(GameSearchBar.Text.Length == 0)
            {
                Debug.LogError("No game entered");
                return;
            }

            if(targetUser == null)
            {
                Debug.LogError("No user selected");
                return;
            }

            string response = "";
            if (IsNumber(GameSearchBar.Text[0]))
            {
                long gameID = 0;
                try
                {
                    gameID = long.Parse(GameSearchBar.Text);
                }
                catch(Exception)
                {
                    Debug.ConversionError(GameSearchBar.Text, "gameID", gameID);
                    return;
                }

                response = AdminCommands.AddGameToUser(targetUser.id, gameID);

                if(response != "OK")
                {
                    Debug.LogError("Add game to user error: \"" + response + "\"");
                    return;
                }

                return;
            }

            response = AdminCommands.AddGameToUser(targetUser.id, GameSearchBar.Text);

            if (response != "OK")
            {
                Debug.LogError("Add game to user error: \"" + response + "\"");
                return;
            }
        }

        private void GameSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.LogWarning("Searching for games not implemented yet");
            
        }
    }
}
