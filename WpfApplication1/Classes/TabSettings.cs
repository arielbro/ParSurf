using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
namespace ParSurf
{
    [Serializable()]
    public class TabSettings
    {
        public int parallelResolution = Properties.Settings.Default.parallelResolution;
        public int renderResolution = Properties.Settings.Default.renderResolution;
        public double pointSize = Properties.Settings.Default.pointSize;
        [NonSerializedAttribute()]
        public Color renderingFrontColor = Properties.Settings.Default.frontColor;
        [NonSerializedAttribute()]
        public Color renderingBackColor = Properties.Settings.Default.backColor;
        public double renderingOpacity = Properties.Settings.Default.renderingOpacity;
        public float[] backArgb;
        public float[] frontArgb;
        public TabSettings()
        {
        }
        public TabSettings(float[] backArgb, float[] frontArgb)
        {
            renderingBackColor = new Color();
            renderingBackColor.ScA = backArgb[0];
            renderingBackColor.ScR = backArgb[1];
            renderingBackColor.ScG = backArgb[2];
            renderingBackColor.ScB = backArgb[3];
            renderingFrontColor = new Color();
            renderingFrontColor.ScA = frontArgb[0];
            renderingFrontColor.ScR = frontArgb[1];
            renderingFrontColor.ScG = frontArgb[2];
            renderingFrontColor.ScB = frontArgb[3];
        }
        public void loadColors()
        {
            renderingBackColor = new Color();
            renderingBackColor.ScA = backArgb[0];
            renderingBackColor.ScR = backArgb[1];
            renderingBackColor.ScG = backArgb[2];
            renderingBackColor.ScB = backArgb[3];
            renderingFrontColor = new Color();
            renderingFrontColor.ScA = frontArgb[0];
            renderingFrontColor.ScR = frontArgb[1];
            renderingFrontColor.ScG = frontArgb[2];
            renderingFrontColor.ScB = frontArgb[3];
        }
        public void saveColors(){
            backArgb = new float[4] { renderingBackColor.ScA, renderingBackColor.ScR, renderingBackColor.ScG, renderingBackColor.ScB };
            frontArgb = new float[4] { renderingFrontColor.ScA, renderingFrontColor.ScR, renderingFrontColor.ScG, renderingFrontColor.ScB };
        }
    }
}
