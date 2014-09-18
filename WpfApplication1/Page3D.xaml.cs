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
        public Page3D(ParametricSurface surface)
            : base(GraphicModes.R3, 3)
        {
            InitializeComponent();
            InputNumberForm parameterDialog = new InputNumberForm(surface.parameters);
            if (parameterDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Read the contents of testDialog's TextBox.
                surface.parameters = parameterDialog.result;
            }
            else
            {
                
            }
            
            parallelTriangles = surface.triangulate(5, 5);
            renderTriangles = surface.triangulate(5, 5);
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
            intializeSizes();
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
