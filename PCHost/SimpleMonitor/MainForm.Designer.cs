namespace SimpleMonitor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.dockPanel1 = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.generateNewMonitorTapeFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newConsoleWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newMemoryWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newDisassemblyWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRegisterWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.disconnect = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.sendBinary = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.sendSNA = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dockPanel1
            // 
            this.dockPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel1.Location = new System.Drawing.Point(0, 49);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.Size = new System.Drawing.Size(800, 401);
            this.dockPanel1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateNewMonitorTapeFileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.windowToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // generateNewMonitorTapeFileToolStripMenuItem
            // 
            this.generateNewMonitorTapeFileToolStripMenuItem.Name = "generateNewMonitorTapeFileToolStripMenuItem";
            this.generateNewMonitorTapeFileToolStripMenuItem.Size = new System.Drawing.Size(188, 20);
            this.generateNewMonitorTapeFileToolStripMenuItem.Text = "&Generate New Monitor Tape File";
            this.generateNewMonitorTapeFileToolStripMenuItem.Click += new System.EventHandler(this.OpenGenerate);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "&Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.OpenSettings);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newConsoleWindowToolStripMenuItem,
            this.newMemoryWindowToolStripMenuItem,
            this.newDisassemblyWindowToolStripMenuItem,
            this.newRegisterWindowToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "&Window";
            // 
            // newConsoleWindowToolStripMenuItem
            // 
            this.newConsoleWindowToolStripMenuItem.Name = "newConsoleWindowToolStripMenuItem";
            this.newConsoleWindowToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.newConsoleWindowToolStripMenuItem.Text = "New &Console Window";
            this.newConsoleWindowToolStripMenuItem.Click += new System.EventHandler(this.NewConsoleView);
            // 
            // newMemoryWindowToolStripMenuItem
            // 
            this.newMemoryWindowToolStripMenuItem.Name = "newMemoryWindowToolStripMenuItem";
            this.newMemoryWindowToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.newMemoryWindowToolStripMenuItem.Text = "New &Memory Window";
            this.newMemoryWindowToolStripMenuItem.Click += new System.EventHandler(this.NewMemoryWindow);
            // 
            // newDisassemblyWindowToolStripMenuItem
            // 
            this.newDisassemblyWindowToolStripMenuItem.Name = "newDisassemblyWindowToolStripMenuItem";
            this.newDisassemblyWindowToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.newDisassemblyWindowToolStripMenuItem.Text = "New &Disassembly Window";
            this.newDisassemblyWindowToolStripMenuItem.Click += new System.EventHandler(this.NewDisassmView);
            // 
            // newRegisterWindowToolStripMenuItem
            // 
            this.newRegisterWindowToolStripMenuItem.Name = "newRegisterWindowToolStripMenuItem";
            this.newRegisterWindowToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.newRegisterWindowToolStripMenuItem.Text = "New &Register Window";
            this.newRegisterWindowToolStripMenuItem.Click += new System.EventHandler(this.NewRegisterView);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disconnect,
            this.toolStripSeparator1,
            this.sendBinary,
            this.toolStripSeparator2,
            this.sendSNA});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // disconnect
            // 
            this.disconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.disconnect.Image = ((System.Drawing.Image)(resources.GetObject("disconnect.Image")));
            this.disconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.disconnect.Name = "disconnect";
            this.disconnect.Size = new System.Drawing.Size(70, 22);
            this.disconnect.Text = "&Disconnect";
            this.disconnect.Click += new System.EventHandler(this.Disconnect);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // sendBinary
            // 
            this.sendBinary.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.sendBinary.Image = ((System.Drawing.Image)(resources.GetObject("sendBinary.Image")));
            this.sendBinary.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.sendBinary.Name = "sendBinary";
            this.sendBinary.Size = new System.Drawing.Size(73, 22);
            this.sendBinary.Text = "Send &Binary";
            this.sendBinary.Click += new System.EventHandler(this.SendBinary);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // sendSNA
            // 
            this.sendSNA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.sendSNA.Image = ((System.Drawing.Image)(resources.GetObject("sendSNA.Image")));
            this.sendSNA.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.sendSNA.Name = "sendSNA";
            this.sendSNA.Size = new System.Drawing.Size(63, 22);
            this.sendSNA.Text = "Send S&NA";
            this.sendSNA.Click += new System.EventHandler(this.SendSNA);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dockPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Spectrum Next Remote Debugger";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_Closing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton disconnect;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton sendBinary;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newMemoryWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateNewMonitorTapeFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newDisassemblyWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRegisterWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newConsoleWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton sendSNA;
    }
}

