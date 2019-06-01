using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleMonitor
{
    static class Program
    {
        public static RemoteControl rc;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            rc = new RemoteControl(Properties.Settings.Default.Settings_IpBindAddress, Properties.Settings.Default.Settings_Port);
            rc.StartServer();
            Application.Run(new MainForm());
        }
    }
}
