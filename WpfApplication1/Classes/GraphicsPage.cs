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
using ParSurf.Properties;

namespace ParSurf
{
    public enum GraphicModes { R3, R4, Rn }; //R3time at the future?
    public enum ReRenderingModes {Viewport, Canvas, Both}
    [Serializable()]
    public abstract class GraphicsPage : Page
    {
        protected double yCoordinateRange = 10;
        protected double xCoordinateRange = 10;
        protected double mouseMoveEpsilon = 0.8;
        public int parallelResolution = Properties.Settings.Default.parallelResolution;
        public int renderResolution = Properties.Settings.Default.renderResolution;
        public double pointSize = Properties.Settings.Default.pointSize;
        private GraphicModes mode;
        protected ViewPortGraphics[] viewportManagers;
        protected CanvasGraphics canvasManager;
        protected Canvas canvas;
        protected Viewport3D[] viewports;
        protected Border viewportsBorder;
        protected Border canvasBorder;
        public int dimension;
        protected int[] currentAxes;
        public Surface surface;
        protected bool[] viewportsmDown;
        protected bool canvasmDown = false;
        private MouseButtonEventArgs lastMouseButtonState;
        private bool mouseMoveSignificant = false;
        protected Point[] viewportsmLastPos;
        protected Point canvasmLastPos;
        public double[][] currentTransform;
        protected IList<double[][]> renderTriangles;
        protected IList<double[][]> parallelTriangles;
        public SolidColorBrush renderingFrontColor;
        public SolidColorBrush renderingBackColor;
        public double renderingOpacity;

        public GraphicsPage()
        {
        }
        public GraphicsPage(GraphicModes mode, int dimension, Surface surface)
        {
            currentTransform = new double[dimension + 1][]; //affine transformations! 
            for (int i = 0; i < dimension + 1; i++)
            {
                currentTransform[i] = new double[dimension + 1];
                currentTransform[i][i] = 1; //identity
            }
            this.mode = mode;
            this.dimension = dimension;
            this.surface = surface;
            if (surface.GetType() == typeof(ParametricSurface) && ((ParametricSurface)surface).parameters.Count != 0)
            {
                //bool temp = false;
                //foreach (KeyValuePair<string, double> param in ((ParametricSurface)surface).parameters)
                //{
                //    if (Double.IsNaN(param.Value)) temp = true;
                //}
                //if (temp)
                //{

                InputNumberForm parameterDialog = new InputNumberForm(((ParametricSurface)surface).parameters);
                if (parameterDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ((ParametricSurface)surface).parameters = parameterDialog.result;
                }
                //}
            }
            this.renderResolution = Settings.Default.renderResolution;
            this.parallelResolution = Settings.Default.parallelResolution;
            this.renderingFrontColor = Settings.Default.frontColor;
            this.renderingBackColor = Settings.Default.backColor;
            this.renderingOpacity = Settings.Default.renderingOpacity;
        }
        protected void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            double widthFactor = 1;
            double heightFactor = 1;
            if (e.PreviousSize.Width != 0)
            {
                widthFactor = e.NewSize.Width / e.PreviousSize.Width;
                heightFactor = e.NewSize.Height / e.PreviousSize.Height;
            }
            else
            {
                widthFactor = e.NewSize.Width / 980;
                heightFactor = e.NewSize.Height / 432;
            }
            if (surface.dimension == 4)
            {
                foreach (Viewport3D viewport in viewports)
                {
                    ((Border)viewport.Parent).Width = this.ActualWidth / 4;
                    ((Border)viewport.Parent).Height = this.ActualHeight / 2;
                }
            }
            canvas.Width *= widthFactor;
            canvas.Height *= heightFactor;
            canvasBorder.BorderThickness = new Thickness(5, 5, 0, 5);
            canvasBorder.Width = Math.Ceiling(this.ActualWidth / 2);
            canvasBorder.Height = this.ActualHeight;
            viewportsBorder.Width = Math.Floor(this.ActualWidth / 2);
            viewportsBorder.Height = this.ActualHeight;
            //force rendering to complete before releasing the mouse
            Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.ApplicationIdle);
        }
        protected void border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double smoothing = 1000;
            double scale = Math.Pow(Math.E, e.Delta / smoothing);

            //find manager to the relevant object.
            UIElement managed = (sender as Border).Child;

            if (canvas == managed)
            {
                canvasManager.performScaleTransform(scale);
                return;
            }

            ScaleTransform3D transform = new ScaleTransform3D(scale, scale, scale);
            double[][] fullTransform = null;
            int[] axesInND = new int[3];
            for (int i = 0; i < viewports.Length; i++)
                if (viewports[i] == managed)
                {
                    if (mode == GraphicModes.R3)
                        axesInND = new int[] { 0, 1, 2 };
                    if (mode == GraphicModes.R4)
                        for (int k = 0; k < 3; k++)
                            axesInND[k] = k >= i ? k + 1 : k;
                    if (mode == GraphicModes.Rn)
                        axesInND = currentAxes;
                    fullTransform = ViewPortGraphics.convert3DTransformToND(transform, dimension, axesInND);
                    currentTransform = multiplyMatrices(currentTransform, fullTransform);
                }
            //update viewports
            if (mode == GraphicModes.R3)
                viewportManagers[0].performTransform(transform);
            if (mode == GraphicModes.R4)
                for (int i = 0; i < viewports.Length; i++)
                {
                    for (int k = 0; k < 3; k++)
                        axesInND[k] = k >= i ? k + 1 : k;
                    viewportManagers[i].performTransform(ViewPortGraphics.convertNDTransformTo3D(fullTransform, axesInND));
                }
            if (mode == GraphicModes.Rn)
                viewportManagers[0].performTransform(transform);
            //update canvas
            canvasManager.reDraw(currentTransform);
        }
        protected void viewportBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseMoveSignificant)
                canvasManager.reDraw(currentTransform);
            resetMouseDowns();
        }
        protected void canvasBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            resetMouseDowns();
        }
        protected void resetMouseDowns()
        {
            for (int i = 0; i < viewports.Length; i++)
                viewportsmDown[i] = false;
            canvasmDown = false;
        }
        protected void border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastMouseButtonState = e;
            //find manager to the relevant object.
            UIElement managed = (sender as Border).Child;
            for (int i = 0; i < viewports.Length; i++)
                if (viewports[i] == managed)
                {
                    viewportsmDown[i] = true;
                    Point pos = Mouse.GetPosition(viewports[i]);
                    viewportsmLastPos[i] = new Point(
                    pos.X - viewports[i].ActualWidth / 2,
                    viewports[i].ActualHeight / 2 - pos.Y);
                }
            if (canvas == managed)
            {
                canvasmDown = true;
                Point pos = Mouse.GetPosition(canvas);
                canvasmLastPos = new Point(
                        pos.X - canvas.ActualWidth / 2,
                        canvas.ActualHeight / 2 - pos.Y);
            }

        }
        protected void viewportBorder_MouseMove(object sender, MouseEventArgs e)
        {
            //If some viewport has its mouse down, I will count it as the one to be transformed,
            //even if the mouse is currently on another viewport.
            int mDownIndex = -1;
            for (int i = 0; i < viewports.Length; i++)
                if (viewportsmDown[i])
                    mDownIndex = i;
            if (mDownIndex == -1)
                return;

            Point pos = Mouse.GetPosition(viewports[mDownIndex]);
            Point actualPos = new Point(
                    pos.X - viewports[mDownIndex].ActualWidth / 2,
                    viewports[mDownIndex].ActualHeight / 2 - pos.Y);
            double dx = actualPos.X - viewportsmLastPos[mDownIndex].X;
            double dy = actualPos.Y - viewportsmLastPos[mDownIndex].Y;
            //Debug.Print(dx + ", " + dy);

            mouseMoveSignificant = Math.Abs(dx) > mouseMoveEpsilon || Math.Abs(dy) > mouseMoveEpsilon;
            if (mouseMoveSignificant)
            {
                //stop whatever the current parallel worker is doing, it is obsolete anyways.
                canvasManager.stopRender();
            }
            Transform3D transform;
            if (lastMouseButtonState.LeftButton == MouseButtonState.Pressed)
            {
                double mouseAngle = 0;

                if (dx != 0 && dy != 0)
                {
                    mouseAngle = Math.Asin(Math.Abs(dy) /
                        Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                    if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                    else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                    else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
                }
                else if (dx == 0 && dy != 0)
                {
                    mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
                }
                else if (dx != 0 && dy == 0)
                {
                    mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;
                }

                double axisAngle = mouseAngle + Math.PI / 2;

                Vector3D axis = new Vector3D(
                        Math.Cos(axisAngle) * 4,
                        Math.Sin(axisAngle) * 4, 0);

                double rotation = 0.02 *
                        Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

                QuaternionRotation3D quaternRotate =
                     new QuaternionRotation3D(
                     new Quaternion(axis, rotation * 180 / Math.PI));

                transform = new RotateTransform3D(quaternRotate);
            }
            else if (lastMouseButtonState.RightButton == MouseButtonState.Pressed)
            {
                double smoothing = 10;
                transform = new TranslateTransform3D(dx / smoothing, dy / smoothing, 0);
            }
            else//middle click does nothing
                return;
            double[][] transformMatrix;
            int[] axesInND;
            if (mode == GraphicModes.R3)
            {
                axesInND = new int[] { 0, 1, 2 };
            }
            else if (mode == GraphicModes.R4)
            {
                axesInND = new int[3];
                for (int i = 0; i < 3; i++)
                    axesInND[i] = i >= mDownIndex ? i + 1 : i;
            }
            else //Rn!
            {
                axesInND = currentAxes;
            }
            transformMatrix = ViewPortGraphics.convert3DTransformToND(transform, dimension, axesInND);
            currentTransform = multiplyMatrices(currentTransform, transformMatrix);

            //transform all the viewports, and update the canvas manager on the transformation.
            if (mode == GraphicModes.R4)
            {
                for (int i = 0; i < viewports.Length; i++)
                {
                    Transform3D projectedTransform = ViewPortGraphics.convert4DTransformTo3D(currentTransform, i);
                    viewportManagers[i].setTransform(projectedTransform);
                }
            }
            if (mode == GraphicModes.R3)
            {
                Transform3D t = new MatrixTransform3D(ViewPortGraphics.convert3DArrayToTransformForm(currentTransform));
                viewportManagers[0].setTransform(t);
            }
            if (mode == GraphicModes.Rn)
            {
                Transform3D projectedTransform = ViewPortGraphics.convertNDTransformTo3D(currentTransform, axesInND);
                viewportManagers[0].setTransform(projectedTransform);
            }
            viewportsmLastPos[mDownIndex] = actualPos;
        }
        protected void canvasBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (!canvasmDown) return;
            Point pos = Mouse.GetPosition(canvas);
            Point actualPos = new Point(
                    pos.X - canvas.ActualWidth / 2,
                    canvas.ActualHeight / 2 - pos.Y);
            double dx = actualPos.X - canvasmLastPos.X;
            double dy = -(actualPos.Y - canvasmLastPos.Y);//don't know why, but this one needs inverting
            canvasManager.performTranslateTransfrom(dx, dy);
            canvasmLastPos = actualPos;
        }
        protected void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double heightChangeFactor = e.NewSize.Height / e.PreviousSize.Height;
            double widthChangeFactor = e.NewSize.Width / e.PreviousSize.Width;
            canvasManager.changeSize(widthChangeFactor, heightChangeFactor);
        }
        protected double[][] multiplyMatrices(double[][] leftMatrix, double[][] rightMatrix)
        {
            /**basically matrix multliplication.
            *See this if optimization is in need:
            *http://www.heatonresearch.com/content/choosing-best-c-array-type-matrix-multiplication
            *also note that C# uses graphics matrices as right-hand multiplicators, so transformation composition is done 
            *by multiplying the last transform in the right side.
            *
            */
            double[][] res = new double[dimension + 1][];
            for (int i = 0; i < dimension + 1; i++)
            {
                res[i] = new double[dimension + 1];
                for (int j = 0; j < dimension + 1; j++)
                {
                    for (int k = 0; k < dimension + 1; k++)
                        res[i][j] += leftMatrix[i][k] * rightMatrix[k][j];
                }
            }
            return res;
        }
        public abstract void reRender(ReRenderingModes who = ReRenderingModes.Both);
        public void resetTransformation()
        {
            currentTransform = identityTransformation();
            foreach (ViewPortGraphics manager in viewportManagers)
                manager.setTransform(Transform3D.Identity);
            canvasManager.reDraw(currentTransform);
        }
        public void applyTransformation(double[][] transformation)
        {
            currentTransform = multiplyMatrices(transformation, currentTransform);
            reRender(ReRenderingModes.Both);
        }
        private double[][] identityTransformation()
        {
            double[][] identity = new double[dimension + 1][]; //affine transformations! 
            for (int i = 0; i < dimension + 1; i++)
            {
                identity[i] = new double[dimension + 1];
                identity[i][i] = 1; //identity
            }
            return identity;
        }
        public void applyRenderingColorScheme(SolidColorBrush frontColor, SolidColorBrush backColor)
        {
            this.renderingFrontColor = frontColor;
            this.renderingBackColor = backColor;
            foreach (ViewPortGraphics manager in viewportManagers)
            {
                foreach (ViewPortGraphics viewportManager in viewportManagers)
                {
                    viewportManager.changeColorScheme(frontColor, backColor);
                }
            }
        }
    }
}