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
    /// Logika interakcji dla klasy InstallAppPopup.xaml
    /// </summary>
    public partial class InstallAppPopup : Window
    {
        private Library library;

        private Library.Game game;

        public enum UserChoose
        {
            unknown,
            install,
            cancel,
        }

        public UserChoose userChoose = UserChoose.unknown;

        public Action<Library.Game, UserChoose> userChoseFunction;

        public void DisplayGameInfo(Library.Game _game)
        {
            game = _game;

            if (game == null)
            {
                Debug.LogError("Game reference is null, quitting");
                return;
            }

            GameName.Text = game.name;
            diskSizeText.Text = game.diskSize.ToStringWithPrecision(1);
            downloadSizeText.Text = game.downloadSize.ToStringWithPrecision(1);
        }

        public InstallAppPopup(Library _library)
        {
            InitializeComponent();

            library = _library;
        }

        private void UserDecided(UserChoose choose)
        {
            userChoose = choose;

            if(userChoseFunction != null)
            {
                userChoseFunction.Invoke(game, choose);
            }

            this.Close();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            UserDecided(UserChoose.install);

            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            UserDecided(UserChoose.cancel);
        }
    }
}
