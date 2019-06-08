using System;
using System.Collections.Generic;
using System.Net.Sockets;

// Mostly used for testing things at present

namespace SimpleMonitor.DockableWindows
{
    public partial class ConsoleView : BaseDock
    {
        protected override List<Tuple<string,string>> Persist()
        {
            var t = new List<Tuple<string, string>>();
            //t.Add(Tuple.Create("BankNum.Value", BankNum.Value.ToString()));
            return t;
        }

        protected override void Persist(string name,string value)
        {
            switch (name)
            {
            /*    case "BankNum.Value":
                    BankNum.Value = Convert.ToDecimal(value);
                    break;*/
                default:
                    break;
            }
        }

        public override void ForceRefresh()
        {
        }

        public ConsoleView(string type) : base(type)
        {
            InitializeComponent();
        }

        private void StepNextInstruction(NetworkStream stream)
        {
            // Fetch bank data
            byte[] banks = new byte[8];

            for (int a = 0; a < 8; a++)
            {
                stream.WriteByte(4);
                stream.WriteByte((byte)(0x50 + a));
                banks[a] = (byte)stream.ReadByte();
            }
            // grab registers
            stream.WriteByte(8);
            UInt16[] regs = new UInt16[12];     // AF BC DE HL SP PC IX IY AF' BC' DE' HL'
            for (int a = 0; a < 12; a++)
            {
                UInt16 t = 0;
                byte b = (byte)stream.ReadByte();
                t = (byte)stream.ReadByte();
                t <<= 8;
                t |= b;
                regs[a] = t;
            }

            // Grab 5 bytes from around the current PC
            byte bank = banks[(byte)(regs[5] >> 13)];
            uint offset = (uint)(regs[5] & 0x1FFF);
            stream.WriteByte(2);    // 2 recieve binary data
            stream.WriteByte(bank);    // Bank
            stream.WriteByte((byte)((offset) & 255));
            stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address
            int length = 5;   // maybe make configurable
            stream.WriteByte((byte)((length) & 255));
            stream.WriteByte((byte)(((length) >> 8) & 255)); // size

            byte[] data = new byte[length];
            for (int a=0;a<length;a++)
            {
                data[a] = (byte)stream.ReadByte();
            }

            // We now want to insert a breakpoint after the current instruction
            //although we will of course need to handle the instructions that change the PC
            //like ret (the Z80.cs should have code to support this)

            UInt64 address = bank * 8192ul + offset;
            SpectrumNextMemory memInfo = new SpectrumNextMemory();
            memInfo.Init(data, address);
            D_Z80 z80 = new D_Z80();

            UInt64 olength = z80.GetLength(memInfo, address);
            address += olength;
            bank = (byte)(address / 8192);
            offset = (uint)(address & 0x1FFF);

            // Send command to insert a breakpoint
            stream.WriteByte(5);
            stream.WriteByte(bank);   // bank
            stream.WriteByte((byte)((offset) & 255));
            stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address
            stream.WriteByte(0); // bp num

            stream.WriteByte(7);       // resume
        }

        private void RunTest(NetworkStream stream, params object[] arguments)
        {
            int testMode = Convert.ToInt32(arguments[0]);
            switch (testMode)
            {
                case 0:

                    // Send command code to setup MM6 bank 10
                    stream.WriteByte(3);
                    stream.WriteByte(0x56);
                    stream.WriteByte(10);

                    // Send command to insert a breakpoint
                    stream.WriteByte(5);
                    stream.WriteByte(10);   // bank
                    UInt16 offset = 0x0000;
                    stream.WriteByte((byte)((offset) & 255));
                    stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address
                    stream.WriteByte(0); // bp num

                    // Send command to tell monitor to execute code
                    offset = 0xC000;
                    stream.WriteByte(6);
                    stream.WriteByte((byte)((offset) & 255));
                    stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address
                    break;
                case 1:
                    StepNextInstruction(stream);
                    break;
            }
            MainForm.mainForm.RefreshAllDockWindows();
        }

        private void CommandInput(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // TODO - history buffer?
            if (e.KeyChar==13)
            {
                var input = textBox1.Text;
                textBox1.Text = "";

                var split = input.Split(' ');

                if (split[0] == "test")
                {
                    if (int.TryParse(split[1], out int num))
                    {
                        Program.rc.SendCommand(new RemoteControl.Command(RunTest), num);
                    }
                }
                if (split[0] == "step" || split[0]=="s")
                {
                    Program.rc.SendCommand(new RemoteControl.Command(RunTest), 1);
                }
            }
        }
    }
}
