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
    public partial class Page3D : GraphicsPage
    {
        public Page3D(IList<double[][]> parallelTriangles, IList<double[][]> renderTriangles)
            : base(GraphicModes.R3, 3)
        {
            InitializeComponent();
            canvasManager = new CanvasGraphics(canvas, xCoordinateRange, yCoordinateRange, 3, parallelTriangles);
            canvasManager.reDraw(currentTransform);
            viewports = new Viewport3D[] { viewport };
            viewportManagers = new ViewPortGraphics[] { new ViewPortGraphics(viewport) };
            viewportsmDown = new bool[1];
            viewportsmLastPos = new Point[1];
            viewportManagers[0].generate_3d_axes(100);
            viewportManagers[0].generate_viewport_object(renderTriangles);
            viewportsBorder = viewportBorder;
            base.canvas = this.canvas;
            base.canvasBorder = this.canvasBorder;
            base.viewportsBorder = this.viewportBorder;
        }
    }
}
