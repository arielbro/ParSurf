﻿using System;
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
        public List<Tuple<int, int, int>> originalPLanePointsShown;//structure - Pi_ijk - X[i][j][k][0], Pi'_ijk - X[i][j][k][1]
        public List<Tuple<int, int, int>> transposedPLanePointsShown;
        [NonSerializedAttribute()]
        public Color originalPlanePointsColor = Properties.Settings.Default.originalPlanePointsColor;
        [NonSerializedAttribute()]
        public Color transposedPlanePointsColor = Properties.Settings.Default.transposedPlanePointsColor;
        public bool isPlanePointsColoringGradient = Properties.Settings.Default.isPlanePointsColoringGradient;
        public bool isPlanePointsColoringArbitrary = Properties.Settings.Default.isPlanePointsColoringArbitrary;
        public float[] backArgb;
        public float[] frontArgb;
        public float[] originalPlanePointsColorArgb;
        public float[] transposedPlanePointsColorArgb;
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
            originalPlanePointsColor = new Color();
            originalPlanePointsColor.ScA = originalPlanePointsColorArgb[0];
            originalPlanePointsColor.ScR = originalPlanePointsColorArgb[1];
            originalPlanePointsColor.ScG = originalPlanePointsColorArgb[2];
            originalPlanePointsColor.ScB = originalPlanePointsColorArgb[3];
            transposedPlanePointsColor = new Color();
            transposedPlanePointsColor.ScA = transposedPlanePointsColorArgb[0];
            transposedPlanePointsColor.ScR = transposedPlanePointsColorArgb[1];
            transposedPlanePointsColor.ScG = transposedPlanePointsColorArgb[2];
            transposedPlanePointsColor.ScB = transposedPlanePointsColorArgb[3];
        }
        public void saveColors(){
            backArgb = new float[4] { renderingBackColor.ScA, renderingBackColor.ScR, renderingBackColor.ScG, renderingBackColor.ScB };
            frontArgb = new float[4] { renderingFrontColor.ScA, renderingFrontColor.ScR, renderingFrontColor.ScG, renderingFrontColor.ScB };
            originalPlanePointsColorArgb = new float[4] { originalPlanePointsColor.ScA, originalPlanePointsColor.ScR, originalPlanePointsColor.ScG, originalPlanePointsColor.ScB };
            transposedPlanePointsColorArgb = new float[4] { transposedPlanePointsColor.ScA, transposedPlanePointsColor.ScR, transposedPlanePointsColor.ScG, transposedPlanePointsColor.ScB };
        }
    }
}
