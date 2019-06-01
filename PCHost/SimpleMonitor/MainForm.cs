using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleMonitor.DockableWindows;

namespace SimpleMonitor
{
    public partial class MainForm : Form
    {
        static MainForm mainForm;
        public static List<BaseDock> myDocks=new List<BaseDock>();

        public MainForm()
        {
            mainForm = this;
            InitializeComponent();
            ConnectionLost();

            Program.rc.OnConnected = ConnectionMade;
            Program.rc.OnDisconnected = ConnectionLost;

            //buttons = new ToolBar();

            //buttons.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockTop);

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
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

        static void ConnectionMade(string handshake)
        {
            if (mainForm.InvokeRequired)
                mainForm.Invoke((MethodInvoker)delegate () { ConnectionMade(handshake); });
            else
                mainForm.Text = "Spectrum Next Remote Debugger - Connected";
        }

        static void ConnectionLost()
        {
            if (mainForm.InvokeRequired)
                mainForm.Invoke((MethodInvoker)delegate () { ConnectionLost(); });
            else
                mainForm.Text = "Spectrum Next Remote Debugger - Waiting for Connection";
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
                dialog.ShowDialog();
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
    }
}
