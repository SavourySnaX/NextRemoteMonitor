using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomControls
{
    public partial class StandardRegister: UserControl
    {
        public StandardRegister()
        {
            InitializeComponent();
        }


        public void SetRegisterName(string pair,string hi,string lo)
        {
            labHighReg.Text = hi;
            labLowReg.Text = lo;
            labRegPair.Text = pair;
        }

        private int _value=0;
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                valuePair.SetValue(value);
                valueHigh.SetValue(value>>8);
                valueLow.SetValue(value&255);
            }
        }

        public bool ShowHex
        {
            set
            {
                valuePair.ShowHex = value;
                valueHigh.ShowHex = value;
                valueLow.ShowHex = value;
            }
        }

        private void PairChanged(object sender, EventArgs e)
        {
            int newHigh = valuePair.Value >> 8;
            int newLow = valuePair.Value & 255;

            if (valueLow.Value != newLow)
                valueLow.SetValue(newLow);
            if (valueHigh.Value != newHigh)
                valueHigh.SetValue(newHigh);
        }

        private void HighChanged(object sender, EventArgs e)
        {
            int newPair = (valuePair.Value & 255) | (valueHigh.Value << 8);

            if (valuePair.Value != newPair)
                valuePair.SetValue(newPair);
        }

        private void LowChanged(object sender, EventArgs e)
        {
            int newPair = (valuePair.Value & 0xFF00) | (valueLow.Value & 255);

            if (valuePair.Value != newPair)
                valuePair.SetValue(newPair);
        }

        private void valuePair_Load(object sender, EventArgs e)
        {

        }
    }
}
