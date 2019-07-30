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

using System.Windows.Markup;
using System.IO;
using System.Xml;

namespace MStore
{
    /// <summary>
    /// Logika interakcji dla klasy Library.xaml
    /// </summary>
    public partial class Library : Window
    {
        StoreClient client;

        int clickedTextBlockIndex = -1;

        string gameTextBlockPrefab = "";

        private static List<Game> games = new List<Game>();

        private Game actualGameSelected;

        public string appPath = "./installed/";

        public User userInfo = new User();

        public class User
        {
            public string userName = "";
            public string token = "";
            public Int64 id = -1;

            public User(string _userName, string _token, Int64 _id)
            {
                userName = _userName;
                token = _token;
                id = _id;
            }

            public User()
            {

            }
        }

        public class Game
        {
            public string name;
            public long id;

            public string price = "";

            public string path = "";

            public bool installed = false;
            //Icon

            public Game(string gameName, int gameID)
            {
                name = gameName;
                id = gameID;
            }
        }

        private Game FindGame(string name, out int listPosition)
        {
            
            for(int i = 0;i<games.Count;i++)
            {
                if(name == games[i].name)
                {
                    listPosition = i;
                    return games[i];
                }
            }

            listPosition = -1;
            return null;

        }

        private Game FindGame(string name)
        {

            int notUsedOut = 0;
            return FindGame(name, out notUsedOut);

        }

        private int FindGameInList(string name)
        {
            for(int i = 0;i<GamesSP.Children.Count;i++)
            {
                if (GamesSP.Children[i].GetType() == typeof(TextBlock))
                {
                    TextBlock text = (TextBlock)GamesSP.Children[i];
                    if (text.Text == name) return i;
                }
            }

            return -1;
        }

        private void DisplayGameInfo(Game game)
        {
            //Debug.LogWarning("DisplayGameInfo(Game game) - Not implemented");

            GameTitle.Text = game.name;

            if(game.installed)
            {
                RunButton.Content = "Run";
            }
            else
            {
                RunButton.Content = "Install";
            }
        }

        //DO POPRAWY
        public void OnGameTextClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            Debug.Log(textBlock.Text + " clicked");

            if(clickedTextBlockIndex != -1)
            {
                //clickedTextBlock.Background.Opacity = 100;
                TextBlock clickedTextBlock = (TextBlock)GamesSP.Children[clickedTextBlockIndex];
                clickedTextBlock.Background.Opacity = 100;

                Debug.Log("Opacity 100");
            }
            clickedTextBlockIndex = FindGameInList(textBlock.Text);
            //textBlock.Background.Opacity = 0;
            TextBlock text = (TextBlock)GamesSP.Children[clickedTextBlockIndex];
            text.Background.Opacity = 0;

            actualGameSelected = FindGame(textBlock.Text);

            DisplayGameInfo(actualGameSelected);
        }


        SolidColorBrush notInstalledColor = Brushes.DarkGray;
        private void AddGameToTheList(Game game)
        {
            if(GameListTextTemplate.Text == "")
            {
                GameListTextTemplate.Text = game.name;
                GameListTextTemplate.MouseLeftButtonDown += OnGameTextClick;

                if (!game.installed)
                {
                    GameListTextTemplate.Foreground = notInstalledColor;
                }

                return;
            }

            StringReader reader = new StringReader(gameTextBlockPrefab);
            XmlReader xmlReader = XmlReader.Create(reader);
            TextBlock newButton = (TextBlock)XamlReader.Load(xmlReader);

            newButton.Text = game.name;

            newButton.MouseLeftButtonDown += OnGameTextClick;

            if(!game.installed)
            {
                newButton.Foreground = notInstalledColor;
            }

            GamesSP.Children.Add(newButton);
        }

        public Library()
        {
            InitializeComponent();


            client = App.storeClient;


            gameTextBlockPrefab = XamlWriter.Save(GameListTextTemplate);

            //string libraryStr = client.RecquestLibrary();

            //TextBlock newButton = new TextBlock();
            //newButton.Text = "Fortnite";
            //newButton.MouseLeftButtonDown += OnGameTextClick;

            //AddGameToTheList("Fortnite");
            //AddGameToTheList("Minecraft");

            GetUserInfo();

            Debug.Log("User info: ");
            Debug.Log("ID: " + userInfo.id);
            Debug.Log("Username: " + userInfo.userName);
            Debug.Log("Token: " + userInfo.token);

            GetGamesList();

        }

        private void GetUserInfo()
        {
            

            string data = client.RequestUserInfo();

            if(data.Length < 5)
            {
                Debug.LogError("Some sort of error, user info is less than 5 chars long");
                return;
            }

            userInfo = new User();

            string info = "";


            //user id
            data = GetStringToSpecialCharAndDelete(data, '\n', out info);


            Int64 _id;
            if(Int64.TryParse(info, out _id))
            {
                userInfo.id = _id;
            }
            else
            {
                Debug.LogError("Cannot parse \"" + info + "\" to Int64");
                return;
            }


            //Token
            data = GetStringToSpecialCharAndDelete(data, '\n', out info);

            if(info.Length > 0)
            {
                userInfo.token = info;
            }
            else
            {
                Debug.LogError("Cannot parse \"" + info + "\" to string");
                return;
            }

            //Username
            data = GetStringToSpecialCharAndDelete(data, '\n', out info);

            if (info.Length > 0)
            {
                userInfo.userName = info;
            }
            else
            {
                Debug.LogError("Cannot parse \"" + info + "\" to string");
                return;
            }

            //Games count not important for now
        }


        private string GetStringToSpecialChar(string str, char specialChar)
        {
            string value = "";

            for(int i = 0;i<str.Length;i++)
            {
                if (str[i] == specialChar) return value;
                value += str[i];
            }

            return value;
        }

        /// <summary>
        /// Returns string from special character ( without it )
        /// </summary>
        /// <param name="str"></param>
        /// <param name="specialChar"></param>
        /// <param name="strToSpecialChar"></param>
        /// <returns></returns>
        private string GetStringToSpecialCharAndDelete(string str, char specialChar, out string strToSpecialChar)
        {
            strToSpecialChar = GetStringToSpecialChar(str, specialChar);

            return str.Remove(0, strToSpecialChar.Length + 1);
        }

        private Game ParseStringToGame(string gameInfo)
        {
            string data = "";

            gameInfo = GetStringToSpecialCharAndDelete(gameInfo, '\n', out data);

            Game game = new Game("", -1);

            if (!long.TryParse(data, out game.id))
            {
                Debug.LogError("Cannot parse " + data + " to string");
                return null;
            }

            gameInfo = GetStringToSpecialCharAndDelete(gameInfo, '\n', out data);

            //game.id = long.Parse(data);
            game.name = data;


            gameInfo = GetStringToSpecialCharAndDelete(gameInfo, '\n', out data);

            game.price = data;

            gameInfo = GetStringToSpecialCharAndDelete(gameInfo, '\n', out data);

            game.path = data;

            return game;
        }

        public bool CheckIfGameIsInstalled(Game game)
        {
            if (Directory.Exists(appPath + game.path)) return true;
            return false;
        }

        public void MarkInstalledGames()
        {
            for(int i = 0;i<games.Count;i++)
            {
                if(CheckIfGameIsInstalled(games[i]))
                {
                    games[i].installed = true;
                }
            }
        }

        public void GetGamesList(bool forceRefresh = false)
        {
            if (games.Count == 0 || forceRefresh)
            {
                string library = client.RecquestLibrary();

                Debug.Log("Library:");
                Debug.Log("\n\n");
                Debug.Log(library);
                Debug.Log("\n\n");

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
                        long id = -1;
                        if (!long.TryParse(gameID, out gamesIDs[actualGame]))
                        {
                            Debug.Log("Cannot parse " + gameID);
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
                    Game game = ParseStringToGame(client.RequestGameInfo(gamesIDs[i]));
                    Debug.Log("Info for the game with id " + gamesIDs[i] + ": ");
                    if (game == null)
                    {
                        Debug.LogError("Something gone desperatelly wrong, game info not found in client memory");
                    }
                    Debug.Log("id: " + game.id);
                    Debug.Log("Name: " + game.name);
                    Debug.Log("Price: " + game.price);


                    games.Add(game);

                    //AddGameToTheList(game);
                }
            }


            SortGamesAlphabetically();

            MarkInstalledGames();

            for (int i = 0; i < games.Count; i++)
            {
                AddGameToTheList(games[i]);
            }

            

            if (games.Count > 0)
            {
                actualGameSelected = games[0];
                DisplayGameInfo(games[0]);
            }
            

           
        }

        

        /// <summary>
        /// If stringToCompare is higher alphabetically returns true, false otherwise. Returns false if the same
        /// </summary>
        /// <param name="original"></param>
        /// <param name="stringToCompare"></param>
        /// <returns></returns>
        public bool IsHigherAlphabetically(string original, string stringToCompare)
        {
            for(int i = 0;i<original.Length && i < stringToCompare.Length;i++)
            {
                if(original[i] > stringToCompare[i])
                {
                    return true;
                }
                if(original[i] < stringToCompare[i])
                {
                    return false;
                }
            }
            if(original.Length > stringToCompare.Length)
            {
                return true;
            }
            if(original.Length < stringToCompare.Length)
            {
                return false;
            }

            return false;
        }

        public void SortGamesAlphabetically()
        {
            List<Game> newGamesList = new List<Game>();

            //int iterator = 0;

            while(games.Count != 0)
            {
                Game actualHighestGame = games[0];
                for(int i = 1;i<games.Count;i++)
                {
                    Debug.Log("Comparing " + actualHighestGame.name + " and " + games[i].name);
                    if(IsHigherAlphabetically(actualHighestGame.name, games[i].name))
                    {
                        Debug.Log("Higher");
                        actualHighestGame = games[i];
                    }
                    else
                    {
                        Debug.Log("Lower");
                    }
                }
                newGamesList.Add(actualHighestGame);

                games.Remove(actualHighestGame);
                
            }

            games = newGamesList;
        }


        public void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new Menu().Content);
        }

        private void NavBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
