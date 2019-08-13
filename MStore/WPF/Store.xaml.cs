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

using System.Text.RegularExpressions;

namespace MStore
{
    /// <summary>
    /// Logika interakcji dla klasy Library.xaml
    /// </summary>
    public partial class Store : Window
    {
        public static StoreClient client;

        int clickedTextBlockIndex = -1;

        string gameTextBlockPrefab = "";

        private static List<Game> games = new List<Game>();

        private static Game actualGameSelected;

        private int gamesSPGamesStartIndex = -1;

        private ImageSource originalIcon;

        public static int minPriceFilter = -1;
        public static int maxPriceFilter = -1;

        public static string nameFilter = "";

        public class Game
        {
            public string name;
            public long id;

            public string price = "";

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
            BuyButton.Visibility = Visibility.Hidden;

            GameIcon.Visibility = Visibility.Hidden;

            PriceText.Visibility = Visibility.Hidden;

            GameTitle.Text = "No apps found";
        }

        private void DisplayGameInfo(Game game)
        {
            //Debug.LogWarning("DisplayGameInfo(Game game) - Not implemented");

            LoadIcon(game.id);

            GameTitle.Text = game.name;

            BuyButton.Visibility = Visibility.Visible;

            //Disable if icon is not downloaded
            GameIcon.Visibility = Visibility.Visible;

            PriceText.Visibility = Visibility.Visible;

            PriceText.Text = game.price;
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
                Debug.Log("Changing list text exmaple to " + game.name);
                GameListTextTemplate.Text = game.name;
                GameListTextTemplate.MouseLeftButtonDown += OnGameTextClick;

                return;
            }

            StringReader reader = new StringReader(gameTextBlockPrefab);
            XmlReader xmlReader = XmlReader.Create(reader);
            TextBlock newButton = (TextBlock)XamlReader.Load(xmlReader);

            newButton.Text = game.name;

            newButton.MouseLeftButtonDown += OnGameTextClick;

            GamesSP.Children.Add(newButton);
        }

        public Store()
        {
            InitializeComponent();

            bool firstRun = false;
            if(client == null)
            {
                firstRun = true;
            }

            client = App.storeClient;

            originalIcon = GameIcon.Source;

            gameTextBlockPrefab = XamlWriter.Save(GameListTextTemplate);

            //string libraryStr = client.RecquestLibrary();

            //TextBlock newButton = new TextBlock();
            //newButton.Text = "Fortnite";
            //newButton.MouseLeftButtonDown += OnGameTextClick;

            NavBar.SetMode(Controls.NavBar.Modes.Store);

            gamesSPGamesStartIndex = GamesSP.Children.Count;

            GetGamesList(firstRun);

            CoinsNumberText.Text = Library.userInfo.coins.ToString();


            if(minPriceFilter >= 0)
            {
                MinimumPriceFilterBox.Text = minPriceFilter.ToString();
            }
            if(maxPriceFilter >= 0)
            {
                MaximumPriceFilterBox.Text = maxPriceFilter.ToString();
            }

            //client.DownloadGame(0, userInfo.token);

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

            if (str.Length > strToSpecialChar.Length)
            {
                return str.Remove(0, strToSpecialChar.Length + 1);
            }
            else
            {
                return "";
            }
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


            return game;
        }

        public void GetGamesList(bool forceRefresh = false)
        {

            if (games.Count == 0 || forceRefresh)
            {

                string gamesList = client.RequestStoreGamesList(minPriceFilter, maxPriceFilter, nameFilter);
                List<string> _games = new List<string>();

                games = new List<Game>();

                if(gamesList[0] == '\0' )
                {
                    RefreshGamesDisplay();
                    return;
                }

                if(gamesList == "NA")
                {
                    Debug.LogError("User not authorised");
                    return;
                }
                
                while(gamesList.Length != 0)
                {
                    string line = "";
                    gamesList = GetStringToSpecialCharAndDelete(gamesList, '\r', out line);
                    _games.Add(line);
                }

                Debug.Log("Got " + _games.Count + " games from store");

                for(int i = 0;i< _games.Count;i++)
                {
                    Game game = ParseStringToGame(_games[i]);
                    Debug.Log("Game id: " + game.id + " with name: " + game.name);
                    if(game != null)
                    {
                        //AddGameToTheList(game);
                        games.Add(game);
                    }
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

            string _path = AppDomain.CurrentDomain.BaseDirectory + Library.iconsPath + gameID.ToString() + "_0.png";

            if(!File.Exists(_path))
            {
                Library.iconsDownloader.DownloadIcon(gameID, Library.userInfo.token, _path, IconDownloaded);
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






        // Buttons clicks

        public void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.SetPage(new Menu().Content);
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationWindow confirmation = new ConfirmationWindow();
            confirmation.ShowDialog();

            if(confirmation.value == false)
            {
                return;
            }

            string response = client.RequestBuyGame(actualGameSelected.id);
            if(response == "OK")
            {
                Debug.Log("Game bought!");

                App.library.NewGameBought(actualGameSelected.id);

                CoinsNumberText.Text = Library.userInfo.coins.ToString();

                GetGamesList(true);


                RefreshGamesDisplay();


                if (games.Count > 0)
                {
                    actualGameSelected = games[0];
                    DisplayGameInfo(actualGameSelected);
                }
                else
                {
                    DisplayNoGamesInfo();
                }
            }
            else
            {
                //Debug.Log("Something went wrong, response: " + response);
                if(response == "TP")
                {
                    MessageBox.Show("Not enough coins to buy this game", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if(response == "NA")
                {
                    MessageBox.Show("Session expired, try restarting app", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if(response == "NF")
                {
                    MessageBox.Show("Game not found on server, are You doing somethig sketchy???", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if(response == "AB")
                {
                    MessageBox.Show("Are you trying to buy this game again? NO! You won't spend money on something You have", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UserVoucherButton_Click(object sender, RoutedEventArgs e)
        {
            VoucherWindow voucher = new VoucherWindow();
            voucher.ShowDialog();

            if(voucher.voucherCode.Length > 0)
            {
                string response = client.SendVoucher(voucher.voucherCode);

                switch (response)
                {
                    case "OK":
                        MessageBox.Show("Voucher successfully used", "Voucher", MessageBoxButton.OK, MessageBoxImage.Information);
                        App.library.GetUserInfo(true);
                        App.library.GetGamesList(true);

                        CoinsNumberText.Text = Library.userInfo.coins.ToString();

                        GetGamesList(true);
                        

                        RefreshGamesDisplay();

                        if(games.Count > 0)
                        {
                            actualGameSelected = games[0];
                            DisplayGameInfo(actualGameSelected);
                        }
                        else
                        {
                            DisplayNoGamesInfo();
                        }
                        break;

                    case "NA":
                        MessageBox.Show("User not authorised", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;

                    case "BA":
                        MessageBox.Show("Bad code", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;

                    case "BV":
                        MessageBox.Show("Bad Voucher", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;

                    case "NF":
                        MessageBox.Show("Voucher game not found on server", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;

                    default:
                        MessageBox.Show("Unknown error", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private void MinimumPriceFilterBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            GetGamesList(true);



            actualGameSelected = null;

            RefreshGamesDisplay();
        }

        private void MinimumPriceFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MinimumPriceFilterBox.Text.Length == 0)
            {
                minPriceFilter = -1;
            }
            else
            {
                minPriceFilter = int.Parse(MinimumPriceFilterBox.Text);

            }
        }

        private void MaximumPriceFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(MaximumPriceFilterBox.Text.Length == 0)
            {
                maxPriceFilter = -1;
            }
            else
            {
                maxPriceFilter = int.Parse(MaximumPriceFilterBox.Text);
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            nameFilter = SearchBar.Text;
        }

        private void SearchBar_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                FilterButton_Click(sender, null);
            }
        }
    }
}
