using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleMonitor
{
    public partial class LoadBinary : Form
    {
        public LoadBinary()
        {
            InitializeComponent();

        }

        private void OK(object sender, EventArgs e)
        {
            Program.rc.SendCommand(new RemoteControl.Command(SendData));
        }

        void SendData(NetworkStream stream)
        {

            byte bank = (byte)BankNum.Value;
            UInt16 offset = (UInt16)BankOffset.Value;

            stream.WriteByte(1);    // 1 Sending binary data
            stream.WriteByte(bank);    // Bank
            stream.WriteByte((byte)((offset) & 255));
            stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address

            try
            {
                var toSend = File.ReadAllBytes(FilePath.Text);
                UInt16 length = (UInt16)toSend.Length;
                stream.WriteByte((byte)((length) & 255));
                stream.WriteByte((byte)(((length) >> 8) & 255)); // size
                stream.Write(toSend, 0, length);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }

        private void ChooseFile(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            try
            {
                ofd.InitialDirectory = Path.GetDirectoryName(FilePath.Text);
            }
            catch (Exception)
            {
                ofd.InitialDirectory = "";
            }
            ofd.Filter = "All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FilePath.Text = ofd.FileNames[0];
            }
        }

        private void Cancel(object sender, EventArgs e)
        {
            Close();
        }
    }
}
