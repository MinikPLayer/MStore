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

            double percentage = dwManager.downloadEngine.downloadedDataSize * 100f / game.downloadSize.bytes;

            acutallyDownloadingProgressBar.Value = percentage;

            actuallyDownloadingPercentage.Text = ((int)percentage).ToString() + "%";
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
                    return;
                }

                SetValues();
                SetQueueTexts();
                Thread.Sleep(100);
            }
        }

        public DownloadMenu()
        {
            InitializeComponent();

            gameTextBlockPrefab = XamlWriter.Save(GameListTextTemplate);

            dwManager = Library.client.downloadManager;

            if(dwManager == null || dwManager.queue.Count == 0)
            {
                Debug.Log("Nothing to show here");
                //PageManager.SetPage(new EmptyDownloadMenu().Content);
                SetEmptyMenu();
                return;
            }


            //SetValues();
            if(updatePercentageThread != null)
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
