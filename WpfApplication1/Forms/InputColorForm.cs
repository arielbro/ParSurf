using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;

namespace ParSurf
{
    public partial class InputColorForm : Form
    {
        public System.Windows.Media.Color firstColor;
        public System.Windows.Media.Color secondColor;
        BrushPickerComboControl brushPicker1;
        BrushPickerComboControl brushPicker2;
        public InputColorForm(System.Windows.Media.Color currentFirstColor, System.Windows.Media.Color currentSecondColor,
                                string label1Text, string label2Text)
        {
            InitializeComponent();
            brushPicker1 = new BrushPickerComboControl();
            brushPicker2 = new BrushPickerComboControl();
            elementHost1.Child = brushPicker1;
            elementHost2.Child = brushPicker2;
            brushPicker1.ColorChanged += brushPicker1_ColorChanged;
            brushPicker2.ColorChanged += brushPicker2_ColorChanged;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.AcceptButton = buttonOk;
            this.CancelButton = buttonCancel;
            buttonCancel.DialogResult = DialogResult.Cancel;

            this.label2.Text = label2Text;
            this.label1.Text = label1Text;
            this.label1.BackColor = System.Drawing.Color.FromArgb(currentFirstColor.A, currentFirstColor.R, 
                                                                    currentFirstColor.G, currentFirstColor.B);
            this.label2.BackColor = System.Drawing.Color.FromArgb(currentSecondColor.A, currentSecondColor.R, 
                                                                    currentSecondColor.G, currentSecondColor.B);
            brushPicker2.SelectedBrush = new SolidColorBrush(currentSecondColor);
            brushPicker1.SelectedBrush = new SolidColorBrush(currentFirstColor);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            firstColor = (brushPicker1.SelectedBrush as SolidColorBrush).Color;
            secondColor = (brushPicker2.SelectedBrush as SolidColorBrush).Color;
            this.DialogResult = DialogResult.OK;
        }

        public void brushPicker1_ColorChanged(Object sender, PropertyChangedEventArgs e)
        {
            System.Windows.Media.Color color = (brushPicker1.SelectedBrush as SolidColorBrush).Color;
            label1.BackColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            this.firstColor = (brushPicker1.SelectedBrush as SolidColorBrush).Color;
        }
        public void brushPicker2_ColorChanged(Object sender, PropertyChangedEventArgs e)
        {
            System.Windows.Media.Color color = (brushPicker2.SelectedBrush as SolidColorBrush).Color;
            label2.BackColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            this.secondColor = (brushPicker2.SelectedBrush as SolidColorBrush).Color;
        }

    }
}