using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ParSurf
{
    public partial class InputNumberForm : Form
    {
        public double result;

        public InputNumberForm()
        {
            InitializeComponent();
        }

        private void InputNumberForm_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
                result = Double.Parse(inputTextBox.Text);
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
