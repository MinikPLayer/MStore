using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using System.IO;

namespace MStore
{
    public class StoreClient
    {
        private ClientEngine socket;
        public bool connected = false;

        public static string gamesPath = "./downloadFiles/";

        public DownloadManager downloadManager;


        private string DeleteSpaces(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    line = line.Remove(i, 1);
                    i--;
                }
            }

            return line;
        }

        private void CreateNewConfig(string path)
        {
            const string defaultConfig = @"ip = mserver.ml
port = 15332";

            if(!Directory.Exists(permamentPath))
            {
                Directory.CreateDirectory(permamentPath);
            }

            File.WriteAllText(path, defaultConfig);
        }

        private void ParseConfigLine(string line)
        {
            line = DeleteSpaces(line);

            if(line.ToLower().StartsWith("ip="))
            {
                line = line.Remove(0, "ip=".Length);

                ipAddress = line;

                Debug.Log("New IP address: " + ipAddress);

               
                return;
            }
            else if(line.ToLower().StartsWith("port="))
            {
                line = line.Remove(0, "port=".Length);

                int _port = 0;

                if(int.TryParse(line, out _port))
                {
                    port = _port;

                    Debug.Log("New port: " + port);
                }
                else
                {
                    Debug.LogError("Invalid port format, cannot parse " + line + " to port ( int )");
                }
            }
            else if(line.ToLower().StartsWith("downloadport="))
            {
                line = line.Remove(0, "downloadport=".Length);

                int _downloadPort = 0;

                if(int.TryParse(line, out _downloadPort))
                {
                    downloadEnginePort = _downloadPort;

                    Debug.Log("New download engine port: " + downloadEnginePort);
                }
                else
                {
                    Debug.LogError("Invalid port format, cannot parse " + line + " to download port ( int )");
                }
            }
            else if(line.ToLower().StartsWith("iconsdownloadport=") || line.ToLower().StartsWith("downloadiconsport="))
            {
                line = line.Remove(0, "iconsdownloadport=".Length);

                int _downloadPort = 0;

                if (int.TryParse(line, out _downloadPort))
                {
                    downloadIconsPort = _downloadPort;

                    Debug.Log("New icons download port: " + downloadIconsPort);
                }
                else
                {
                    Debug.LogError("Invalid port format, cannot parse " + line + " to icons download port ( int )");
                }
            }
        }

        public static string GetPath(string relativePath)
        {
            return System.IO.Path.Combine(permamentPath, relativePath);
        }

        static string permamentPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MStore");

        static string configDir = "config.ini";
        private void LoadConfig()
        {
            if(!File.Exists(configDir))
            {
                Debug.LogError("File " + configDir + " doesn't exist, creating one");
                CreateNewConfig(configDir);
                //return;
            }

            string[] line = File.ReadAllLines(configDir);

            for(int i = 0;i<line.Length;i++)
            {
                ParseConfigLine(line[i]);
            }
        }

        private void SetRelativePaths()
        {
            configDir = GetPath(configDir);
        }

        private void CreateDirectories()
        {
            if(!Directory.Exists(gamesPath))
            {
                Directory.CreateDirectory(gamesPath);
            }
        }

        public string ipAddress = "127.0.0.1";
        public int port = 15332;
        public int downloadEnginePort = 5592;
        public int downloadIconsPort = 5593;

        public StoreClient()
        {
            SetRelativePaths();

            CreateDirectories();

            LoadConfig();

            //Debugging purposes
            //Debug.LogWarning("Debugging enabled in StoreClient()!");
            //TestDownloadEngine test = new TestDownloadEngine(ipAddress, 5592);
            //return;


            Debug.Log("Connecting to server...");
            socket = new ClientEngine(ipAddress, port);
            //socket = new ClientEngine("mserver.ml", 15332);


            /*socket.Receive(-1);
            socket.Send("Minik");

            Thread th = socket.Receive(1);
            th.Join();

            socket.Send("NiePodamCiHasla");*/

            //TestStartCommunication();
            



            //Check if connected
            for(int i = 0;i<50;i++)
            {
                if(socket.connected)
                {
                    break;
                }
                if(socket.connectionFailed)
                {
                    break;
                }
                
                Thread.Sleep(5000 / 50);
            }
            if(!socket.connected)
            {
                Debug.FatalError("Cannot connect to server", 14, 1000);
                return;
            }

            WaitForWelcomePacket();
        }

        private void WaitForWelcomePacket()
        {

            while (!connected)
            {
                //string packet = socket.WaitForReceive();
                string packet = socket.WaitForReceive();

                Debug.Log("Welcome Packet: " + packet);

                if (packet == "WP")
                {
                    connected = true;
                }
                else
                {
                    Debug.Log("Error connecting");
                    connected = false;
                    //return;
                    Thread.Sleep(100);
                }
            }
        }

        public enum Commands
        {
            login,
            register,
            requestLibrary,
            gameInfo,
            userInfo,
            storeGamesList,
            buyGame,
            sendVoucher,
        }

        private void SendCommand(Commands command, string parameters = "")
        {
            switch(command)
            {
                case Commands.login:
                    socket._Send("LOGIN" + parameters);
                    break;
                case Commands.register:
                    socket._Send("REGIS" + parameters);
                    break;
                case Commands.requestLibrary:
                    socket._Send("RQLBR" + parameters);
                    break;
                case Commands.gameInfo:
                    socket._Send("GMNFO" + parameters);
                    break;
                case Commands.userInfo:
                    socket._Send("URNFO" + parameters);
                    break;
                case Commands.storeGamesList:
                    socket._Send("SGLST" + parameters);
                    break;
                case Commands.buyGame:
                    socket._Send("BGAME" + parameters);
                    break;
                case Commands.sendVoucher:
                    socket._Send("VCHER" + parameters);
                    break;
            }
        }

        public static ClientEngine createNewClient(string _ip, int _port)
        {
            ClientEngine returnClient = new ClientEngine(_ip, _port);







            //Check if connected
            for (int i = 0; i < 50; i++)
            {
                if (returnClient.connected)
                {
                    break;
                }
                if (returnClient.connectionFailed)
                {
                    break;
                }

                Thread.Sleep(5000 / 50);
            }
            if (!returnClient.connected)
            {
                Debug.FatalError("Cannot connect to server", 14, 1000);
                return null;
            }

            while(true)
            {
                string data = returnClient.WaitForReceive();
                if(data == "WP")
                {
                    Debug.LogWarning("Got welcome packet");
                    break;
                }
                else
                {
                    Debug.LogError("Wrong welcome packet: " + data);
                }
            }
            

            return returnClient;
        }

        public DownloadEngine DownloadGame(Int64 id, string token, Action<DownloadEngine.DownloadStatus, long, DownloadManager.DownloadTypes> downloadCompleteFunction = null)
        {
            //DownloadEngine downloadEngine = new DownloadEngine(createNewClient(ipAddress, downloadEnginePort), token);
            /*if(downloadEngine == null)
            {
                downloadEngine = new DownloadEngine(createNewClient(ipAddress, downloadEnginePort), token);
            }*/

            if(downloadManager == null)
            {
                downloadManager = new DownloadManager(ipAddress, downloadEnginePort);
            }

            int empty = -1;
            Library.Game game = Library.FindGame(id, out empty);
            if(game == null)
            {
                Debug.LogError("Game with id " + id + " not found!!!");
                return null;
            }
            Debug.Log("Downloading game...");
            if(!Directory.Exists(gamesPath + game.path))
            {
                Directory.CreateDirectory(gamesPath + game.path);
            }
            //downloadEngine.DownloadGame(id, gamesPath + game.path + game.fileName, downloadCompleteFunction);

            return downloadManager.DownloadGame(id, token, gamesPath + game.path + game.fileName, downloadCompleteFunction);
        }

        public string SendVoucher(string code)
        {
            SendCommand(Commands.sendVoucher, code);

            return socket.WaitForReceive();
        }

        public string RequestBuyGame(Int64 id)
        {
            SendCommand(Commands.buyGame, id.ToString());

            return socket.WaitForReceive();
        }

        public string RequestStoreGamesList(int minimumPrice = -1, int maximumPrice = -1, string nameFilter = "")
        {
            string parameters = "";
            if(minimumPrice > 0)
            {
                parameters += "m" + minimumPrice.ToString() + "\n";
            }
            if(maximumPrice > 0)
            {
                parameters += "M" + maximumPrice.ToString() + "\n";
            }
            if(nameFilter.Length > 0)
            {
                parameters += "S" + nameFilter + "\n";
            }



            SendCommand(Commands.storeGamesList, parameters);

            return socket.WaitForReceive();
        }

        public string RequestUserInfo()
        {
            SendCommand(Commands.userInfo);

            return socket.WaitForReceive();
        }

        public string RequestGameInfo(Int64 id)
        {
            SendCommand(Commands.gameInfo, id.ToString());

            return socket.WaitForReceive();
        }

        public string RecquestLibrary()
        {
            
            SendCommand(Commands.requestLibrary);

            return socket.WaitForReceive();
        }

        public string SendUserCredentials(string username, string password)
        {
            Debug.Log("Sending username");
            socket._Send(username);

            Debug.Log("Waiting for \"N\" from server");
            string receive = socket.WaitForReceive(10);

            if (receive != "N")
            {
                Debug.LogError("Login error: " + receive);
                return receive;
            }

            Debug.Log("Sending password");
            socket._Send(password);



            Debug.Log("Waiting for \"N\" from server");
            receive = socket.WaitForReceive(10);
            return receive;
        }

        public string Login(string username, string password)
        {
            if (!connected)
            {
                Debug.LogError("Not connected to server");
                return "Not connected to server";
            }

            SendCommand(Commands.login);

            return SendUserCredentials(username, password);


        }

        public string Register(string username, string password)
        {
            if (!connected)
            {
                Debug.LogError("Not connected to server");
                return "Not connected to server";
            }

            SendCommand(Commands.register);

            return SendUserCredentials(username, password);
        }

        public void TestStartCommunication()
        {
            string welcomePacket = socket.WaitForReceive();

            string response = "";
            while (response != "LS:OK")
            {
                SendCommand(Commands.login);

                socket._Send("Minik");

                response = socket.WaitForReceive();


                socket._Send("pass");

                response = socket.WaitForReceive();

                if (response == "LS:NR")
                {
                    Debug.LogError("User not found");
                    continue;
                }

                Debug.Log("Status: " + response);
            }

            

            while (true) ;
        }
    }
}
