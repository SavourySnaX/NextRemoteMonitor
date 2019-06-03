namespace SimpleMonitor
{
    partial class GenerateMonitorTap
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
            this.label5 = new System.Windows.Forms.Label();
            this.IP3 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.IP2 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.IP1 = new System.Windows.Forms.NumericUpDown();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.Port = new System.Windows.Forms.NumericUpDown();
            this.IP0 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.IP3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IP2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IP1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Port)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IP0)).BeginInit();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(320, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 15);
            this.label5.TabIndex = 25;
            this.label5.Text = ".";
            // 
            // IP3
            // 
            this.IP3.Location = new System.Drawing.Point(336, 7);
            this.IP3.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.IP3.Name = "IP3";
            this.IP3.Size = new System.Drawing.Size(46, 20);
            this.IP3.TabIndex = 24;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(252, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 15);
            this.label4.TabIndex = 23;
            this.label4.Text = ".";
            // 
            // IP2
            // 
            this.IP2.Location = new System.Drawing.Point(268, 6);
            this.IP2.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.IP2.Name = "IP2";
            this.IP2.Size = new System.Drawing.Size(46, 20);
            this.IP2.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(184, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 15);
            this.label3.TabIndex = 21;
            this.label3.Text = ".";
            // 
            // IP1
            // 
            this.IP1.Location = new System.Drawing.Point(200, 6);
            this.IP1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.IP1.Name = "IP1";
            this.IP1.Size = new System.Drawing.Size(46, 20);
            this.IP1.TabIndex = 20;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(304, 72);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 19;
            this.button3.Text = "&Cancel";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Cancel);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(388, 72);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 18;
            this.button2.Text = "&Save Tape";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OK);
            // 
            // Port
            // 
            this.Port.Location = new System.Drawing.Point(288, 37);
            this.Port.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.Port.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.Port.Name = "Port";
            this.Port.ReadOnly = true;
            this.Port.Size = new System.Drawing.Size(94, 20);
            this.Port.TabIndex = 17;
            this.Port.Value = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            // 
            // IP0
            // 
            this.IP0.Location = new System.Drawing.Point(132, 6);
            this.IP0.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.IP0.Name = "IP0";
            this.IP0.Size = new System.Drawing.Size(46, 20);
            this.IP0.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(268, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Port Number Will Be The Same As The Global Settings!";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "IP4 Address To Bind To";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(388, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 26;
            this.button1.Text = "Get Local IP";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.GetLocalIP);
            // 
            // GenerateMonitorTap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 107);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.IP3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.IP2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.IP1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.Port);
            this.Controls.Add(this.IP0);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "GenerateMonitorTap";
            this.ShowIcon = false;
            this.Text = "GenerateMonitorTap";
            this.Load += new System.EventHandler(this.OnLoad);
            ((System.ComponentModel.ISupportInitialize)(this.IP3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IP2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IP1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Port)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IP0)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown IP3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown IP2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown IP1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.NumericUpDown Port;
        private System.Windows.Forms.NumericUpDown IP0;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}