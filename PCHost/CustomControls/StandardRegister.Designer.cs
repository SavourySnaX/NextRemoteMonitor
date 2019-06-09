namespace CustomControls
{
    partial class StandardRegister
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labLowReg = new System.Windows.Forms.Label();
            this.labHighReg = new System.Windows.Forms.Label();
            this.labRegPair = new System.Windows.Forms.Label();
            this.valueLow = new CustomControls.HexDecimalValue();
            this.valueHigh = new CustomControls.HexDecimalValue();
            this.valuePair = new CustomControls.HexDecimalValue();
            this.SuspendLayout();
            // 
            // labLowReg
            // 
            this.labLowReg.AutoSize = true;
            this.labLowReg.Location = new System.Drawing.Point(160, 1);
            this.labLowReg.Name = "labLowReg";
            this.labLowReg.Size = new System.Drawing.Size(21, 13);
            this.labLowReg.TabIndex = 7;
            this.labLowReg.Text = "PC";
            this.labLowReg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labHighReg
            // 
            this.labHighReg.AutoSize = true;
            this.labHighReg.Location = new System.Drawing.Point(116, 1);
            this.labHighReg.Name = "labHighReg";
            this.labHighReg.Size = new System.Drawing.Size(21, 13);
            this.labHighReg.TabIndex = 8;
            this.labHighReg.Text = "PC";
            this.labHighReg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labRegPair
            // 
            this.labRegPair.AutoSize = true;
            this.labRegPair.Location = new System.Drawing.Point(3, 21);
            this.labRegPair.Name = "labRegPair";
            this.labRegPair.Size = new System.Drawing.Size(21, 13);
            this.labRegPair.TabIndex = 5;
            this.labRegPair.Text = "PC";
            this.labRegPair.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // valueLow
            // 
            this.valueLow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.valueLow.Location = new System.Drawing.Point(152, 17);
            this.valueLow.Name = "valueLow";
            this.valueLow.ShowHex = false;
            this.valueLow.Size = new System.Drawing.Size(42, 22);
            this.valueLow.TabIndex = 10;
            this.valueLow.ValueSize = 2;
            this.valueLow.ValueUpdated += new System.EventHandler(this.LowChanged);
            // 
            // valueHigh
            // 
            this.valueHigh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.valueHigh.Location = new System.Drawing.Point(106, 17);
            this.valueHigh.Name = "valueHigh";
            this.valueHigh.ShowHex = false;
            this.valueHigh.Size = new System.Drawing.Size(42, 22);
            this.valueHigh.TabIndex = 9;
            this.valueHigh.ValueSize = 2;
            this.valueHigh.ValueUpdated += new System.EventHandler(this.HighChanged);
            // 
            // valuePair
            // 
            this.valuePair.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.valuePair.Location = new System.Drawing.Point(36, 17);
            this.valuePair.Name = "valuePair";
            this.valuePair.ShowHex = false;
            this.valuePair.Size = new System.Drawing.Size(59, 22);
            this.valuePair.TabIndex = 6;
            this.valuePair.ValueSize = 4;
            this.valuePair.ValueUpdated += new System.EventHandler(this.PairChanged);
            // 
            // StandardRegister
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labLowReg);
            this.Controls.Add(this.labHighReg);
            this.Controls.Add(this.valueLow);
            this.Controls.Add(this.valueHigh);
            this.Controls.Add(this.valuePair);
            this.Controls.Add(this.labRegPair);
            this.Name = "StandardRegister";
            this.Size = new System.Drawing.Size(197, 43);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labLowReg;
        private System.Windows.Forms.Label labHighReg;
        private HexDecimalValue valueLow;
        private HexDecimalValue valueHigh;
        private HexDecimalValue valuePair;
        private System.Windows.Forms.Label labRegPair;
    }
}
