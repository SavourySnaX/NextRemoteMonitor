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
    public partial class HexDecimalValue : UserControl
    {
        public HexDecimalValue()
        {
            InitializeComponent();
        }

        public event EventHandler ValueUpdated;

        public void SetValue(int value)
        {
            _value = value;
            comboBox1.Items[0] = String.Format("{0}", value);
            comboBox1.Items[1] = String.Format($"${{0:X{ValueSize}}}", value);
            comboBox1.SelectedItem = comboBox1.Items[_showHex?1:0];
        }

        int _value = 65535;
        public int Value { get { return _value; } }

        bool _showHex;
        public bool ShowHex
        {
            get
            {
                return _showHex;
            }
            set
            {
                _showHex = value;
                SetValue(_value);
            }
        }

        public int ValueSize
        {
            get; set;
        } = 4;

        private void KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar==13)
            {
                ValidateAndSet(comboBox1.Text);
            }

        }

        private void ValidateAndSet(string enteredValue)
        {
            if (enteredValue.Length < 1)
            {
                SetValue(_value);
            }

            if (enteredValue[0] == '$')
            {
                if (int.TryParse(enteredValue.TrimStart('$'), System.Globalization.NumberStyles.HexNumber, null, out int newValue))
                {
                    SetValue(newValue);
                    ValueUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    SetValue(_value);
                }
            }
            else
            {
                if (int.TryParse(enteredValue, out int newValue))
                {
                    SetValue(newValue);
                    ValueUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    SetValue(_value);
                }
            }
        }

        private void IndexChanged(object sender, EventArgs e)
        {
            _showHex = (comboBox1.SelectedItem == comboBox1.Items[1]);
        }

        private void LeftFocus(object sender, EventArgs e)
        {
            ValidateAndSet(comboBox1.Text);
        }
    }
}
