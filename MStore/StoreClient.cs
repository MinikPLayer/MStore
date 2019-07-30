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

        string ipAddress = "127.0.0.1";
        int port = 15332;

        public StoreClient()
        {
            SetRelativePaths();

            LoadConfig();

            //Debugging purposes
            //Debug.LogWarning("Debugging enabled in StoreClient()!");
            //TestDownloadEngine test = new TestDownloadEngine(ipAddress, 5592);
            //return;


            socket = new ClientEngine(ipAddress, port);
            //socket = new ClientEngine("mserver.ml", 15332);


            /*socket.Receive(-1);
            socket.Send("Minik");

            Thread th = socket.Receive(1);
            th.Join();

            socket.Send("NiePodamCiHasla");*/

            //TestStartCommunication();

            Thread.Sleep(500);

            WaitForWelcomePacket();
        }

        private void WaitForWelcomePacket()
        {
            
            while (!connected)
            {
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
            }
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
            socket._Send(username);

            string receive = socket.WaitForReceive(10);

            if (receive != "N")
            {
                Debug.LogError("Login error: " + receive);
                return receive;
            }

            socket._Send(password);

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
