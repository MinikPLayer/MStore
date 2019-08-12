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

using System.Threading;

using System.Windows.Threading;

using System.IO;

using System.Windows.Markup;

using System.Xml;

namespace MStore
{
    /// <summary>
    /// Logika interakcji dla klasy DownloadMenu.xaml
    /// </summary>
    public partial class DownloadMenu : Window
    {
        private static Thread updatePercentageThread;

        private DownloadManager dwManager;

        private static bool stopThread = false;

        string gameTextBlockPrefab = "";

        private long lastGameID = -1;

        public const int measureInterval = 100;
        public Library.Game.Size[] measuredSizes = new Library.Game.Size[10];
        private int measuredSizesIndex = 0;
        private void SetQueueTexts()
        {
            if(!downloadQueueView.Dispatcher.CheckAccess())
            {
                downloadQueueView.Dispatcher.BeginInvoke(new Action(SetQueueTexts));
                return;
            }

            downloadQueueView.Children.RemoveRange(1, downloadQueueView.Children.Count);

            List<DownloadManager.QueueEntry> entries = dwManager.queue;

            for(int i = 1;i<entries.Count;i++)
            {
                Library.Game game = Library.FindGame(entries[i].id);
                if (game == null)
                {
                    Debug.LogError("Something went desperately wrong, cannot find game with id " + dwManager.queue[0].id);
                    return;
                }

                StringReader reader = new StringReader(gameTextBlockPrefab);
                XmlReader xmlReader = XmlReader.Create(reader);
                TextBlock newButton = (TextBlock)XamlReader.Load(xmlReader);

                newButton.Text = game.name;


                downloadQueueView.Children.Add(newButton);
            }
        }

        private void SetValues()
        {
            Library.Game game = Library.FindGame(dwManager.queue[0].id);
            if(game == null)
            {
                Debug.LogError("Something went desperately wrong, cannot find game with id " + dwManager.queue[0].id);
                return;
            }

            if(!actuallyDownloadingName.Dispatcher.CheckAccess())
            {
                actuallyDownloadingName.Dispatcher.Invoke(new Action(SetValues));
                return;
            }

            actuallyDownloadingName.Text = game.name;

            if(dwManager.downloadEngine == null)
            {
                Debug.LogError("Download engine is null");
                Thread.Sleep(1000);
                return;
            }

            double percentage = dwManager.downloadEngine.downloadedDataSize * 100f / game.downloadSize.bytes;

            acutallyDownloadingProgressBar.Value = percentage;

            actuallyDownloadingPercentage.Text = ((int)percentage).ToString() + "%";


            //Speed measure
            measuredSizes[measuredSizesIndex].bytes = dwManager.downloadEngine.downloadedDataSize;
            measuredSizesIndex++;
            if(measuredSizesIndex == measuredSizes.Length)
            {
                measuredSizesIndex = 0;
                long sum = 0;
                for(int i = 1;i<measuredSizes.Length;i++)
                {
                    sum += measuredSizes[i].bytes - measuredSizes[i - 1].bytes;
                }

                long averge = (long)(sum / (float)(measuredSizes.Length - 1) * 1000f/measureInterval);

                Library.Game.Size newSize = new Library.Game.Size(averge);

                actuallyDownloadingSpeed.Text = newSize.ToStringWithPrecision(2) + "/s";
            }


            if(lastGameID != game.id)
            {
                string iconPath = AppDomain.CurrentDomain.BaseDirectory + Library.iconsPath + game.id.ToString() + "_1.png";

                if (File.Exists(iconPath))
                {
                    actuallyDownloadingIcon.Source = new BitmapImage(new Uri(iconPath));
                    lastGameID = game.id;
                }
                else
                {
                    Library.iconsDownloader.DownloadIcon(game.id, Library.userInfo.token, iconPath, IconDownloaded, false, true);
                    lastGameID = game.id;
                }
                
            }
        }

        private void SetIcon(string path)
        {
            if(!actuallyDownloadingIcon.Dispatcher.CheckAccess())
            {
                actuallyDownloadingIcon.Dispatcher.Invoke(new Action(() => SetIcon(path)));
                return;
            }

            SetIcon(new BitmapImage(new Uri(path)));
        }

        private void SetIcon(BitmapImage image)
        {
            if (!actuallyDownloadingIcon.Dispatcher.CheckAccess())
            {
                actuallyDownloadingIcon.Dispatcher.Invoke(new Action(() => SetIcon(image)));
                return;
            }

            actuallyDownloadingIcon.Source = image;
        }

        private void IconDownloaded(DownloadEngine.DownloadStatus status, Int64 gameID, DownloadManager.DownloadTypes type)
        {
            if(status == DownloadEngine.DownloadStatus.success)
            {
                SetIcon(AppDomain.CurrentDomain.BaseDirectory + Library.iconsPath + gameID.ToString() + "_1.png");
            }
        }

        private void SetEmptyMenu()
        {
            this.Content = new EmptyDownloadMenu().Content;
            //PageManager.SetPage(new EmptyDownloadMenu().Content);
        }

        private void UpdatePercentageThread()
        {
            while(!stopThread)
            {
                while(dwManager.queue.Count == 0)
                {
                    //this.Content = new EmptyDownloadMenu().Content;
                    actuallyDownloadingName.Dispatcher.BeginInvoke(new Action(SetEmptyMenu));
                    measuredSizes = new Library.Game.Size[10];
                    measuredSizesIndex = 0;

                    lastGameID = -1;
                    return;
                }

                SetValues();
                SetQueueTexts();
                Thread.Sleep(measureInterval);
            }
        }

        public DownloadMenu()
        {
            InitializeComponent();

            gameTextBlockPrefab = XamlWriter.Save(GameListTextTemplate);

            dwManager = Library.client.downloadManager;

            NavBar.SetMode(Controls.NavBar.Modes.Download);

            if(dwManager == null || dwManager.queue.Count == 0)
            {
                Debug.Log("Nothing to show here");
                //PageManager.SetPage(new EmptyDownloadMenu().Content);
                SetEmptyMenu();
                return;
            }



            //SetValues();
            if (updatePercentageThread != null)
            {
                if(updatePercentageThread.IsAlive)
                {
                    stopThread = true;
                    updatePercentageThread.Join();
                    stopThread = false;
                }
            }
            updatePercentageThread = new Thread(UpdatePercentageThread);
            updatePercentageThread.Start();
        }
    }
}
