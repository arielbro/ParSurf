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
        public System.Windows.Media.SolidColorBrush frontColor;
        public System.Windows.Media.SolidColorBrush backColor;
        BrushPickerComboControl brushPicker1;
        BrushPickerComboControl brushPicker2;
        public InputColorForm(SolidColorBrush currentFrontColor, SolidColorBrush currentBackColor)
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

            this.label2.BackColor = System.Drawing.Color.FromArgb(currentFrontColor.Color.A, currentFrontColor.Color.R, 
                                                                    currentFrontColor.Color.G, currentFrontColor.Color.B);
            this.label1.BackColor = System.Drawing.Color.FromArgb(currentBackColor.Color.A, currentBackColor.Color.R, 
                                                                    currentBackColor.Color.G, currentBackColor.Color.B);
            brushPicker2.SelectedBrush = currentFrontColor;
            brushPicker1.SelectedBrush = currentBackColor;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            frontColor = brushPicker1.SelectedBrush as SolidColorBrush;
            backColor = brushPicker2.SelectedBrush as SolidColorBrush;
            this.DialogResult = DialogResult.OK;
        }

        public void brushPicker1_ColorChanged(Object sender, PropertyChangedEventArgs e)
        {
            System.Windows.Media.Color color = (brushPicker1.SelectedBrush as SolidColorBrush).Color;
            label1.BackColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            this.frontColor = (brushPicker1.SelectedBrush as SolidColorBrush);
        }
        public void brushPicker2_ColorChanged(Object sender, PropertyChangedEventArgs e)
        {
            System.Windows.Media.Color color = (brushPicker2.SelectedBrush as SolidColorBrush).Color;
            label2.BackColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            this.backColor = (brushPicker1.SelectedBrush as SolidColorBrush);
        }

    }
}