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
            Properties.Settings.Default.LoadBinary_FilePath = FilePath.Text;
            Properties.Settings.Default.LoadBinary_BankNum = (byte)BankNum.Value;
            Properties.Settings.Default.LoadBinary_BankOffset = (UInt16)BankOffset.Value;
            Properties.Settings.Default.LoadBinary_Location = Location;
            Properties.Settings.Default.Save();

            try
            {
                Program.rc.SendCommand(new RemoteControl.Command(SendData), BankNum.Value, BankOffset.Value, File.ReadAllBytes(FilePath.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        void SendData(NetworkStream stream, params object[] arguments)
        {
            byte bank = Convert.ToByte(arguments[0]);
            UInt16 offset = Convert.ToUInt16(arguments[1]);
            byte[] toSend = arguments[2] as byte[];

            stream.WriteByte(1);    // 1 Sending binary data
            stream.WriteByte(bank);    // Bank
            stream.WriteByte((byte)((offset) & 255));
            stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address

            UInt16 length = (UInt16)toSend.Length;
            stream.WriteByte((byte)((length) & 255));
            stream.WriteByte((byte)(((length) >> 8) & 255)); // size
            stream.Write(toSend, 0, length);
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
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Location = Properties.Settings.Default.LoadBinary_Location;
            FilePath.Text = Properties.Settings.Default.LoadBinary_FilePath;
            BankNum.Value = Properties.Settings.Default.LoadBinary_BankNum;
            BankOffset.Value = Properties.Settings.Default.LoadBinary_BankOffset;
        }
    }
}
