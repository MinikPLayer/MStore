using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using System.IO;

using System.Diagnostics;

namespace MStore
{
    public class DownloadEngine
    {
        ClientEngine client;

        Thread workingThread;

        FileStream stream;

        /// <summary>
        /// Tells if something is uploading / downloading
        /// </summary>
        public bool working { get; private set; } = false;

        //public delegate void DownloadStatusFunction(DownloadStatus status);
        //public DownloadStatusFunction downloadCompleteFunction;
        public Action<DownloadStatus, long, DownloadManager.DownloadTypes> downloadCompleteFunction;

        public DownloadManager.DownloadTypes type = DownloadManager.DownloadTypes.none;


        /// <summary>
        /// Blocking function to receive data
        /// </summary>
        /// <returns></returns>
        public string ReceiveData()
        {
            return client.WaitForReceive();
        }



        public byte[] ReceiveDataRaw()
        {
            client.dataRawReceived = SaveToFile;

            return client.WaitForReceiveRaw(5f, false, false);
        }

        private bool AuthenticateClient(string token)
        {
            client.Send("TOKEN" + token);

            string data = client.WaitForReceive();
            switch (data)
            {
                case "OK":
                    return true;
                    
                default:
                    if(data.Length != 0)
                    {
                        Debug.LogError("Authentication failed: " + data);
                        return false;
                    }
                    break;
            }

            Debug.LogWarning("Sending ok");
            client.Send("OK");

            return false;

        }

        public int downloadedDataSize { get; private set; } = 0;
        private void SaveToFile(byte[] data, Int32 length)
        {
            //Debug.Log("Received");
            //Debug.Log("Writing data with length: " + length);
            downloadedDataSize += length;
            stream.Write(data, 0, length);


            //Debug.Log("Debug slowing downloading");
        }

        public enum DownloadStatus
        {
            unknown,
            success,
            fileNotFoundOnServer,
            cannotOpenFile,
            unfinished,
            connectionLost,       
            notAuthorised,
            appWithoutIcon
        }

        private enum RequestStatus
        {
            unknown,
            success,
            fileNotFound,
            connectionLost,
            notAuthorised,
            appWithoutIcon
        }

        private RequestStatus RequestGameFromServer(Int64 id)
        {
            client._Send("DWNLD" + id.ToString());
            string response = client.WaitForReceive();
            if(response == null || response.Length == 0)
            {
                return RequestStatus.connectionLost;
            }
            Debug.Log("Server response to download: " + response);
            switch (response)
            {
                case "OK":
                    return RequestStatus.success;
                case "NF":
                    return RequestStatus.fileNotFound;
                case "NA":
                    return RequestStatus.notAuthorised;
                default:
                    return RequestStatus.unknown;
            }
        }

        private void FinishDownloading(DownloadStatus status, Int64 id)
        {
            if (downloadCompleteFunction != null)
            {
                //downloadCompleteFunction.Invoke(status);
                downloadCompleteFunction(status, id, type);
                Debug.LogWarning("Invoking downloadcompleteFunction");
            }
            else
            {
                Debug.LogWarning("DownloadCompleteFunction is null");
            }

            working = false;
        }

        private void _DownloadGame(Int64 id, string outputFile)
        {
            

            RequestStatus requestStatus = RequestGameFromServer(id);
            switch (requestStatus)
            {
                case RequestStatus.unknown:
                    Debug.LogError("Unknown download error");
                    FinishDownloading(DownloadStatus.unknown, id);
                    return;

                case RequestStatus.success:
                    break;
                        
                case RequestStatus.fileNotFound:
                    Debug.LogError("Game not found on server");
                    FinishDownloading(DownloadStatus.fileNotFoundOnServer, id);
                    return;

                case RequestStatus.connectionLost:
                    Debug.LogError("Connection lost");
                    FinishDownloading(DownloadStatus.connectionLost, id);
                    return;

                case RequestStatus.notAuthorised:
                    Debug.LogError("Not authorised to download");
                    FinishDownloading(DownloadStatus.notAuthorised, id);
                    return;

                default:
                    Debug.LogError("Unknown download error");
                    FinishDownloading(DownloadStatus.unknown, id);
                    return;
            }

            

            //Create file
            stream = File.Create(outputFile);

            //string testReceive = client.WaitForReceive();
            //Debug.Log("Test receive: " + testReceive);

            //Download
            ReceiveDataRaw();

            stream.Close();
            Debug.Log("Closed stream");
            Debug.Log("Downloaded " + downloadedDataSize + " bytes");

            DownloadStatus status = DownloadStatus.success;


            

            FinishDownloading(status, id);
            return;
        }

        /*private void _DownloadGames(Int64[] gamesIDs, string[] outputFiles)
        {
            for (int i = 0; i < outputFiles.Length; i++)
            {
                _DownloadGame(gamesIDs[i], outputFiles[i]);
            }

            
        }

        public bool DownloadGames(Int64[] gamesIDs, string[] outputFiles)
        {
            if (working)
            {
                Debug.LogWarning("Download engine is downloading, cannot start new download");
                return false;
            }

            if (gamesIDs.Length != outputFiles.Length)
            {
                Debug.LogError("DownloadFilesNames and outputFilesPaths are not the same size, returning");
                return false;
            }

            working = true;
            workingThread = new Thread(() => _DownloadGames(gamesIDs, outputFiles));
            workingThread.Start();


            return true;
        }*/

        /// <summary>
        /// Downloads file from server
        /// </summary>
        /// <param name="id">Game ID to download</param>
        /// <param name="outputFilePath">Downloaded file path</param>
        /// <returns></returns>
        public bool DownloadGame(Int64 id, string outputFile, Action<DownloadStatus, long, DownloadManager.DownloadTypes> _downloadCompleteFunction = null)
        {
            if(working)
            {
                Debug.LogWarning("Download engine is downloading, cannot start new download");
                return false;
            }

            type = DownloadManager.DownloadTypes.game;

            downloadCompleteFunction = _downloadCompleteFunction;

            working = true;

            workingThread = new Thread(() => _DownloadGame(id, outputFile));
            workingThread.Start();

            return true;
        }

        private RequestStatus RequestIconFromServer(Int64 id, bool highRes)
        {
            string resLetter = "L";
            if(highRes)
            {
                resLetter = "H";
            }

            client._Send("GICON" + resLetter + id.ToString());
            string response = client.WaitForReceive();
            if (response == null || response.Length == 0)
            {
                return RequestStatus.connectionLost;
            }
            Debug.Log("Server response to download: " + response);
            switch (response)
            {
                case "OK":
                    return RequestStatus.success;
                case "NF":
                    return RequestStatus.fileNotFound;
                case "NA":
                    return RequestStatus.notAuthorised;
                case "NI":
                    return RequestStatus.appWithoutIcon;
                default:
                    return RequestStatus.unknown;
            }
        }

        private void _DownloadIcon(Int64 gameID, string outputFile, bool highRes)
        {
            RequestStatus requestStatus = RequestIconFromServer(gameID, highRes);
            switch (requestStatus)
            {
                case RequestStatus.unknown:
                    Debug.LogError("Unknown download error");
                    FinishDownloading(DownloadStatus.unknown, gameID);
                    return;

                case RequestStatus.success:
                    break;

                case RequestStatus.fileNotFound:
                    Debug.LogError("Game not found on server");
                    FinishDownloading(DownloadStatus.fileNotFoundOnServer, gameID);
                    return;

                case RequestStatus.connectionLost:
                    Debug.LogError("Connection lost");
                    FinishDownloading(DownloadStatus.connectionLost, gameID);
                    return;

                case RequestStatus.notAuthorised:
                    Debug.LogError("Not authorised to download");
                    FinishDownloading(DownloadStatus.notAuthorised, gameID);
                    return;

                case RequestStatus.appWithoutIcon:
                    Debug.LogWarning("App without icon");
                    FinishDownloading(DownloadStatus.appWithoutIcon, gameID);
                    return;

                default:
                    Debug.LogError("Unknown download error");
                    FinishDownloading(DownloadStatus.unknown, gameID);
                    return;
            }

            //Create file
            stream = File.Create(outputFile);

            //string testReceive = client.WaitForReceive();
            //Debug.Log("Test receive: " + testReceive);

            //Download
            ReceiveDataRaw();

            stream.Close();
            Debug.Log("Closed stream");
            Debug.Log("Downloaded " + downloadedDataSize + " bytes");

            DownloadStatus status = DownloadStatus.success;




            FinishDownloading(status, gameID);
            return;
        }

        public bool DownloadIcon(Int64 gameID, string outputFile, Action<DownloadStatus, long, DownloadManager.DownloadTypes> _downloadCompleteFunction = null, bool highResImage = false)
        {
            if (working)
            {
                Debug.LogWarning("Download engine is downloading, cannot start new download");
                return false;
            }

            type = DownloadManager.DownloadTypes.icon;

            downloadCompleteFunction = _downloadCompleteFunction;

            working = true;

            workingThread = new Thread(() => _DownloadIcon(gameID, outputFile, highResImage));
            workingThread.Start();

            return true;
        }

        public DownloadEngine(ClientEngine _client, string token)
        {
            client = _client;

            bool result = AuthenticateClient(token);
            Debug.Log("Authentication result: " + result);
        }
    }

    public class DownloadManager
    {
        public enum DownloadTypes
        {
            none,
            game,
            icon
        }

        public class QueueEntry
        {
            public long id = -1;
            public string path = "";
            public string token = "";
            public Action<DownloadEngine.DownloadStatus, long, DownloadTypes> completeFunction;
            public DownloadTypes type;

            public bool highResImage;

            public QueueEntry(long _id, string _path, string _token, Action<DownloadEngine.DownloadStatus, long, DownloadTypes> _completeFunction, DownloadTypes _type, bool _highResImage = false)
            {
                id = _id;
                path = _path;
                token = _token;
                completeFunction = _completeFunction;

                type = _type;

                highResImage = _highResImage;
            }
        }

        public List<QueueEntry> queue = new List<QueueEntry>();

        public string ipAddress;
        public int port;

        public DownloadEngine downloadEngine;

        private Thread workingThread;

        public QueueEntry FindEntry(long _id, string _path)
        {
            for(int i = 0;i<queue.Count;i++)
            {
                if(queue[i].id == _id && queue[i].path == _path)
                {
                    return queue[i];
                }
            }

            return null;
        }

        public void DownloadThread()
        {
            /*if (downloadEngine == null)
            {
                downloadEngine = new DownloadEngine(StoreClient.createNewClient(ipAddress, port), token);
            }*/
            while(true)
            {
                while(queue.Count == 0)
                {
                    Thread.Sleep(100);
                }

                QueueEntry entry = queue[0];

                downloadEngine = new DownloadEngine(StoreClient.createNewClient(ipAddress, port), entry.token);

                if (entry.type == DownloadTypes.game)
                {
                    downloadEngine.DownloadGame(entry.id, entry.path, entry.completeFunction);
                }
                else if(entry.type == DownloadTypes.icon)
                {
                    downloadEngine.DownloadIcon(entry.id, entry.path, entry.completeFunction, entry.highResImage);
                }
                

                

                while(downloadEngine.working)
                {
                    Thread.Sleep(100);
                }

                queue.RemoveAt(0);
            }
        }

        public DownloadEngine DownloadGame(long id, string token, string outputPath, Action<DownloadEngine.DownloadStatus, long, DownloadTypes> completeFunction = null, bool ignoreThatSameEntryExists = false)
        {


            /*int empty = -1;
            Library.Game game = Library.FindGame(id, out empty);
            if (game == null)
            {
                Debug.LogError("Game with id " + id + " not found!!!");
                return null;
            }
            Debug.Log("Downloading game...");
            if (!Directory.Exists(gamesPath + game.path))
            {
                Directory.CreateDirectory(gamesPath + game.path);
            }*/
            //downloadEngine.DownloadGame(id, outputPath, completeFunction);

            if(FindEntry(id, outputPath) != null && !ignoreThatSameEntryExists)
            {
                Debug.LogWarning("Download entry already exist, returning");
                return downloadEngine;
            }

            QueueEntry entry = new QueueEntry(id, outputPath, token, completeFunction, DownloadTypes.game);

            queue.Add(entry);

            return downloadEngine;
        }

        public DownloadEngine DownloadIcon(long gameID, string token, string outputPath, Action<DownloadEngine.DownloadStatus, long, DownloadTypes> completeFunction = null, bool ignoreThatSameEntryExists = false, bool highRes = false)
        {
            if (FindEntry(gameID, outputPath) != null && !ignoreThatSameEntryExists)
            {
                Debug.LogWarning("Download entry already exist, returning");
                return downloadEngine;
            }

            QueueEntry entry = new QueueEntry(gameID, outputPath, token, completeFunction, DownloadTypes.icon, highRes);

            queue.Add(entry);

            return downloadEngine;
        }

        public DownloadManager(string _ip, int _port)
        {
            ipAddress = _ip;
            port = _port;

            workingThread = new Thread(DownloadThread);
            workingThread.Start();
        }
    }

    public class TestDownloadEngine
    {
        FileStream stream;

        string outpuFileName = StoreClient.GetPath("output.bmp");


        public void SaveToFile(byte[] data, Int32 length)
        {
            stream.Write(data, 0, length);
        }

        public TestDownloadEngine(string ip, int port)
        {
            Debug.LogWarning("TestDownloadEngien is deprecated, might not work");

            ClientEngine client = new ClientEngine(ip, port);

            DownloadEngine engine = new DownloadEngine(client, "TESTTOKEN");

            string wp = engine.ReceiveData();

            //string data = engine.ReceiveData();


            if (File.Exists(outpuFileName))
            {
                File.Delete(outpuFileName);
            }
            stream = File.Create(outpuFileName);



            Stopwatch watch = new Stopwatch();
            watch.Start();

            Debug.Log("Downloading...");
            byte[] bytes = engine.ReceiveDataRaw();
            watch.Stop();

            Debug.Log("Download Complete\nBytes lenght: " + stream.Position);

            Debug.Log("Time elapsed: " + watch.ElapsedMilliseconds);
            Debug.Log("Averge speed: " + (stream.Position / ((float)watch.ElapsedMilliseconds / 1000f)) + "B/s ");


            //Debug.Log("Received engine data: ");
            //Debug.Log(data);






            //byte[] bytes = Encoding.ASCII.GetBytes(data);

            //file.Write(bytes, 0, bytes.Length);

            //file.Close();

            stream.Close();
        }
    }

}
