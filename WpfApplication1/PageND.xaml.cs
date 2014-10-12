﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ParSurf
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    [Serializable()]
    public partial class PageND : GraphicsPage
    {

        public PageND(Surface surface, int dimension, bool paramAsk = true)
            : base(GraphicModes.Rn, dimension, surface,paramAsk)
        {
            InitializeComponent();
            parallelTriangles = surface.triangulate(Properties.Settings.Default.parallelResolution, Properties.Settings.Default.parallelResolution, this);
            renderTriangles = surface.triangulate(Properties.Settings.Default.renderResolution, Properties.Settings.Default.renderResolution, this);
            canvasManager = new CanvasGraphics(canvas, xCoordinateRange, yCoordinateRange, dimension, parallelTriangles,
                                                settings.originalPLanePointsShown, settings.transposedPLanePointsShown, settings.pointSize);
            applyParallelColorScheme(settings.originalPlanePointsColor, settings.transposedPlanePointsColor,
                         settings.isPlanePointsColoringGradient, settings.isPlanePointsColoringArbitrary);
//            canvasManager.reDraw(currentTransform, parallelTriangles);
            viewports = new Viewport3D[] { viewport };
            viewportManagers = new ViewPortGraphics[] { new ViewPortGraphics(viewport) };
            viewportsmDown = new bool[1];
            viewportsmLastPos = new Point[1];
            currentAxes = new int[] { 0, 1, 2 };
            viewportManagers[0].generate_3d_axes(100);
            viewportManagers[0].generate_viewport_object(ViewPortGraphics.projectNDTrianglesTo3D(renderTriangles, currentAxes),
                                                         settings.renderingFrontColor, settings.renderingBackColor, settings.renderingOpacity);
            viewportsBorder = viewportBorder;
            base.canvas = this.canvas;
            base.canvasBorder = this.canvasBorder;
            base.viewportsBorder = this.viewportBorder;
            this.renderTriangles = renderTriangles;
            intializeSizes();
        }
        public PageND(Surface surface,double[][] currentTrans, TabSettings settings) : this(surface,surface.dimension,false)
        {
            currentTransform = currentTrans;
            this.settings = settings;
//            reRender(ReRenderingModes.Both);
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int[] requestedAxes = new int[3];
            try
            {
                requestedAxes[0] = int.Parse(textBox1.Text);
                requestedAxes[1] = int.Parse(textBox2.Text);
                requestedAxes[2] = int.Parse(textBox3.Text);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error parsing axes numbers. Input is required to be an integer");
                return;
            }
            foreach (int axisNumber in requestedAxes)
                if (axisNumber < 0 || axisNumber > dimension - 1)
                {
                    MessageBox.Show("Error parsing axes numbers. Numbers should be in range 0-" + (dimension - 1).ToString());
                    return;
                }
            if (requestedAxes[0] == requestedAxes[1] || requestedAxes[1] == requestedAxes[2] || requestedAxes[2] == requestedAxes[0])
            {
                MessageBox.Show("Error parsing axes numbers. A number cannot appear twice");
                return;
            }
            currentAxes[0] = requestedAxes[0];
            currentAxes[1] = requestedAxes[1];
            currentAxes[2] = requestedAxes[2];
            viewportManagers[0].reset();
            viewportManagers[0].generate_viewport_object(ViewPortGraphics.projectNDTrianglesTo3D(renderTriangles, currentAxes),
                                                         settings.renderingFrontColor, settings.renderingBackColor, settings.renderingOpacity,
                                                         ViewPortGraphics.convertNDTransformTo3D(currentTransform, currentAxes));
        }
        public override void reRender(ReRenderingModes who = ReRenderingModes.Both)
        {
            if (who == ReRenderingModes.Viewport || who == ReRenderingModes.Both)
            {
                System.Windows.Media.Media3D.Transform3D trans = viewportManagers[0].getCurrentTransform();
                viewportManagers[0].reset();
                viewportManagers[0].generate_3d_axes(100);
                renderTriangles = surface.triangulate(settings.renderResolution, settings.renderResolution, this);
                viewportManagers[0].generate_viewport_object(renderTriangles, settings.renderingFrontColor, settings.renderingBackColor, settings.renderingOpacity);
                viewportManagers[0].performTransform(ViewPortGraphics.convertNDTransformTo3D(currentTransform, currentAxes));
            }
            if (who == ReRenderingModes.Canvas || who == ReRenderingModes.Both)
            {
                canvasManager.pointSize = settings.pointSize;
                parallelTriangles = surface.triangulate(settings.parallelResolution, settings.parallelResolution, this);
                canvasManager.reDraw(currentTransform, parallelTriangles);
            }

        }
        public void intializeSizes()
        {
            canvasBorder.Height = this.ActualHeight;
            canvasBorder.Width = this.ActualWidth / 2;
            viewportBorder.Height = this.ActualHeight;
            viewportBorder.Width = this.ActualWidth / 2;
        }

    }
}
