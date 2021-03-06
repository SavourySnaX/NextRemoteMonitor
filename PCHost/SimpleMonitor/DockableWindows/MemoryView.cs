﻿using System;
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

        protected override List<Tuple<string,string>> Persist()
        {
            var t = new List<Tuple<string, string>>();
            t.Add(Tuple.Create("BankNum.Value", BankNum.Value.ToString()));
            t.Add(Tuple.Create("ColumnWidth.Value", ColumnWidth.Value.ToString()));
            return t;
        }

        protected override void Persist(string name,string value)
        {
            switch (name)
            {
                case "BankNum.Value":
                    BankNum.Value = Convert.ToDecimal(value);
                    break;
                case "ColumnWidth.Value":
                    ColumnWidth.Value = Convert.ToDecimal(value);
                    break;
                default:
                    break;
            }
        }

        public override void ForceRefresh()
        {
            RefreshMemory();
        }

        public MemoryView(string Type) : base(Type)
        {
            InitializeComponent();

            provider = new MyByteProvider();
            hexBox.ByteProvider = provider;
            hexBox.LineInfoOffset = 0;
            hexBox.ContextMenu = null;

            currentBank = 255;
            BankNum.Value = 0;

            RefreshMemory();
        }

        void RefreshMemory()
        {
            hexBox.Enabled = false;
            Text = $"Memory View - Bank {BankNum.Value}";
            currentBank = (byte)BankNum.Value;
            hexBox.BytesPerLine = (int)ColumnWidth.Value;
            Program.rc.SendCommand(new RemoteControl.Command(RecvData), currentBank);
        }

        void RecvData(NetworkStream stream, params object[] arguments)
        {
            byte bank = Convert.ToByte(arguments[0]);

            byte[] data = NextNetworkHelpers.GetData(stream, bank, 0, 8192);

            Invoke((MethodInvoker)delegate () { DataRecieved(data,bank); });
        }

        void DataRecieved(byte[] data,byte bank)
        {
            provider.NewBank(data,bank);
            hexBox.LineInfoOffset = bank * 8192;
            hexBox.Enabled = true;
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
                byte[] data = new byte[1];
                data[0] = Convert.ToByte(arguments[2]);

                NextNetworkHelpers.SetData(stream, bank, offset, data);
            }

        }

        private void ValueChanged(object sender, EventArgs e)
        {
            RefreshMemory();
        }

        private void WidthChanged(object sender, EventArgs e)
        {
            hexBox.BytesPerLine = (int)ColumnWidth.Value;
        }
    }
}
