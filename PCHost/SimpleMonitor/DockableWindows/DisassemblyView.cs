using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleMonitor.DockableWindows
{
    public partial class DisassemblyView : BaseDock
    {
        protected override List<Tuple<string,string>> Persist()
        {
            var t = new List<Tuple<string, string>>();
            t.Add(Tuple.Create("BankNum.Value", BankNum.Value.ToString()));
            t.Add(Tuple.Create("BankOffset.Value", BankOffset.Value.ToString()));
            t.Add(Tuple.Create("TrackPC.Checked", trackPC.Checked.ToString()));
            return t;
        }

        protected override void Persist(string name,string value)
        {
            switch (name)
            {
                case "BankNum.Value":
                    BankNum.Value = Convert.ToDecimal(value);
                    break;
                case "BankOffset.Value":
                    BankOffset.Value = Convert.ToDecimal(value);
                    break;
                case "TrackPC.Checked":
                    trackPC.Checked = Convert.ToBoolean(value);
                    break;
                default:
                    break;
            }
        }

        public override void ForceRefresh()
        {
            RefreshMemory();
        }

        public DisassemblyView(string Type) : base(Type)
        {
            InitializeComponent();

            // Need to make this all more flexible, e.g. can wrap the Bank/Offset as a linear address
            //and deal with it that way, translating to bank/offset when transmitting to next.
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            RefreshMemory();
        }

        void RefreshMemory()
        {
            Text = $"Disassembly View - {String.Format("{0:X8}",(uint)(BankNum.Value*8192 + BankOffset.Value))}";
            Program.rc.SendCommand(new RemoteControl.Command(RecvData), (byte)BankNum.Value, (UInt16)BankOffset.Value, trackPC.Checked);
        }

        void RecvData(NetworkStream stream, params object[] arguments)
        {
            byte bank = Convert.ToByte(arguments[0]);
            UInt16 offset = Convert.ToUInt16(arguments[1]);
            bool trackPC = Convert.ToBoolean(arguments[2]);

            if (trackPC)
            {
                // Get Current Memory Banks
                byte[] banks = NextNetworkHelpers.GetCurrentBanks(stream);

                // Get Current Reg Values
                UInt16[] regs = NextNetworkHelpers.GetNextState(stream);

                var bankNum = regs[5] >> 13;

                bank = banks[bankNum];
                offset = (UInt16)(regs[5] & 0x1FFF);
            }

            byte[] data = NextNetworkHelpers.GetData(stream, bank, offset, 200);

            Invoke((MethodInvoker)delegate () { DataRecieved(data,bank,offset); });
        }

        void DataRecieved(byte[] data,byte bank,UInt16 offset)
        {
            // Disassemble the data....
            UInt64 remaining = (UInt64)data.Length;
            UInt64 address = bank * 8192ul + offset;
            SpectrumNextMemory memInfo = new SpectrumNextMemory();
            memInfo.Init(data, address);
            D_Z80 z80 = new D_Z80();

            StringBuilder s = new StringBuilder();

            while (remaining > 0)
            {
                UInt64 length = z80.Disassemble(memInfo, address, out string mnemonic);
                if (length == 0 || length>remaining)
                    break;

                StringBuilder b = new StringBuilder();

                remaining -= length;
                for (UInt64 a=0;a<length;a++)
                {
                    if (memInfo.FetchByte(address+a,out byte by))
                    {
                        b.Append(String.Format("{0:X2} ", by));
                    }
                    else
                    {
                        b.Append("?? ");
                    }
                }
                for (UInt64 a=length;a<5;a++)
                {
                    b.Append("   ");
                }

                s.AppendLine($"{String.Format("{0:X8}", address)}\t{b.ToString()}\t{mnemonic}");

                address += length;
            }

            Disassembly.Text = s.ToString();
        }

        private void ChangedFollow(object sender, EventArgs e)
        {
            bool trackingPC = trackPC.Checked;

            if (trackingPC)
            {
                BankNum.Enabled = false;
                BankOffset.Enabled = false;
                ForceRefresh();
            }
            else
            {
                BankNum.Enabled = true;
                BankOffset.Enabled = true;
                ForceRefresh();
            }
        }
    }
}
