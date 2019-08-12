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

using System.Windows.Threading;

using System.IO.Compression;

using System.Diagnostics;

using System.Threading;

namespace MStore
{
    /// <summary>
    /// Logika interakcji dla klasy Library.xaml
    /// </summary>
    public partial class Library : Window
    {
        public static StoreClient client;

        int clickedTextBlockIndex = -1;

        string gameTextBlockPrefab = "";

        private static List<Game> games = new List<Game>();

        private static Game actualGameSelected;

        public static string appPath = "./installed/";
        public static string iconsPath = "./icons/";

        public static User userInfo = new User();

        private int gamesSPGamesStartIndex = -1;

        private ImageSource originalIcon;


        public static DownloadManager iconsDownloader;

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
            public string fileName = "";

            public string execName = "";

            public Size diskSize = new Size(-1);
            public Size downloadSize = new Size(-1);

            public Size alreadyDownloaded = new Size(-1);

            public bool installed = false;
            //Icon

            public Game(string gameName, int gameID)
            {
                name = gameName;
                id = gameID;
            }

            public struct Size
            {
                public long bytes;

                public Size(long _bytes)
                {
                    bytes = _bytes;
                }

                static string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
                public override string ToString()
                {
                    return ToStringWithPrecision(0);


                }

                /// <summary>
                /// Creates string with decimal point precision based on precision integrer
                /// </summary>
                /// <param name="precision"></param>
                /// <returns></returns>
                public string ToStringWithPrecision(int precision, bool spaceBetween = true)
                {
                    int level = 0;
                    double __bytes = bytes;

                    string spaceChar = "";
                    if(spaceBetween)
                    {
                        spaceChar = " ";
                    }

                    while (__bytes > 1024)
                    {
                        __bytes = __bytes / 1024f;
                        level++;
                    }

                    if (level >= sizeSuffixes.Length)
                    {
                        return bytes.ToString();
                    }

                    if(precision == 0)
                    {
                        //return __bytes.ToString() + sizeSuffixes[level];
                        long bytesLong = (long)__bytes;
                        return bytesLong.ToString();
                    }

                    /*string byteString = bytes.ToString();
                    string __bytesString = __bytes.ToString();

                    byteString.Insert(__bytesString.Length, ".");


                    int removePoint = __bytesString.Length + precision;
                    if(removePoint >= byteString.Length)
                    {
                        removePoint = byteString.Length - 1;
                    }
                    return byteString.Remove(removePoint) + sizeSuffixes[level];*/

                    string __bytesString = __bytes.ToString();

                    string returnValue = "";
                    int index = -1;
                    for(int i = 0;i<__bytesString.Length;i++)
                    {
                        returnValue += __bytesString[i];

                        if(__bytesString[i] == '.' || __bytesString[i] == ',')
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index == -1)
                    {
                        return __bytesString + spaceChar + sizeSuffixes[level];
                    }

                    for (int i = index + 1; i< index + 1 + precision && i < __bytesString.Length;i++)
                    {
                        returnValue += __bytesString[i];
                    }


                    return returnValue + spaceChar + sizeSuffixes[level];

                    //return __bytes.ToString() + sizeSuffixes[level];
                }

                public static Size operator +(Size s1, long add)
                {
                    s1.bytes += add;
                    return s1;
                }

            }
        }

        public static Game FindGame(long id)
        {
            int listPosition = 0;
            return FindGame(id, out listPosition);
        }

        public static Game FindGame(long id, out int listPosition)
        {
            for (int i = 0; i < games.Count; i++)
            {
                if (id == games[i].id)
                {
                    listPosition = i;
                    return games[i];
                }
            }

            listPosition = -1;
            return null;
        }

        private static Game FindGame(string name, out int listPosition)
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

        private void DisplayNoGamesInfo()
        {
            RunButton.Visibility = Visibility.Hidden;

            UninstallButton.Visibility = Visibility.Hidden;

            GameIcon.Visibility = Visibility.Hidden;
        }

        private void DisplayGameInfo(Game game)
        {
            //Debug.LogWarning("DisplayGameInfo(Game game) - Not implemented");

            LoadIcon(game.id);

            GameTitle.Text = game.name;

            RunButton.Visibility = Visibility.Visible;

            //Disable if icon is not downloaded
            GameIcon.Visibility = Visibility.Visible;

            if(game.installed)
            {
                RunButton.Content = "Run";
                UninstallButton.Visibility = Visibility.Visible;
            }
            else
            {
                RunButton.Content = "Install";
                UninstallButton.Visibility = Visibility.Hidden;
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
        SolidColorBrush installedColor = Brushes.White;
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
                else
                {
                    GameListTextTemplate.Foreground = installedColor;
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
            else
            {
                newButton.Foreground = installedColor;
            }

            GamesSP.Children.Add(newButton);
        }

        public Library()
        {
            InitializeComponent();

            if (client == null)
            {
                if (Directory.Exists(iconsPath))
                {
                    Directory.Delete(iconsPath, true);
                }
                Directory.CreateDirectory(iconsPath);
            }

            client = App.storeClient;
            iconsDownloader = new DownloadManager(client.ipAddress, client.downloadIconsPort);

            originalIcon = GameIcon.Source;


            gameTextBlockPrefab = XamlWriter.Save(GameListTextTemplate);

            //string libraryStr = client.RecquestLibrary();

            //TextBlock newButton = new TextBlock();
            //newButton.Text = "Fortnite";
            //newButton.MouseLeftButtonDown += OnGameTextClick;


            GetUserInfo();

            Debug.Log("User info: ");
            Debug.Log("ID: " + userInfo.id);
            Debug.Log("Username: " + userInfo.userName);
            Debug.Log("Token: " + userInfo.token);

            gamesSPGamesStartIndex = GamesSP.Children.Count;

            GetGamesList();


            //client.DownloadGame(0, userInfo.token);

        }

        private void GetUserInfo(bool force = false)
        {
            
            if(userInfo.id != -1 && !force)
            {
                return;
            }

            Debug.Log("Getting user info...");

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


            gameInfo = GetStringToSpecialCharAndDelete(gameInfo, '\n', out data);
            game.fileName = data;


            gameInfo = GetStringToSpecialCharAndDelete(gameInfo, '\n', out data);
            game.execName = data;

            //Size
            gameInfo = GetStringToSpecialCharAndDelete(gameInfo, '\n', out data);
            if(!long.TryParse(data, out game.downloadSize.bytes))
            {
                Debug.LogError("Cannot parse \"" + data + "\" ( download size ) to long");
            }

            gameInfo = GetStringToSpecialCharAndDelete(gameInfo, '\n', out data);
            if (!long.TryParse(data, out game.diskSize.bytes))
            {
                Debug.LogError("Cannot parse \"" + data + "\" ( disk size ) to long");
            }


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
                else
                {
                    games[i].installed = false;
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
                Debug.Log("\"" + library + "\"");
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


            RefreshGamesDisplay();
            

           
        }

        private void SetNewIcon(string path)
        {
            if (!GameIcon.Dispatcher.CheckAccess())
            {
                GameIcon.Dispatcher.Invoke(new Action(() => SetNewIcon(path)));
                return;
            }
            //GameIcon.Source = new BitmapImage(new Uri(path));
            SetNewIcon(new BitmapImage(new Uri(path)));
        }

        private void SetNewIcon(BitmapImage image)
        {
            if(!GameIcon.Dispatcher.CheckAccess())
            {
                GameIcon.Dispatcher.Invoke(new Action(() => SetNewIcon(image)));
                return;
            }
            GameIcon.Source = image;
        }

        public void LoadIcon(Int64 gameID)
        {
            Debug.Log("Loading " + gameID.ToString() + " icon");

            string _path = AppDomain.CurrentDomain.BaseDirectory + iconsPath + gameID.ToString() + "_0.png";

            if(!File.Exists(_path))
            {
                iconsDownloader.DownloadIcon(gameID, userInfo.token, _path, IconDownloaded);
                //GameIcon.Source = null;//originalIcon;
                BitmapImage image = null;
                SetNewIcon(image);
                return;
            }

            SetNewIcon(_path);
            
        }

        public void IconDownloaded(DownloadEngine.DownloadStatus status, Int64 gameID, DownloadManager.DownloadTypes type)
        {
            if (status == DownloadEngine.DownloadStatus.success)
            {

                if (actualGameSelected.id == gameID)
                {
                    LoadIcon(gameID);
                }
                return;
            }

            if(status == DownloadEngine.DownloadStatus.appWithoutIcon)
            {
                if(!GameIcon.Dispatcher.CheckAccess())
                {
                    GameIcon.Dispatcher.Invoke(new Action(() => IconDownloaded(status, gameID, type)));
                    return;
                }

                GameIcon.Source = originalIcon;
            }
        }

        /*public void DownloadGamesIcons()
        {
            if(!Directory.Exists(iconsPath))
            {
                Directory.CreateDirectory(iconsPath);
            }

            for(int i = 0;i<games.Count;i++)
            {
                string _iconPath = iconsPath + games[i].id.ToString() + ".png";

                if (File.Exists(_iconPath))
                {
                    continue;
                }

                iconsDownloader.DownloadIcon(games[i].id, userInfo.token, _iconPath, IconDownloaded);
            }
        }*/

        public void ClearDisplayedGames()
        {

            if(GameListTextTemplate.Text != "")
            {
                GameListTextTemplate.MouseLeftButtonDown -= OnGameTextClick;
            }

            GameListTextTemplate.Text = "";
            

            if (GamesSP.Children.Count > gamesSPGamesStartIndex)
            {
                GamesSP.Children.RemoveRange(gamesSPGamesStartIndex, GamesSP.Children.Count);
            }


            
            
        }

        public void RefreshGamesDisplay()
        {

            ClearDisplayedGames();


            SortGamesAlphabetically();

            MarkInstalledGames();

            for (int i = 0; i < games.Count; i++)
            {
                AddGameToTheList(games[i]);
            }



            if (games.Count > 0)
            {
                if (actualGameSelected == null)
                {
                    actualGameSelected = games[0];
                    DisplayGameInfo(games[0]);
                }
                else
                {
                    DisplayGameInfo(actualGameSelected);
                }
            }
            else
            {
                DisplayNoGamesInfo();
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

        public void GameInstalled(DownloadEngine.DownloadStatus status, long id, DownloadManager.DownloadTypes type)
        {
            Debug.LogWarning("Game installation status: " + status);

            if(status != DownloadEngine.DownloadStatus.success)
            {
                return;
            }

            Game game = FindGame(id);
            if(game == null)
            {
                Debug.LogError("Something went desperately wrong, downloaded game not found");
                return;
            }

            UnpackGame(game);

            game.installed = true;

            //RefreshGamesDisplay();
            if (!GameListTextTemplate.Dispatcher.CheckAccess())
            {
                GameListTextTemplate.Dispatcher.Invoke(new Action(RefreshGamesDisplay));
                return;
            }
        }

        public void UnpackGame(Game game)
        {
            if(!Directory.Exists(appPath + game.path))
            {
                Directory.CreateDirectory(appPath + game.path);
            }
            ZipFile.ExtractToDirectory(StoreClient.gamesPath + game.path + game.fileName, appPath + game.path);
        }

        public void InstallGameUserDecided(Game game, InstallAppPopup.UserChoose choose)
        {
            DownloadEngine engine;
            if(choose == InstallAppPopup.UserChoose.install)
            {
                Debug.Log("User token: \"" + userInfo.token + "\"");
                engine = client.DownloadGame(game.id, userInfo.token, GameInstalled);

                /*Thread.Sleep(50);

                //while(engine.downloadedDataSize < game.downloadSize.bytes)
                while(!game.installed)
                {
                    while(client.downloadManager.downloadEngine == null)
                    {
                        Thread.Sleep(10);
                    }
                    Thread.Sleep(50);
                    //Debug.Log("Downloaded " + engine.downloadedDataSize + " bytes");
                    Debug.Log("Downloaded " + (client.downloadManager.downloadEngine.downloadedDataSize * 100f / game.downloadSize.bytes).ToString() + "%");
                }*/
            }

            
        }

        public void InstallGame(Game game)
        {
            //client.DownloadGame(game.id, userInfo.token, GameInstalled);
            InstallAppPopup installAppPopup = new InstallAppPopup(this);
            installAppPopup.DisplayGameInfo(game);
            installAppPopup.userChoseFunction = InstallGameUserDecided;
            


            installAppPopup.ShowDialog();
        }

        public void RunGame(Game game)
        {
            string runPath = AppDomain.CurrentDomain.BaseDirectory + appPath + game.path + game.execName;
            if (File.Exists(runPath))
            {
                Process myProcess = new Process();
                myProcess.StartInfo.FileName = runPath;
                myProcess.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + appPath + game.path;
                myProcess.Start();
            }
            else
            {
                Debug.LogError("Error running " + game.name + ", missing executable");
            }
        }

        public void InstallOrRunGame(Game game)
        {
            if (game.installed)
            {
                RunGame(game);
            }
            else
            {
                InstallGame(game);
            }
        }


        public void UninstallGame(Game game)
        {

            ConfirmationWindow confirm = new ConfirmationWindow();
            confirm.ShowDialog();

            if (confirm.value)
            {

                string _path = appPath + game.path;

                if (Directory.Exists(_path))
                {


                    Directory.Delete(_path, true);
                }

                RefreshGamesDisplay();
            }

        }





        // Buttons clicks

        public void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new Menu().Content);
        }


        /// <summary>
        /// Run / install button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            InstallOrRunGame(actualGameSelected);
        }

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            UninstallGame(actualGameSelected);
        }
    }
}
