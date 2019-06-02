using System;
using System.Windows.Forms;

namespace SimpleMonitor
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void OK(object sender, EventArgs e)
        {
            Properties.Settings.Default.Settings_IpBindAddress = $"{IP0.Value}.{IP1.Value}.{IP2.Value}.{IP3.Value}";
            Properties.Settings.Default.Settings_Port = (ushort)Port.Value;
            Properties.Settings.Default.Settings_Location = Location;
            Properties.Settings.Default.Save();

            Close();
        }

        private void Cancel(object sender, EventArgs e)
        {
            Close();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            string t = Properties.Settings.Default.Settings_IpBindAddress;
            var split = t.Split('.');
            IP0.Value = byte.Parse(split[0]);
            IP1.Value = byte.Parse(split[1]);
            IP2.Value = byte.Parse(split[2]);
            IP3.Value = byte.Parse(split[3]);
            Port.Value = Properties.Settings.Default.Settings_Port;
            Location = Properties.Settings.Default.Settings_Location;
        }
    }
}
