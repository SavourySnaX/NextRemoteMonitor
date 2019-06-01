using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;

namespace SimpleMonitor.DockableWindows
{
    public partial class MemoryView :  BaseDock
    {
        byte currentBank;
        MyByteProvider provider;

        public MemoryView(string Type) : base(Type)
        {
            InitializeComponent();

            provider = new MyByteProvider();
            hexBox.ByteProvider = provider;
            hexBox.BytesPerLine = 8;
            hexBox.LineInfoOffset = 0;

            currentBank = 255;
            BankNum.Value = 0;

            RefreshMemory();
        }

        void RefreshMemory()
        {
            if (currentBank != BankNum.Value)
            {
                currentBank = (byte)BankNum.Value;
                Program.rc.SendCommand(new RemoteControl.Command(RecvData), currentBank);
            }
        }

        void RecvData(NetworkStream stream, params object[] arguments)
        {
            byte bank = Convert.ToByte(arguments[0]);

            stream.WriteByte(2);    // 2 recieve binary data
            stream.WriteByte(bank);    // Bank
            stream.WriteByte((byte)((0) & 255));
            stream.WriteByte((byte)(((0) >> 8) & 255)); // Address

            int length = 8192;
            stream.WriteByte((byte)((length) & 255));
            stream.WriteByte((byte)(((length) >> 8) & 255)); // size

            byte[] data = new byte[length];
            int bytesRead = 0;
            int position = 0;
            while (length!=0)
            {
                bytesRead = stream.Read(data, position, length);
                length -= bytesRead;
                position += bytesRead;
            }

            Invoke((MethodInvoker)delegate () { DataRecieved(data,bank); });
        }

        void DataRecieved(byte[] data,byte bank)
        {
            provider.NewBank(data,bank);
            hexBox.LineInfoOffset = bank * 8192;
        }

        class MyByteProvider : IByteProvider
        {
            public long Length => 8192;

            public event EventHandler LengthChanged;
            public event EventHandler Changed;

            void OnChanged(EventArgs e)
            {
                hasChanges = true;
                if (Changed != null)
                    Changed(this, e);
            }

            byte[] data;
            byte bank;
            bool hasChanges = true;
            public MyByteProvider()
            {
                data = new byte[Length];
                bank = 255;
                hasChanges = false;
            }

            public void ApplyChanges()
            {
                hasChanges = false;
            }

            public void NewBank(byte[] _data, byte _bank)
            {
                data = _data;
                bank = _bank;
                hasChanges = true;
                OnChanged(EventArgs.Empty);
            }

            public void DeleteBytes(long index, long length)
            {
                throw new NotImplementedException();
            }

            public bool HasChanges()
            {
                return hasChanges;
            }

            public void InsertBytes(long index, byte[] bs)
            {
                throw new NotImplementedException();
            }

            public byte ReadByte(long index)
            {
                return data[index];
            }

            public bool SupportsDeleteBytes()
            {
                return false;
            }

            public bool SupportsInsertBytes()
            {
                return false;
            }

            public bool SupportsWriteByte()
            {
                return true;
            }

            public void WriteByte(long index, byte value)
            {
                data[index] = value;
                OnChanged(EventArgs.Empty);
                Program.rc.SendCommand(new RemoteControl.Command(SendByte), bank, index, value);
            }

            void SendByte(NetworkStream stream, params object[] arguments)
            {
                byte bank = Convert.ToByte(arguments[0]);
                UInt16 offset = Convert.ToUInt16(arguments[1]);
                byte value = Convert.ToByte(arguments[2]);

                stream.WriteByte(1);    // 1 send binary data
                stream.WriteByte(bank);    // Bank
                stream.WriteByte((byte)((offset) & 255));
                stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address

                UInt16 length = 1;
                stream.WriteByte((byte)((length) & 255));
                stream.WriteByte((byte)(((length) >> 8) & 255)); // size

                stream.WriteByte(value);
            }

        }

        private void ValueChanged(object sender, EventArgs e)
        {
            RefreshMemory();
        }
    }
}
