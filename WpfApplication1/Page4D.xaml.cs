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
        public Page4D(IList<double [][]> paralleltriangles, IList<double[][]> renderTriangles) : base(GraphicModes.R4, 4)
        {
            InitializeComponent();
            canvasManager = new CanvasGraphics(canvas, xCoordinateRange, yCoordinateRange, 4, paralleltriangles);
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
        }

    }
}
