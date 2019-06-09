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
            byte[] banks = NextNetworkHelpers.GetCurrentBanks(stream);

            var regs = NextNetworkHelpers.GetNextState(stream);

            // Grab 5 bytes from around the current PC
            byte bank = banks[(byte)(regs[5] >> 13)];
            UInt16 offset = (UInt16)(regs[5] & 0x1FFF);
            byte[] data = NextNetworkHelpers.GetData(stream, bank, offset, 5);

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
            offset = (UInt16)(address & 0x1FFF);

            // Send command to insert a breakpoint
            NextNetworkHelpers.SetBreakpoint(stream, 0, bank, offset);

            NextNetworkHelpers.SendResume(stream);

        }

        private void RunTest(NetworkStream stream, params object[] arguments)
        {
            int testMode = Convert.ToInt32(arguments[0]);
            switch (testMode)
            {
                case 0:

                    // Send command code to setup MM6 bank 10
                    NextNetworkHelpers.SetNextRegister(stream, 0x56, 10);

                    // Send command to insert a breakpoint
                    NextNetworkHelpers.SetBreakpoint(stream, 0, 10, 0);

                    // Send command to tell monitor to execute code
                    NextNetworkHelpers.SendExecute(stream, 0xC000);
                    break;
                case 1:
                    StepNextInstruction(stream);
                    break;
            }
            MainForm.mainForm.RefreshAllDockWindows();
        }
        
        void RunOut(NetworkStream stream, params object[] arguments)
        {
            UInt16 port = Convert.ToUInt16(arguments[0]);
            byte value = Convert.ToByte(arguments[1]);

            NextNetworkHelpers.SetIOPort(stream, port, value);
        }

        private int ParseNumber(string value)
        {
            if (value[0] == '$')
            {
                return int.Parse(value.TrimStart('$'), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                return int.Parse(value);
            }
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
                if (split[0] == "out" && split.Length==3)
                {
                    Program.rc.SendCommand(new RemoteControl.Command(RunOut), ParseNumber(split[1]), ParseNumber(split[2]));
                }
            }
        }
    }
}
