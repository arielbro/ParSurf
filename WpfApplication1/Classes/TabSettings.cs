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
        public Color renderingFrontColor = Properties.Settings.Default.frontColor;
        public Color renderingBackColor = Properties.Settings.Default.backColor;
        public double renderingOpacity = Properties.Settings.Default.renderingOpacity;
    }
}
