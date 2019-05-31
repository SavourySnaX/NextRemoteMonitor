using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleHost
{
    class RemoteControl
    {
        const string IPToBindTo = "192.168.5.2";
        const Int32 PortToBindTo = 9999;

        public enum Commands
        {
            Exit=0,
            SendData=1,
            RecvData=2
        }

        Thread worker;
        ConcurrentQueue<Commands> commands;
        bool hasConnection;

        public RemoteControl()
        {
            commands = new ConcurrentQueue<Commands>();
            hasConnection = false;

            // Spin up a thread 
            worker = new Thread(delegate ()
            {
                NextRemoteHandler();
            });

            worker.Start();
        }

        public bool IsRunning => worker.IsAlive;
        public bool IsConnected => hasConnection;


        public void SendCommand(Commands command)
        {
            commands.Enqueue(command);
        }
                    //commands.Enqueue(Commands.SendData);
                    //commands.Enqueue(Commands.RecvData);
                    //commands.Enqueue(Commands.Exit);


        void NextRemoteHandler()
        {
            TcpListener server = null;
            try
            {
                Int32 port = PortToBindTo;
                IPAddress localAddr = IPAddress.Parse(IPToBindTo);

                server = new TcpListener(localAddr, port);

                server.Start();

                List<byte> inputStream = new List<byte>(); 
                Byte[] bytes = new Byte[4096];
                String data = null;

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    // We only handle a single connection here at any time, 
                    //(How many Spectrum Nexts do you have?!)

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    hasConnection = false;
                    // slightly less blocking?
                    while (true)
                    {
                        int res = 0;
                        inputStream.Clear();
                        while (stream.DataAvailable)
                        {
                            res = stream.ReadByte();
                            if (res == -1)
                                break;
                            inputStream.Add((byte)res);
                        }
                        if (res == -1)
                            break;

                        Thread.Sleep(1);

                        if (inputStream.Count > 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(inputStream.ToArray());
                            hasConnection = true;
                        }

                        // Check if we have a pending command, 

                        if (hasConnection && commands.TryDequeue(out Commands result))
                        {
                            bool allDone = false;
                            switch (result)
                            {
                                case Commands.SendData: // BK, ADDR, LEN
                                    stream.WriteByte(1);
                                    stream.WriteByte(0);    // Bank
                                    stream.WriteByte((16384) & 255);
                                    stream.WriteByte(((16384) >> 8) & 255); // Address
                                    stream.WriteByte((6912) & 255);
                                    stream.WriteByte(((6912) >> 8) & 255);  // Length

                                    var pic = File.ReadAllBytes("poc.scr");
                                    stream.Write(pic, 0, 6912);
                                    break;

                                case Commands.RecvData: // BK,ADDR, LEN
                                    stream.WriteByte(2);
                                    stream.WriteByte(0);    // Bank
                                    stream.WriteByte((16384) & 255);
                                    stream.WriteByte(((16384) >> 8) & 255); // Address
                                    stream.WriteByte((6912) & 255);
                                    stream.WriteByte(((6912) >> 8) & 255);  // Length

                                    byte [] screen = new byte[6912];
                                    int read = 0;
                                    while (read!=6912)
                                    {
                                        read+=stream.Read(screen, read, 6912-read);
                                    }
                                    
                                    //File.WriteAllBytes("C:\\work\\next\\from.scr",screen);
                                    break;

                                case Commands.Exit:
                                default:
                                    {
                                        // for now just exit
                                        stream.WriteByte(0);
                                        Thread.Sleep(2500);     // wait for spectrum end to shutdown (hang up takes roughly 1 second either side)
                                    }
                                    allDone = true;
                                    break;
                               
                            }

                            if (allDone)
                                break;
                        }

                    }
                    hasConnection = false;

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
    }
}
