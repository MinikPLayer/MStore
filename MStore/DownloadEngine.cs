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
        public Action<DownloadStatus, long> downloadCompleteFunction;

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

        int fileSize = 0;
        private void SaveToFile(byte[] data, Int32 length)
        {
            //Debug.Log("Received");
            //Debug.Log("Writing data with length: " + length);
            fileSize += length;
            stream.Write(data, 0, length);
        }

        public enum DownloadStatus
        {
            unknown,
            success,
            fileNotFoundOnServer,
            cannotOpenFile,
            unfinished,
            connectionLost,       
            notAuthorised
        }

        private enum RequestStatus
        {
            unknown,
            success,
            fileNotFound,
            connectionLost,
            notAuthorised
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
                downloadCompleteFunction(status, id);
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
            Debug.Log("Downloaded " + fileSize + " bytes");

            DownloadStatus status = DownloadStatus.success;


            

            FinishDownloading(status, id);
            return;
        }

        private void _DownloadGames(Int64[] gamesIDs, string[] outputFiles)
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
        }

        /// <summary>
        /// Downloads file from server
        /// </summary>
        /// <param name="id">Game ID to download</param>
        /// <param name="outputFilePath">Downloaded file path</param>
        /// <returns></returns>
        public bool DownloadGame(Int64 id, string outputFile, Action<DownloadStatus, long> _downloadCompleteFunction = null)
        {
            if(working)
            {
                Debug.LogWarning("Download engine is downloading, cannot start new download");
                return false;
            }

            downloadCompleteFunction = _downloadCompleteFunction;

            working = true;

            workingThread = new Thread(() => _DownloadGame(id, outputFile));
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
