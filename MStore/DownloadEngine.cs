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

        /// <summary>
        /// Tells if something is uploading / downloading
        /// </summary>
        public bool working { get; private set; } = false;

        /// <summary>
        /// Blocking function to receive data
        /// </summary>
        /// <returns></returns>
        public string ReceiveData()
        {
            return client.WaitForReceive();
        }



        public byte[] ReceiveDataRaw(TestDownloadEngine test)
        {
            client.dataRawReceived = test.SaveToFile;

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

            return false;

        }

        public DownloadEngine(ClientEngine _client, string token)
        {
            client = _client;

            AuthenticateClient(token);
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
            byte[] bytes = engine.ReceiveDataRaw(this);
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
