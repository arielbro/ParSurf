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
        public List<Tuple<int, int, int>> originalPLanePointsShown;//structure - Pi_ijk - X[i][j][k][0], Pi'_ijk - X[i][j][k][1]
        public List<Tuple<int, int, int>> transposedPLanePointsShown;
        public Color originalPlanePointsColor = Properties.Settings.Default.originalPlanePointsColor;
        public Color transposedPlanePointsColor = Properties.Settings.Default.transposedPlanePointsColor;
        public bool isPlanePointsColoringGradient = Properties.Settings.Default.isPlanePointsColoringGradient;
        public bool isPlanePointsColoringArbitrary = Properties.Settings.Default.isPlanePointsColoringArbitrary;
    }
}
