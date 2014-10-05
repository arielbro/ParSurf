using System;
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
    public partial class Page4D : GraphicsPage
    {
        public Page4D(Surface surface)
            : base(GraphicModes.R4, 4, surface)
        {
            InitializeComponent();
            parallelTriangles = surface.triangulate(Properties.Settings.Default.parallelResolution, Properties.Settings.Default.parallelResolution);
            renderTriangles = surface.triangulate(Properties.Settings.Default.renderResolution, Properties.Settings.Default.renderResolution);
            canvasManager = new CanvasGraphics(canvas, xCoordinateRange, yCoordinateRange, 4, parallelTriangles, Properties.Settings.Default.pointSize);
            canvasManager.reDraw(currentTransform);
            viewports = new Viewport3D[] { viewport1, viewport2, viewport3, viewport4 };
            viewportManagers = new ViewPortGraphics[] { new ViewPortGraphics(viewport1), new ViewPortGraphics(viewport2),
                                                        new ViewPortGraphics(viewport3), new ViewPortGraphics(viewport4)};
            viewportsmDown = new bool[4];
            viewportsmLastPos = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                //project the triangles to the 
                viewportManagers[i].generate_3d_axes(100);
                viewportManagers[i].generate_viewport_object(ViewPortGraphics.project4DTrianglesTo3D(renderTriangles, i),
                                                             renderingFrontColor, renderingBackColor, renderingOpacity);
            }
            base.canvas = this.canvas;
            base.canvasBorder = this.canvasBorder;
            base.viewportsBorder = this.viewportsBorder;
            intializeSizes();

        }
        public override void reRender(ReRenderingModes who = ReRenderingModes.Both)
        {
            if (who == ReRenderingModes.Viewport || who == ReRenderingModes.Both)
            {
                for (int i = 0; i < 4; i++)
                {
                    viewportManagers[i].reset();
                    viewportManagers[i].generate_3d_axes(100);
                    renderTriangles = surface.triangulate(renderResolution, renderResolution);
                    viewportManagers[i].generate_viewport_object(ViewPortGraphics.project4DTrianglesTo3D(renderTriangles, i),
                                                                 renderingFrontColor, renderingBackColor, renderingOpacity);
                    viewportManagers[i].performTransform(ViewPortGraphics.convert4DTransformTo3D(currentTransform, i));
                }
            }
            if (who == ReRenderingModes.Canvas || who == ReRenderingModes.Both)
            {
                parallelTriangles = surface.triangulate(parallelResolution, parallelResolution);
                canvasManager = new CanvasGraphics(canvas, xCoordinateRange, yCoordinateRange, 4, parallelTriangles, Properties.Settings.Default.pointSize);
                canvasManager.reDraw(currentTransform);
            }

        }
        public void intializeSizes()
        {
            viewport1Border.BorderThickness = new Thickness(0, 0, 2, 2);
            viewport2Border.BorderThickness = new Thickness(2, 0, 0, 2);
            viewport3Border.BorderThickness = new Thickness(0, 2, 2, 0);
            viewport4Border.BorderThickness = new Thickness(2, 2, 0, 0);
            canvasBorder.Height = this.ActualHeight;
            canvasBorder.Width = this.ActualWidth / 2;
            viewportsBorder.Height = this.ActualHeight;
            viewportsBorder.Width = this.ActualWidth / 2;
            foreach (Border bord in ((Grid)viewportsBorder.Child).Children)
            {
                bord.Height = this.ActualHeight / 2;
                bord.Width = this.ActualWidth / 4;
            }
        }
    }
}
