namespace SimpleMonitor.DockableWindows
{
    partial class DisassemblyView
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
            this.Disassembly = new System.Windows.Forms.TextBox();
            this.BankOffset = new System.Windows.Forms.NumericUpDown();
            this.BankNum = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.BankOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BankNum)).BeginInit();
            this.SuspendLayout();
            // 
            // Disassembly
            // 
            this.Disassembly.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Disassembly.Location = new System.Drawing.Point(12, 33);
            this.Disassembly.Multiline = true;
            this.Disassembly.Name = "Disassembly";
            this.Disassembly.ReadOnly = true;
            this.Disassembly.Size = new System.Drawing.Size(776, 405);
            this.Disassembly.TabIndex = 0;
            // 
            // BankOffset
            // 
            this.BankOffset.Location = new System.Drawing.Point(163, 7);
            this.BankOffset.Maximum = new decimal(new int[] {
            8191,
            0,
            0,
            0});
            this.BankOffset.Name = "BankOffset";
            this.BankOffset.Size = new System.Drawing.Size(94, 20);
            this.BankOffset.TabIndex = 8;
            this.BankOffset.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // BankNum
            // 
            this.BankNum.Location = new System.Drawing.Point(111, 7);
            this.BankNum.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.BankNum.Name = "BankNum";
            this.BankNum.Size = new System.Drawing.Size(46, 20);
            this.BankNum.TabIndex = 7;
            this.BankNum.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Bank | Start Offset";
            // 
            // DisassemblyView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BankOffset);
            this.Controls.Add(this.BankNum);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Disassembly);
            this.Name = "DisassemblyView";
            this.Text = "Disassembly";
            ((System.ComponentModel.ISupportInitialize)(this.BankOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BankNum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Disassembly;
        private System.Windows.Forms.NumericUpDown BankOffset;
        private System.Windows.Forms.NumericUpDown BankNum;
        private System.Windows.Forms.Label label2;
    }
}