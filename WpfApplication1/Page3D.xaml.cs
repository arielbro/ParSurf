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
using System.Windows.Media.Media3D;
using System.Windows.Threading;
namespace ParSurf
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    [Serializable()]
    public partial class Page3D : GraphicsPage
    {
        public Page3D(Surface surface, bool paramAsk = true)
            : base(GraphicModes.R3, 3,surface, paramAsk)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            InitializeComponent();
            parallelTriangles = surface.triangulate(settings.parallelResolution, settings.parallelResolution, this);
            renderTriangles = surface.triangulate(settings.renderResolution, settings.renderResolution, this);
            canvasManager = new CanvasGraphics(canvas, xCoordinateRange, yCoordinateRange, 3, parallelTriangles,
                                  settings.originalPLanePointsShown, settings.transposedPLanePointsShown, settings.pointSize);
            applyParallelColorScheme(settings.originalPlanePointsColor, settings.transposedPlanePointsColor, 
                                     settings.isPlanePointsColoringGradient, settings.isPlanePointsColoringArbitrary);
//            canvasManager.reDraw(currentTransform, parallelTriangles);
            viewports = new Viewport3D[] { viewport };
            viewportManagers = new ViewPortGraphics[] { new ViewPortGraphics(viewport) };
            viewportsmDown = new bool[1];
            viewportsmLastPos = new Point[1];
            viewportManagers[0].generate_3d_axes(100);
            viewportManagers[0].generate_viewport_object(renderTriangles, settings.renderingFrontColor, settings.renderingBackColor, settings.renderingOpacity);
            viewportsBorder = viewportBorder;
            base.canvas = this.canvas;
            base.canvasBorder = this.canvasBorder;
            base.viewportsBorder = this.viewportBorder;
            intializeSizes();
            Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.ApplicationIdle);
        }
        public Page3D(Surface surface,double[][] currentTrans, TabSettings settings) : this(surface,false)
        {
            currentTransform = currentTrans;
            this.settings = settings;
//            reRender(ReRenderingModes.Both);
        }
        // reRendering objects after settings change. who = 0 vieport, who = 1 canvas, who = 2 both.
        public override void reRender(ReRenderingModes who = ReRenderingModes.Both)
        {
            if(who == ReRenderingModes.Viewport || who == ReRenderingModes.Both)
                    {
                        Transform3D trans = new MatrixTransform3D(ViewPortGraphics.convert3DArrayToTransformForm(currentTransform));
                        viewportManagers[0].reset();
                        viewportManagers[0].generate_3d_axes(100);
                        renderTriangles = surface.triangulate(settings.renderResolution, settings.renderResolution, this);
                        viewportManagers[0].generate_viewport_object(renderTriangles, settings.renderingFrontColor, settings.renderingBackColor,
                                                                    settings.renderingOpacity);
                        viewportManagers[0].performTransform(trans);

                    }
          if(who == ReRenderingModes.Canvas || who == ReRenderingModes.Both)
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
