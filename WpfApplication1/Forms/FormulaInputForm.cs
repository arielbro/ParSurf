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
        public List<String> formulas;
        //public List<String> ranges;
        public string[] urange;
        public string[] vrange;
        public string name;
        //int dimension = 3;
        public FormulaInputForm()
        {
            InitializeComponent();
        }
    

        private void button1_Click_1(object sender, EventArgs e)
        {
            formulas = new List<String>();
            for (int i = 1; i < dimension + 1; i++)
            {
                formulas.Add((this.Controls.Find("DimForm" + i, false))[0].Text);
            }
            urange = new string[]{textBox1.Text, textBox2.Text};
            vrange = new string[] { textBox3.Text, textBox4.Text };
            name = textBoxName.Text;
        }
        public void setFormulas(String[] formula,string name,string[] urange,string[] vrange)
        {
            textBoxName.Text = name;
            textBox1.Text = urange[0];
            textBox2.Text = urange[1];
            textBox3.Text = vrange[0];
            textBox4.Text = vrange[1];
            DimForm1.Text = formula[0];
            DimForm2.Text = formula[1];
            DimForm3.Text = formula[2];
            for (int i = 3; i < formula.Length; i++)
            {
                button3_Click(new object(), new EventArgs());
                ((this.Controls.Find("DimForm" + (i + 1), false))[0]).Text = formula[i];
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dimension++;
            foreach (Control item in this.Controls)
            {
                if (item.Location.Y > 122+((dimension-4)*29))
                    item.Location = new Point(item.Location.X,item.Location.Y+29);
            }
            Label axislabel = new Label();
            axislabel.Size = AxisLabel1.Size;
            axislabel.Font = AxisLabel1.Font;
            axislabel.Text = "X" + dimension+"(u,v):";
            axislabel.AutoSize = true;
            axislabel.Location = new Point(12, 64 + ((dimension - 1) * 29));
            axislabel.Name = "AxisLabel" + dimension;
            this.Controls.Add(axislabel);
            TextBox dimform = new TextBox();
            dimform.Location = new Point(70, 64 + ((dimension - 1) * 29));
            dimform.Size = DimForm1.Size;
            dimform.Font = DimForm1.Font;
            dimform.Name = "DimForm" + dimension;
            this.Controls.Add(dimform);
            dimform.BringToFront();
            this.Height += 29;
        }

    }
}
