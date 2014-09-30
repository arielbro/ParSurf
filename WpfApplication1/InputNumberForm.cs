﻿using System;
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
        public Dictionary<string,double> result;
        private int paramNum;
        public InputNumberForm()
        {
            InitializeComponent();
        }
        public InputNumberForm(Dictionary<string,double> parameters)
        {
            InitializeComponent();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            int i = 0;
            foreach (KeyValuePair<string, double> param in parameters)
            {
                if (Double.IsNaN(param.Value))
                {
                    Label newlabel = new Label();
                    TextBox newtextbox = new TextBox();
                    newlabel.Text = param.Key + ":";
                    newlabel.Name = "label" + i;
                    newtextbox.Text = param.Value.ToString();
                    newtextbox.Name = "textbox" + i;
                    newlabel.SetBounds(9, 18 + i * 56, 12, 13);
                    newtextbox.SetBounds(12, 36 + i * 56, 200, 20);
                    newlabel.AutoSize = true;
                    newtextbox.Anchor = newtextbox.Anchor | AnchorStyles.Right;
                    this.Controls.AddRange(new Control[] { newlabel, newtextbox });
                    i++;
                }
            }
            paramNum = i;
            buttonOk.Text = "OK";
            buttonOk.Click += buttonOk_Click;
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonOk.SetBounds(28, 10+i*56, 75, 23);
            buttonCancel.SetBounds(109, 10+i*56, 75, 23);
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Size = new Size(250,(i+1)*56+30); 
            this.Controls.AddRange(new Control[] { buttonOk, buttonCancel });
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.AcceptButton = buttonOk;
            this.CancelButton = buttonCancel;

        }
        private void InputNumberForm_Load(object sender, EventArgs e)
        {
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            result= new Dictionary<string,double>();
            for (int i = 0; i < paramNum; i++)
            {
                //Parameters should be able to include Pi and e, so NCalc will handle the representing text
                NCalc.Expression exp = new NCalc.Expression(this.Controls.Find("textbox" + i, false)[0].Text);
                exp.Parameters["Pi"]=Math.PI;
                exp.Parameters["pi"]=Math.PI;
                exp.Parameters["e"]=Math.E;
                exp.Parameters["E"]=Math.E;
                result.Add(this.Controls.Find("label" + i,false)[0].Text.Replace(":",""), Convert.ToDouble(exp.Evaluate()));
            }
        }
    }
}
