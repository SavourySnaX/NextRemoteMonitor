
using System;
using System.Collections.Generic;
using System.Text;

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

        protected override string GetPersistString()
        {
            return $"{dockUniqueName}|{PersistString()}";
        }

        public void RestoreSettings(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                PersistString(s);
            }
        }

        private string PersistString()
        {
            StringBuilder s = new StringBuilder();
            var list = Persist();

            foreach (var t in list)
            {
                s.Append(t.Item1).Append("=").Append(t.Item2).Append(",");
            }

            return s.ToString();
        }

        private void PersistString(string s)
        {
            var split = s.Split(',');
            foreach (var sp in split)
            {
                if (!string.IsNullOrEmpty(sp))
                {
                    var name = sp.Split('=')[0];
                    var value = sp.Split('=')[1];
                    Persist(name, value);
                }
            }
        }

        protected virtual List<Tuple<string,string>> Persist()
        {
            return null;
        }

        public virtual void ForceRefresh() { }

        protected virtual void Persist(string name,string value) { }

        private void BaseDock_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            MainForm.myDocks.Remove(this);
        }
    }
}
