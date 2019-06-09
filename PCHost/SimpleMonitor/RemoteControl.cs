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
        string IPToBindTo;
        uint PortToBindTo;

        class CommandArgs
        {
            public Command command;
            public object[] arguments;
        }
        // NOTE- Called from thread, be careful
        public delegate void Command(NetworkStream connection,params object[] parameters);

        public delegate void Connected(string handshake);
        public delegate void Disconnected();

        Thread worker;
        ConcurrentQueue<CommandArgs> commands;
        bool hasConnection;
        string connectionHandshake;

        public Connected OnConnected {get; set;}
        public Disconnected OnDisconnected {get; set;}


        public RemoteControl(string ip, uint port)
        {
            OnConnected = null;
            commands = new ConcurrentQueue<CommandArgs>();
            hasConnection = false;
            IPToBindTo = ip;
            PortToBindTo = port;
        }

        public void StartServer()
        {
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
            server.Stop();
            worker.Abort();
        }

        // Send null to disconnect
        public void SendCommand(Command command,params object[] arguments)
        {
            if (hasConnection)
            {
                if (command == null)
                    commands.Enqueue(null);
                else
                {
                    CommandArgs com = new CommandArgs();
                    com.command = command;
                    com.arguments = arguments;
                    commands.Enqueue(com);
                }
            }
        }

        TcpListener server = null;

        void NextRemoteHandler()
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(IPToBindTo);

                server = new TcpListener(localAddr, (int)PortToBindTo);

                server.Start();

                List<byte> inputStream = new List<byte>(); 
                Byte[] bytes = new Byte[4096];
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    // We only handle a single connection here at any time, 
                    //(How many Spectrum Nexts do you have?!)

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    hasConnection = false;
                    // slightly less blocking?

                    DateTime connectionResetAbort = DateTime.Now;
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

                        if (inputStream.Count > 0)
                        {
                            // Translate data bytes to a ASCII string.
                            connectionHandshake = System.Text.Encoding.ASCII.GetString(inputStream.ToArray());
                            
                            // flush any left over commands from prior connection
                            CommandArgs ignored; while(commands.TryDequeue(out ignored));

                            hasConnection = true;
                            OnConnected?.Invoke(connectionHandshake);
                        }

                        // Check if we have a pending command, 

                        if (hasConnection && commands.TryDequeue(out CommandArgs result))
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
                                result.command(stream, result.arguments);
                                Thread.Sleep(40);
                            }
                        }

                        if (!hasConnection && (DateTime.Now-connectionResetAbort)>TimeSpan.FromSeconds(5))
                        {
                            stream.WriteByte(0);
                            Thread.Sleep(2500);
                            break;
                        }

                    }
                    hasConnection = false;

                    // Shutdown and end connection
                    client.Close();

                    OnDisconnected?.Invoke();
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
