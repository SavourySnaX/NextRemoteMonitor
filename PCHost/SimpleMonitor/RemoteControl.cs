using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleMonitor
{
    class RemoteControl
    {
        const string IPToBindTo = "192.168.5.2";
        const Int32 PortToBindTo = 9999;

        // NOTE- Called from thread, be careful
        public delegate void Command(NetworkStream connection);

        public delegate void Connected(string handshake);
        public delegate void Disconnected();

        Thread worker;
        ConcurrentQueue<Command> commands;
        bool hasConnection;
        string connectionHandshake;
        bool closeDown;

        public Connected OnConnected {get; set;}
        public Disconnected OnDisconnected {get; set;}

        public RemoteControl()
        {
            OnConnected = null;
            commands = new ConcurrentQueue<Command>();
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
        public string Handshake => connectionHandshake;

        public void StopServer()
        {
            Thread.Sleep(500);
            worker.Abort();
        }

        // Send null to disconnect
        public void SendCommand(Command command)
        {
            if (hasConnection)
            {
                commands.Enqueue(command);
            }
        }

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
                while (!closeDown)
                {
                    TcpClient client = server.AcceptTcpClient();

                    // We only handle a single connection here at any time, 
                    //(How many Spectrum Nexts do you have?!)

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
                            connectionHandshake = System.Text.Encoding.ASCII.GetString(inputStream.ToArray());
                            
                            // flush any left over commands from prior connection
                            Command ignored; while(commands.TryDequeue(out ignored));

                            hasConnection = true;
                            if (OnConnected!=null)
                            {
                                OnConnected(connectionHandshake);
                            }
                        }

                        // Check if we have a pending command, 

                        if (hasConnection && commands.TryDequeue(out Command result))
                        {
                            if (result == null)
                            {
                                // for now just exit
                                stream.WriteByte(0);
                                Thread.Sleep(2500);     // wait for spectrum end to shutdown (hang up takes roughly 1 second either side)
                                break;
                            }
                            else
                            {
                                result(stream);
                            }
                        }

                    }
                    hasConnection = false;

                    // Shutdown and end connection
                    client.Close();

                    if (OnDisconnected != null)
                        OnDisconnected();
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
