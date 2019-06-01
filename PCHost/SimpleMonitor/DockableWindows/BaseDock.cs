
namespace SimpleMonitor.DockableWindows
{
    public class BaseDock : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public static int luid = 0;
        public string dockUniqueName;

        public BaseDock()
        {
            // Just for Visual Studio Designer
        }

        public BaseDock(string Type)
        {
            dockUniqueName = Type + ":" + luid++;
            FormClosing += BaseDock_FormClosing;
        }

        private void BaseDock_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            MainForm.myDocks.Remove(this);
        }
    }
}
