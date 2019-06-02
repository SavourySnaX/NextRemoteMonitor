using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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
        static MainForm mainForm;
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

            Program.rc.OnConnected = ConnectionMade;
            Program.rc.OnDisconnected = ConnectionLost;
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
            t.Location = new Point(Location.X + Size.Width / 2, Location.Y + Size.Height / 2);
            myDocks.Add(t);
        }
    }
}
