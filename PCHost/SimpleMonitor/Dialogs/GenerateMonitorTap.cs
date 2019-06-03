using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleMonitor
{
    public partial class GenerateMonitorTap : Form
    {
        string filePath;
        public GenerateMonitorTap()
        {
            InitializeComponent();
        }

        private void OK(object sender, EventArgs e)
        {
            // Open save dialog ..
            SaveFileDialog sfd = new SaveFileDialog();

            try
            {
                sfd.InitialDirectory = Path.GetDirectoryName(filePath);
                sfd.FileName = Path.GetFileName(filePath);
            }
            catch (Exception)
            {
                sfd.InitialDirectory = "";
                sfd.FileName = "MONITOR.TAP";
            }
            sfd.Filter = "Tape files (*.TAP)|*.TAP";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            sfd.Title = "Please choose a destination for the Spectrum Next Tape File (e.g. SDCARD!)";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // Load Monitor program and replace part of the data with the new IP address and port
                byte[] monitor = File.ReadAllBytes("monitor.dpl");
                ReplaceBytesInMonitor(monitor, Properties.Settings.Default.GenerateMonitorTap_IpAddress, (UInt16)Port.Value);
                filePath = sfd.FileNames[0];
                File.WriteAllBytes(filePath, monitor);

                Properties.Settings.Default.GenerateMonitorTap_IpAddress = $"{IP0.Value}.{IP1.Value}.{IP2.Value}.{IP3.Value}";
                Properties.Settings.Default.GenerateMonitorTap_FilePath = filePath;
                Properties.Settings.Default.GenerateMonitorTap_Location = Location;
                Properties.Settings.Default.Save();
            }

            Close();

        }

        private void ReplaceBytesInMonitor(byte[] tapefile, string ip, UInt16 port)
        {
            // Assumes the monitor code is the last block on the tape - which it is for the one we ship!!!
            if (tapefile[0] != 0x13 && tapefile[1] != 0x00)
                return;

            // because the monitor is the last block, the checksum is the last byte
            int checksumPos = tapefile.Length-1;

            //Slow but will do for now
            string toFind = "RRREPPPLAAACEEE";
            byte[] bToFind = Encoding.ASCII.GetBytes(toFind);
            for (int outer = 0;outer < tapefile.Length;outer++)
            {
                bool found = true;
                for (int inner = 0; inner < bToFind.Length; inner++)
                {
                    if (bToFind[inner] != tapefile[outer+inner])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    // outer points to start of sequence, there should be enough padding for this to "just work"
                    string ToNext = $"{ip}\",{port}\r\n\0";
                    byte[] toNext = Encoding.ASCII.GetBytes(ToNext);

                    for (int a=0;a<toNext.Length;a++)
                    {
                        tapefile[checksumPos] ^= tapefile[outer + a];
                        tapefile[outer + a]= toNext[a];
                        tapefile[checksumPos] ^= toNext[a];     // deal with checksum
                    }
                    return;
                }
            }
        }

        private void GetLocalIP(object sender, EventArgs e)
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    string localIP = endPoint.Address.ToString();
                    SetIPBoxes(localIP);
                }
            }
            catch (Exception )
            {
                MessageBox.Show("Couldn't determine local IP, sorry you will have to fill it in yourself!");
            }
        }

        private void Cancel(object sender, EventArgs e)
        {
            Close();
        }

        private void SetIPBoxes(string ip)
        {
            var split = ip.Split('.');
            IP0.Value = byte.Parse(split[0]);
            IP1.Value = byte.Parse(split[1]);
            IP2.Value = byte.Parse(split[2]);
            IP3.Value = byte.Parse(split[3]);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            SetIPBoxes(Properties.Settings.Default.GenerateMonitorTap_IpAddress);
            Port.Value = Properties.Settings.Default.Settings_Port;
            Location = Properties.Settings.Default.GenerateMonitorTap_Location;
            filePath = Properties.Settings.Default.GenerateMonitorTap_FilePath;
        }

    }
}
