using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Threading;

namespace MStore
{

    

    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static StoreClient storeClient;
        public static Library library;
        private Thread storeClientThread;

        private Thread appThread;

        public static event EventHandler<String> DisplayLogOnWindow = delegate { };
        

        private void StartStoreClientEngine()
        {
            

        }

        public void Log(object text, bool newLine = true)
        {
            string additionalChar = "";
            if (newLine) additionalChar = "\n";
            Dispatcher.BeginInvoke(new Action(() => DisplayLogOnWindow(this, text.ToString() + additionalChar)));
        }

        public void AppHandler()
        {

        }

        public App()
        {
            storeClient = new StoreClient();

            storeClientThread = new Thread(StartStoreClientEngine);
            storeClientThread.Start();

            appThread = new Thread(AppHandler);
            appThread.Start();
        }
    }
}
