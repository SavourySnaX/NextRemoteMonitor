namespace SimpleMonitor.DockableWindows
{
    partial class MemoryView
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
            this.BankNum = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.hexBox = new Be.Windows.Forms.HexBox();
            ((System.ComponentModel.ISupportInitialize)(this.BankNum)).BeginInit();
            this.SuspendLayout();
            // 
            // BankNum
            // 
            this.BankNum.Location = new System.Drawing.Point(50, 7);
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
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Bank";
            // 
            // hexBox
            // 
            this.hexBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hexBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hexBox.ColumnInfoVisible = true;
            this.hexBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.hexBox.LineInfoVisible = true;
            this.hexBox.Location = new System.Drawing.Point(12, 33);
            this.hexBox.Name = "hexBox";
            this.hexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexBox.Size = new System.Drawing.Size(776, 405);
            this.hexBox.StringViewVisible = true;
            this.hexBox.TabIndex = 9;
            this.hexBox.UseFixedBytesPerLine = true;
            this.hexBox.VScrollBarVisible = true;
            // 
            // MemoryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.hexBox);
            this.Controls.Add(this.BankNum);
            this.Controls.Add(this.label2);
            this.Name = "MemoryView";
            this.Text = "MemoryView";
            ((System.ComponentModel.ISupportInitialize)(this.BankNum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.NumericUpDown BankNum;
        private System.Windows.Forms.Label label2;
        private Be.Windows.Forms.HexBox hexBox;
    }
}