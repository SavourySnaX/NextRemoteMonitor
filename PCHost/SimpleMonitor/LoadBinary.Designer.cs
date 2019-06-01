namespace SimpleMonitor
{
    partial class LoadBinary
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
            this.label1 = new System.Windows.Forms.Label();
            this.FilePath = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.BankNum = new System.Windows.Forms.NumericUpDown();
            this.BankOffset = new System.Windows.Forms.NumericUpDown();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.BankNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BankOffset)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Binary To Load";
            // 
            // FilePath
            // 
            this.FilePath.Location = new System.Drawing.Point(97, 12);
            this.FilePath.Name = "FilePath";
            this.FilePath.Size = new System.Drawing.Size(160, 20);
            this.FilePath.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(263, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(26, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ChooseFile);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Bank | Start Offset";
            // 
            // BankNum
            // 
            this.BankNum.Location = new System.Drawing.Point(111, 43);
            this.BankNum.Maximum = new decimal(new int[] {
            110,
            0,
            0,
            0});
            this.BankNum.Name = "BankNum";
            this.BankNum.Size = new System.Drawing.Size(46, 20);
            this.BankNum.TabIndex = 4;
            // 
            // BankOffset
            // 
            this.BankOffset.Location = new System.Drawing.Point(163, 43);
            this.BankOffset.Maximum = new decimal(new int[] {
            8191,
            0,
            0,
            0});
            this.BankOffset.Name = "BankOffset";
            this.BankOffset.Size = new System.Drawing.Size(94, 20);
            this.BankOffset.TabIndex = 5;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(214, 135);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "&Ok";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OK);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(133, 135);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 7;
            this.button3.Text = "&Cancel";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Cancel);
            // 
            // LoadBinary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 166);
            this.ControlBox = false;
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.BankOffset);
            this.Controls.Add(this.BankNum);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.FilePath);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoadBinary";
            this.ShowIcon = false;
            this.Text = "Upload Binary Data";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.BankNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BankOffset)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FilePath;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown BankNum;
        private System.Windows.Forms.NumericUpDown BankOffset;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}