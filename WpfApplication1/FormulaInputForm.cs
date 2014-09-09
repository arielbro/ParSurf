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
    public partial class FormulaInputForm : Form
    {
        public String XFormula;
        public String YFormula;
        public String ZFormula;

        public FormulaInputForm()
        {
            InitializeComponent();
        }
        private void InputNumberForm_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            XFormula = XTextBox.Text;
            YFormula = YTextBox.Text;
            ZFormula = ZTextBox.Text;
        }
        public void setFormulas(String[] formulas)
        {
            XTextBox.Text = formulas[0];
            YTextBox.Text = formulas[1];
            ZTextBox.Text = formulas[2];
        }
    }
}
