using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleMonitor.DockableWindows;

namespace SimpleMonitor
{
    public partial class MainForm : Form
    {
        public static MainForm mainForm;
        public static List<BaseDock> myDocks=new List<BaseDock>();
        WeifenLuo.WinFormsUI.Docking.DeserializeDockContent serializeDocks;

        public MainForm()
        {
            mainForm = this;
            InitializeComponent();
            ConnectionLost();

            serializeDocks = new WeifenLuo.WinFormsUI.Docking.DeserializeDockContent(LayoutHandler);

            if (File.Exists("layout.xml"))
            {
                dockPanel1.LoadFromXml("layout.xml", serializeDocks);
            }

            Program.rc.OnConnected = new RemoteControl.Connected(ConnectionMade);
            Program.rc.OnDisconnected = new RemoteControl.Disconnected(ConnectionLost);
        }

        public WeifenLuo.WinFormsUI.Docking.IDockContent LayoutHandler(string name)
        {
            string iName = name.Split(':')[0];
            string cName = $"SimpleMonitor.DockableWindows.{iName}";
            string settings = name.Split('|')[1];
            foreach (BaseDock sv in myDocks)
            {
                if (sv.dockUniqueName == name)
                {
                    sv.RestoreSettings(settings);
                    return sv;
                }
            }
            // Instantiate from SimpleMonitor.DockableWindows.name
            var assembly = Assembly.GetExecutingAssembly();
            var type = assembly.GetType(cName);
            var t = (BaseDock)Activator.CreateInstance(type, iName);
            t.RestoreSettings(settings);
            myDocks.Add(t);
            return t;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Location = Properties.Settings.Default.MainWindow_Location;
            if (Properties.Settings.Default.MainWindow_State == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Normal;
                if (Properties.Settings.Default.MainWindow_Size.Width!=0 && Properties.Settings.Default.MainWindow_Size.Height!=0)
                {
                    Size = Properties.Settings.Default.MainWindow_Size;
                }
            }
            WindowState = Properties.Settings.Default.MainWindow_State;
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.MainWindow_Location = Location;
            Properties.Settings.Default.MainWindow_Size = Size;
            Properties.Settings.Default.MainWindow_State = WindowState;
            Properties.Settings.Default.Save();

            dockPanel1.SaveAsXml("layout.xml");

            CloseRemoteControl();
        }

        private void CloseRemoteControl()
        {
            Program.rc.SendCommand(null);   // disconnect spectrum

            while (Program.rc.IsRunning)
            {
                Program.rc.StopServer();
            }
        }

        private void ConnectionMade(string handshake)
        {
            if (mainForm.InvokeRequired)
                mainForm.Invoke((MethodInvoker)delegate () { ConnectionMade(handshake); });
            else
            {
                if (handshake != "MON!")
                {
                    Program.rc.SendCommand(null, null);
                    return;
                }
                RefreshAllDockWindows();
                mainForm.Text = "Spectrum Next Remote Debugger - Connected";
            }
        }

        private void ConnectionLost()
        {
            if (mainForm.InvokeRequired)
                mainForm.Invoke((MethodInvoker)delegate () { ConnectionLost(); });
            else
            {
                RefreshAllDockWindows();
                mainForm.Text = "Spectrum Next Remote Debugger - Waiting for Connection";
            }
        }

        public void RefreshAllDockWindows()
        {
            if (mainForm.InvokeRequired)
                mainForm.Invoke((MethodInvoker)delegate () { RefreshAllDockWindows(); });
            else
            {
                foreach (BaseDock dock in myDocks)
                {
                    dock.ForceRefresh();
                }
            }
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            using (var dialog = new Settings())
            {
                string prevIP = Properties.Settings.Default.Settings_IpBindAddress;
                uint prevPort = Properties.Settings.Default.Settings_Port;
                if (dialog.ShowDialog() == DialogResult.OK &&
                    (prevPort != Properties.Settings.Default.Settings_Port ||
                     prevIP != Properties.Settings.Default.Settings_IpBindAddress))
                {
                    CloseRemoteControl();
                    Program.rc = new RemoteControl(Properties.Settings.Default.Settings_IpBindAddress, Properties.Settings.Default.Settings_Port);
                    Program.rc.StartServer();
                }

            }
        }

        private void SendBinary(object sender, EventArgs e)
        {
            using (LoadBinary dialog = new LoadBinary())
            {
                if (dialog.ShowDialog()==DialogResult.OK)
                {
                    RefreshAllDockWindows();
                }
            }
        }

        private void Disconnect(object sender, EventArgs e)
        {
            Program.rc.SendCommand(null);
        }

        private void NewMemoryWindow(object sender, EventArgs e)
        {
            var t = new MemoryView("MemoryView");
            t.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.Float);
            myDocks.Add(t);
        }

        private void OpenGenerate(object sender, EventArgs e)
        {
            using (GenerateMonitorTap dialog = new GenerateMonitorTap())
            {
                dialog.ShowDialog();
            }

        }

        private void NewDisassmView(object sender, EventArgs e)
        {
            var t = new DisassemblyView("DisassemblyView");
            t.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.Float);
            myDocks.Add(t);
        }

        private void NewRegisterView(object sender, EventArgs e)
        {
            var t = new RegisterView("RegisterView");
            t.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.Float);
            myDocks.Add(t);
        }

        private void NewConsoleView(object sender, EventArgs e)
        {
            var t = new ConsoleView("ConsoleView");
            t.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.Float);
            myDocks.Add(t);
        }

        private void SendSNA(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = "";
            ofd.Filter = "Snapshot files (*.SNA)|*.SNA";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                byte[] data = File.ReadAllBytes(ofd.FileNames[0]);

                Program.rc.SendCommand(new RemoteControl.Command(SendSNA), data);
            }
        }

        void SendSNA(NetworkStream stream, params object[] arguments)
        {
            byte[] toSend = arguments[0] as byte[];

            NextNetworkHelpers.SetNextRegister(stream, 0x56, 10);

            // Send command to insert a breakpoint
            NextNetworkHelpers.SetBreakpoint(stream, 0, 10, 0);

            // Send command to tell monitor to execute code
            NextNetworkHelpers.SendExecute(stream, 0xC000);


            // Plan for 48K snaps is as follows :
            //The monitor code is normally mapped in at MMU0, but ideally we want the 48k ROM
            //and some modifications to lie there, for now I`ll achieve this by:
            //
            // Map 8K Pages as follows : 
            // Page 8
            // Page 9
            // Page 10
            // Page 11 
            // Page 12
            // Page 13
            // Page 14
            // Page 15

            var romBytes = File.ReadAllBytes("c:\\work\\next\\48.rom");

            //patch rst 66 - 14 byte area to account for retn needs
            for (int a = 0; a < 7; a++)
            {
                romBytes[0x66 + a * 2] = 0xED;
                romBytes[0x67 + a * 2] = 0x45;
            }

            NextNetworkHelpers.SetData(stream, 8, 0, romBytes);

            // AF BC DE HL SP PC IX IY AF' BC' DE' HL' IR IFF2
            UInt16[] regs = new ushort[14];
            regs[12] = toSend[0];
            regs[11] = toSend[1];
            regs[11]|= (UInt16)(toSend[2]<<8);
            regs[10] = toSend[3];
            regs[10]|= (UInt16)(toSend[4]<<8);
            regs[9] = toSend[5];
            regs[9]|= (UInt16)(toSend[6]<<8);
            regs[8] = toSend[7];
            regs[8]|= (UInt16)(toSend[8]<<8);
            regs[3] = toSend[9];
            regs[3]|= (UInt16)(toSend[10]<<8);
            regs[2] = toSend[11];
            regs[2]|= (UInt16)(toSend[12]<<8);
            regs[1] = toSend[13];
            regs[1]|= (UInt16)(toSend[14]<<8);
            regs[7] = toSend[15];
            regs[7]|= (UInt16)(toSend[16]<<8);
            regs[6] = toSend[17];
            regs[6]|= (UInt16)(toSend[18]<<8);
            regs[13] = (UInt16)((toSend[19] & 4) | ((toSend[19] & 4) << 8));
            regs[12]|= (UInt16)(toSend[20]<<8);
            regs[0] = toSend[21];
            regs[0]|= (UInt16)(toSend[22]<<8);
            regs[4] = toSend[23];
            regs[4] |= (UInt16)(toSend[24] << 8);
            regs[5] = 0x66;
            int interruptMode = toSend[25];
            byte currentPortValue = NextNetworkHelpers.GetIOPort(stream, 254);
            currentPortValue &= 0xF8;
            currentPortValue |= (byte)(toSend[26] & 0x07);
            NextNetworkHelpers.SetIOPort(stream, 254, currentPortValue);

            NextNetworkHelpers.SetData(stream, 10, 0, toSend, 27);

            NextNetworkHelpers.SetNextState(stream, regs);

            // todo should switch on layer 2 write mode to prevent 
            //programs writing over rom area

            // Don't page in first bank yet, 
            //NextNetworkHelpers.SetNextRegister(stream, 0x50, 8);
            NextNetworkHelpers.SetNextRegister(stream, 0x51, 9);
            NextNetworkHelpers.SetNextRegister(stream, 0x52, 10);
            NextNetworkHelpers.SetNextRegister(stream, 0x53, 11);
            NextNetworkHelpers.SetNextRegister(stream, 0x54, 12);
            NextNetworkHelpers.SetNextRegister(stream, 0x55, 13);
            NextNetworkHelpers.SetNextRegister(stream, 0x56, 14);
            NextNetworkHelpers.SetNextRegister(stream, 0x57, 15);

 //           NextNetworkHelpers.SetBreakpoint(stream, 0, 111, 0x66);

            // Will cause a jump to address 0x66 where a small routine will handle the
            //final handover
            NextNetworkHelpers.SendResume(stream);
        }




    }
}
