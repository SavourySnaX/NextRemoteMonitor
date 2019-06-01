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

namespace SimpleMonitor
{
    public partial class MainForm : Form
    {
        ToolBar buttons;

        static MainForm mainForm;

        public MainForm()
        {
            mainForm = this;
            InitializeComponent();
            ConnectionLost();

            Program.rc.OnConnected = ConnectionMade;
            Program.rc.OnDisconnected = ConnectionLost;

            buttons = new ToolBar();

            buttons.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockTop);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
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
    }
}
