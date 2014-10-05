using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace ParSurf
{
    public partial class InputMatrixForm : Form
    {
        int matrixDimension;
        TextBox textBox;
        public double[][] result;

        public InputMatrixForm(int matrixDimension, double[][] currentMatrix = null)
        {
            InitializeComponent();
            this.AcceptButton = buttonOk;
            this.CancelButton = buttonCancel;
            buttonCancel.DialogResult = DialogResult.Cancel;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.matrixDimension = matrixDimension;
            textBox = new TextBox();
            textBox.Multiline = true;
            textBox.Dock = DockStyle.Fill;
            panel1.Controls.Add(textBox);
            textBox.Focus();
            textBox.ScrollBars = ScrollBars.Both;
            textBox.AcceptsReturn = true;


            if (currentMatrix != null)
            {
                string representation = "";
                for (int i = 0; i < currentMatrix.Length; i++)
                {
                    for (int j = 0; j < currentMatrix.Length; j++)
                    {
                        representation += string.Format("{0:0.##}", currentMatrix[i][j]);
                        if (j != currentMatrix.Length - 1)
                            representation += ",\t";
                    }
                    if (i != currentMatrix.Length - 1)
                        representation += System.Environment.NewLine;
                }
                textBox.Text = representation;
            }
        }
        private void InputNumberForm_Load(object sender, EventArgs e)
        {
        }
        private void buttonOk_Click(object sender, EventArgs e)
        {
            try
            {
                result = new double[matrixDimension][];
                string[] splittedInput = textBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedInput.Length != matrixDimension)
                    throw new Exception();

                for (int i = 0; i < matrixDimension; i++)
                {
                    result[i] = new double[matrixDimension];
                    string[] splittedLine = Regex.Replace(splittedInput[i], @"\s+", "").Split(
                                                          new string[]{","},StringSplitOptions.RemoveEmptyEntries);
                    if (splittedLine.Length != matrixDimension)
                        throw new Exception();
                       
                    //numbers should be able to include Pi and e, so NCalc will handle the representing text
                    for (int j = 0; j < matrixDimension; j++)
                    {
                        NCalc.Expression exp = new NCalc.Expression(splittedLine[j]);
                        exp.Parameters["Pi"] = Math.PI;
                        exp.Parameters["pi"] = Math.PI;
                        exp.Parameters["e"] = Math.E;
                        exp.Parameters["E"] = Math.E;
                        result[i][j] = Convert.ToDouble(exp.Evaluate());
                    }
                }
                this.DialogResult = DialogResult.OK;
            }
            catch
            {
                MessageBox.Show("Invalid format for matrix of size " + matrixDimension + "X" + matrixDimension);
            }


        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
