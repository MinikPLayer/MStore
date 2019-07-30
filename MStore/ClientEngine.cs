﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

using System.Threading;

using System.Diagnostics;

namespace MStore
{
    public class ClientEngine
    {
        private TcpClient socket = null;
        private string address = "";
        private int port = -1;

        public string receiveData = "";
        const int maxDataPacketSize = 255;
        public byte[] rawReceivedata = new byte[maxDataPacketSize * 2];

        public delegate void DataRawReceived(byte[] dataReceived, Int32 length);
        public DataRawReceived dataRawReceived;

        
        public ClientEngine(string _address, Int32 _port)
        {
            address = _address;
            port = _port;


            Connect();
        }

        private void Send_LowLevel(string data)
        {
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);

            string sendData = "";
            for(int i = 0;i<buffer.Length;i++)
            {
                sendData += buffer[i].ToString() + ' ';
            }

            //Debug.Log("Sending " + sendData);


            try
            {
                NetworkStream stream = socket.GetStream();

                stream.Write(buffer, 0, buffer.Length);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message + " at " + e.Source);
            }
        }

        public void _Send(string data)
        {
            byte size = (byte)((char)data.Length);
            string size_str = "";
            //size_str += size;
            size_str += (char)size;
            
            Debug.Log("Size str lenth: " + size_str.Length);
            Debug.Log("Sending size_str: " + size_str);

            Send_LowLevel(size_str);    // Length

            //Thread.Sleep(500);

            Send_LowLevel(data);
        }

        public Thread Send(string data)
        {
            Thread newThread = new Thread(() => _Send(data));
            newThread.Start();
            return newThread;
        }

        public struct Message
        {
            public string data;
            public string code;

            public Message(string messageData, string messageCode)
            {
                data = messageData;
                code = messageCode;
            }

            public static Message zero = new Message("", "");
        }

        private void AddBytesArrayToList(byte[] bytesArray, List<byte> list, Int32 size)
        {
            for(int i = 0;i< size; i++)
            {
                list.Add(bytesArray[i]);
            }
        }

        private void AddBytesToByteArray(byte[] src, byte[] dst, Int32 size, int dstOffset)
        {
            for(int i = 0;i<size;i++)
            {
                dst[dstOffset + i] = src[i];
            }
        }

        private void Receive_LowLevel_Raw(int size, float timeout, bool receiveMessageCode)
        {

        }


        public Int32 bytesReceived = -1;
        
        private string __ReceiveData = "";
        private Message Receive_LowLevel(int size, float timeout, bool receiveMessageCode, bool useString = true, bool resetBytesReceived = true)
        {
            if (socket == null) return new Message("-", "-");
            /*byte[] buffer;
            if (size == -1)
            {
                buffer = new byte[maxDataPacketSize + 1];
            }
            else
            {
                buffer = new byte[size + 1];
            }*/



            //try
           //{
                NetworkStream stream = socket.GetStream();

                float timeElapsed = 0f;
                int sleepTime = (int)(timeout / 50f);


                while (!stream.DataAvailable)
                {
                    timeElapsed += sleepTime / 1000f;
                    if (timeElapsed >= timeout)
                    {
                        Console.WriteLine("Timeout reached");
                        //receiveData = "\0";
                        return new Message("Timeout Reached", "ERROR");//"\0";
                    }
                    //Console.WriteLine("Waiting for data...");
                    Thread.Sleep(sleepTime);

                }



            //Mutex mtx = new Mutex();

            //mtx.WaitOne();



                if (resetBytesReceived)
                {
                    bytesReceived = 0;
                }
                //receiveData = "";
                string messageCode = "";

                if (receiveMessageCode)
                {
                    Debug.Log("Receiving message code");
                    byte[] codeBuffer = new byte[5];

                    int readedBytes = 0;

                    while (readedBytes != 5)
                    {
                        /*if (stream.Read(codeBuffer, readedBytes, 5) == 5)
                        {
                            messageCode = Encoding.UTF8.GetString(codeBuffer, 0, 5);
                            //Debug.Log("Message code: " + messageCode);
                        }
                        else
                        {
                            Debug.LogError("No message code received, probably some sort of error");
                        }*/
                        int streamRead = stream.Read(codeBuffer, readedBytes, 5 - readedBytes);
                        readedBytes += streamRead;
                        if(streamRead != 5)
                        {
                            Debug.LogWarning("Probably fragmented data");
                        }

                        messageCode = Encoding.UTF8.GetString(codeBuffer, 0, 5);
                    }
                    Debug.Log("Message Code: " + messageCode);
                }


                while (!stream.DataAvailable)
                {
                    timeElapsed += sleepTime / 1000f;
                    if (timeElapsed >= timeout)
                    {
                        Console.WriteLine("Timeout reached");
                        //receiveData = "\0";
                        return new Message("Timeout Reached", "ERROR");//"\0";
                    }
                    //Console.WriteLine("Waiting for data...");
                    Thread.Sleep(sleepTime);

                }

            /*if (size == 0)
            {
                rawReceivedata = new byte[maxDataPacketSize];
            }
            else
            {
                rawReceivedata = new byte[size];
            }*/




                int packetSize = size;
                //string data = "";
                //while (stream.DataAvailable && (bytesReceived < size || size == -1))
                while ((size == -1 && stream.DataAvailable) || (bytesReceived < size))
                {
                    
                    while(!stream.DataAvailable)
                    {

                    }


                //Int32 bytesCount = stream.Read(buffer, 0, size);
                //Debug.Log("Size: " + size);
                Int32 bytesCount = stream.Read(rawReceivedata, bytesReceived, packetSize);

                    //AddBytesToByteArray(buffer, rawReceivedata, bytesCount, bytesReceived);
                    //AddBytesArrayToList(buffer, rawReceivedata, bytesCount);
                    //receiveData += System.Text.Encoding.ASCII.GetString(buffer, 0, bytesCount);

                    //__ReceiveData += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesCount);
                    //__ReceiveData += Encoding.UTF8.GetString(rawReceivedata, bytesReceived, bytesCount);

                    bytesReceived += bytesCount;

                    packetSize -= bytesCount;

            }


                //Console.WriteLine("Readed data: " + __ReceiveData);

                string data = "";
                if (useString)
                {
                    __ReceiveData = Encoding.UTF8.GetString(rawReceivedata, 0, bytesReceived);

                    for(int i = 0;i<size && __ReceiveData.Length != 0;i++)
                    {
                        data += __ReceiveData[i];
                        i--;
                        __ReceiveData = __ReceiveData.Remove(0, 1);
                        size--;
                    }
                }




                //mtx.ReleaseMutex();



                //return new Message(data, messageCode);
                return new Message(data, messageCode);
            /*}
            catch (Exception e)
            {
                Debug.LogError("Receive_lowLevel ( last ): " + e.Message + " at " + e.Source + ", " + e.TargetSite);

                Debug.Log("Size: " + size);
                Debug.Log("bytesReceived: " + bytesReceived);
                return new Message(e.Message, "ERROR");
            }*/
        }

        private void _Receive(float timeout, bool clearReceive = true, bool receiveMessageCode = true, bool useString = true)
        {
            if (clearReceive)
            {
                receiveData = "";
            }

            byte size = 0;


            int count = 0;


            Stopwatch overrallWatch = new Stopwatch();

            overrallWatch.Start();
            do
            {
                size = 0;
                
                while (size == 0)
                {
                    Message message = Receive_LowLevel(1, timeout, receiveMessageCode, useString);
                    //receiveMessage = false;
                    /* if(!byte.TryParse(message.data, out size))
                     {
                         Debug.LogError("Message data conversion failed, trying to convert string " + message.data + " to byte.");
                         return;
                     }*/
                    //if (rawReceivedata.Length == 0)
                    if(bytesReceived == 0)
                    {
                        Debug.LogError("Error while reading data in _Receive(float timeout, bool clearReceive = true), size is 0");
                        Thread.Sleep(1000);
                        continue;
                        //return;
                    }
                    //size = (byte)message.data[0];
                    //size = rawReceivedata[rawReceivedata.Length - 1];
                    size = rawReceivedata[0];

                    //if(rawReceivedata[rawReceivedata.Length - 1] == size)
                    if(rawReceivedata[0] == size)
                    { 
                        //rawReceivedata.RemoveAt(rawReceivedata.Count - 1);
                        //byte[] newData = new byte[rawReceivedata.Length - 1];
                        /*byte[] newData = new byte[bytesReceived - 1];
                        for(int i = 0;i<newData.Length;i++)
                        {
                            newData[i] = rawReceivedata[i];
                        }

                        rawReceivedata = newData;*/

                        bytesReceived = 0;
                    }
                    else
                    {
                        Debug.LogError("Cannot found size in rawReceivedData :/");
                    }

                }

                //Debug.Log("Size: " + size);

                //byte.TryParse(message.code, out size);

                //Debug.Log("Size: " + size);

                receiveData += Receive_LowLevel(size, timeout, false, useString).data;

                if(dataRawReceived != null)
                {

                    dataRawReceived.Invoke(rawReceivedata, bytesReceived);

                    //rawReceivedata.Clear();
                    //rawReceivedata = null;
                    bytesReceived = 0;

                    receiveData = "";

                }

                if(count % 5000 == 0)
                {
                    Debug.Log("Averge full receive time: " + (overrallWatch.ElapsedMilliseconds / 5000f) + " ms");
                    overrallWatch.Restart();
                }

                count++;

            } while (size >= 255);

        }

        public Thread Receive(float timeout = 5f)
        {
            if(socket == null)
            {
                return null;
            }

            Thread newThread = new Thread(() => _Receive(timeout));
            newThread.Start();
            return newThread;
        }

        public string WaitForReceive(float timeout = 5f, bool receiveMessageCode = true, bool useString = true)
        {
            _Receive(timeout, true, receiveMessageCode, useString);
            string data = receiveData;
            receiveData = "";
            //rawReceivedata.Clear();
            //rawReceivedata = null;
            bytesReceived = 0;

            return data;
        }

        public byte[] WaitForReceiveRaw(float timeout = 5f, bool receiveMessageCode = true, bool useString = false)
        {
            _Receive(timeout, true, receiveMessageCode, useString);
            //byte[] data = rawReceivedata.ToArray();
            byte[] data = rawReceivedata;
            receiveData = "";
            //rawReceivedata.Clear();
            //rawReceivedata = null;
            bytesReceived = 0;
            
            return data;
        }
        

        private void Connect()
        {
            try
            {

                socket = new TcpClient(address, port);

                /*Receive(-1);
                Send("Minik");

                Thread th = Receive(1);
                th.Join();

                Send("NiePodamCiHasla");

                

                while (true) ;*/

            }
            catch (SocketException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Cannot connect to server on ip " + address + ", error code: " + e.ErrorCode + " ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                string message = "";
                switch (e.ErrorCode)
                {
                    case 10061:
                        //Console.WriteLine("Connection refused");
                        message = "Connection refused";
                        break;
                    case 11001:
                        //Console.WriteLine("IP is invalid");
                        message = "IP is invalid or DNS server returned an error";
                        break;
                }
                if (message.Length != 0)
                {
                    message = message.Insert(0, "( ");
                    message += " )";
                }
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}