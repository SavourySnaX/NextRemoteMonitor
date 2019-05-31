using System;
using System.Threading;

namespace SimpleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            RemoteControl rc = new RemoteControl();     // This will start a server running on a remote thread

            while (rc.IsRunning)
            {
                //Wait for connection
                if (rc.IsConnected)
                {
                    DisplayHelp();

                    // Wait for input
                    string input = Console.ReadLine();

                    if (input.ToLower()=="quit" || input.ToLower()=="q")
                    {
                        rc.SendCommand(RemoteControl.Commands.Exit);
                        while (rc.IsConnected)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    if (input.ToLower()=="send" || input.ToLower()=="s")
                    {
                        rc.SendCommand(RemoteControl.Commands.SendData);
                    }
                    if (input.ToLower()=="recv" || input.ToLower()=="r")
                    {
                        rc.SendCommand(RemoteControl.Commands.RecvData);
                    }
                }
                else
                {
                    Console.WriteLine("Waiting for Connection...");
                    Thread.Sleep(1000);
                }
            }
        }

        static void DisplayHelp()
        {
            Console.WriteLine("Initial proof of concept, quite hardwired!");
            Console.WriteLine("Enter QUIT/Q to disconnect");
            Console.WriteLine("Enter SEND/S to send - currently hardwired will send a scr$ across to the spectrum");
            Console.WriteLine("Enter RECV/R to recv - currently hardwired to recieve a copy of the screen and throw it away");
        }
    }
}
