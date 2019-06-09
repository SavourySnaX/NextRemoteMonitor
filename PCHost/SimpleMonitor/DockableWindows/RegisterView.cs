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
    public partial class RegisterView :  BaseDock
    {
        protected override List<Tuple<string,string>> Persist()
        {
            var t = new List<Tuple<string, string>>();
            //t.Add(Tuple.Create("BankNum.Value", BankNum.Value.ToString()));
            //t.Add(Tuple.Create("ColumnWidth.Value", ColumnWidth.Value.ToString()));
            return t;
        }

        protected override void Persist(string name,string value)
        {
            switch (name)
            {
/*                case "ColumnWidth.Value":
                    ColumnWidth.Value = Convert.ToDecimal(value);
                    break;*/
                default:
                    break;
            }
        }

        BindingList<RegisterItem> registerData;

        class RegisterItem
        {
            public string Register { get; set; }
            public string Alias { get; set; }
            public string Value { get; set; }
        }

        public override void ForceRefresh()
        {
            RefreshRegisters();
        }

        public CustomControls.StandardRegister AddRegister(string pair,string high,string low)
        {
            var control = new CustomControls.StandardRegister();
            control.SetRegisterName(pair, high, low);
            control.Value = 0x1234;
            control.ShowHex = true;
            control.RegisterChanged += RegisterChanged;
            flowLayoutPanel1.Controls.Add(control);
            return control;
        }

        private void RegisterChanged(object sender, EventArgs e)
        {
            Program.rc.SendCommand(new RemoteControl.Command(SetState), null);
        }

        CustomControls.StandardRegister[] registers = new CustomControls.StandardRegister[12];

        public RegisterView(string Type) : base(Type)
        {
            InitializeComponent();

            // Add custom controls to flow layout
            registers[0] = AddRegister("AF", "A", "F");
            registers[1] = AddRegister("BC", "B", "C");
            registers[2] = AddRegister("DE", "D", "E");
            registers[3] = AddRegister("HL", "H", "L");
            registers[4] = AddRegister("SP", "SPh", "SPl");
            registers[5] = AddRegister("PC", "PCh", "PCl");
            registers[6] = AddRegister("IX", "IXh", "IXl");
            registers[7] = AddRegister("IY", "IYh", "IYl");
            registers[8] = AddRegister("AF'", "A'", "F'");
            registers[9] = AddRegister("BC'", "B'", "C'");
            registers[10] = AddRegister("DE'", "D'", "E'");
            registers[11] = AddRegister("HL'", "H'", "L'");

            RefreshRegisters();
        }

        void RefreshRegisters()
        {
            Program.rc.SendCommand(new RemoteControl.Command(GetState), null);
        }

        void GetState(NetworkStream stream, params object[] arguments)
        {
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

            Invoke((MethodInvoker)delegate () { DataRecieved(regs); });
        }

        void SetState(NetworkStream stream, params object[] arguments)
        {
            UInt16[] regs = new UInt16[12];     // AF BC DE HL SP PC IX IY AF' BC' DE' HL'
            for (int a=0;a<regs.Length;a++)
            {
                regs[a] = (UInt16)registers[a].Value;
            }
            stream.WriteByte(9);
            for (int a = 0; a < regs.Length; a++)
            {
                stream.WriteByte((byte)(regs[a] & 0xFF));
                stream.WriteByte((byte)((regs[a] >> 8) & 0xFF));
            }
        }

        void DataRecieved(UInt16 [] regs)
        {
            for (int a=0;a<regs.Length;a++)
            {
                registers[a].Value = regs[a];
            }
        }
        

    }
}
