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
        public Page4D(ParametricSurface surface) : base(GraphicModes.R4, 4, surface)
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
                viewportManagers[i].generate_viewport_object(ViewPortGraphics.project4DTrianglesTo3D(renderTriangles, i));
            }
            base.canvas = this.canvas;
            base.canvasBorder = this.canvasBorder;
            base.viewportsBorder = this.viewportsBorder;
            intializeSizes();

        }
        public override void reRender(int who = 2)
        {
            switch (who)
            {
                case 0:
                    {
                        System.Windows.Media.Media3D.Transform3D trans = viewportManagers[0].getCurrentTransform();
                        for (int i = 0; i < 4; i++)
                        {
                            viewportManagers[i].reset();
                            viewportManagers[i].generate_3d_axes(100);
                            renderTriangles = surface.triangulate(Properties.Settings.Default.renderResolution, Properties.Settings.Default.renderResolution);
                            viewportManagers[i].generate_viewport_object(ViewPortGraphics.project4DTrianglesTo3D(renderTriangles, i));
                            viewportManagers[i].performTransform(ViewPortGraphics.convert4DTransformTo3D(currentTransform,i));
                        }
                        break;
                    }
                case 1:
                    {
                        parallelTriangles = surface.triangulate(Properties.Settings.Default.parallelResolution, Properties.Settings.Default.parallelResolution);
                        canvasManager = new CanvasGraphics(canvas, xCoordinateRange, yCoordinateRange, 4, parallelTriangles, Properties.Settings.Default.pointSize);
                        canvasManager.reDraw(currentTransform);
                        break;
                    }
                default:
                    {
                        this.reRender(0);
                        this.reRender(1);
                        break;
                    }
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
