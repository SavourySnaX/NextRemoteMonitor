using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleMonitor
{
    public partial class ToolBar : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public ToolBar()
        {
            InitializeComponent();
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
    }
}
