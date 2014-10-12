using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;
using ParSurf;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Reflection;
using System.Diagnostics;

namespace ParSurf
{
    public class CanvasGraphics
    {
        private Canvas canvas;
        private double xCoordinateRange;
        private double yCoordinateRange;
        private double currentCanvasScale = 1;
        private BackgroundWorker bgWorker;
        private Point currentCanvasOrigin;
        public double pointSize;
        private ParallelCoordinates parallelUniverse;
        private IList<double[][]> triangles;
        private int dimension;
        public List<Tuple<int, int, int>> originalParallelPointsShown;
        public List<Tuple<int, int, int>> transposedParallelPointsShown;
        public ColoringFunction coloringFunction;

        public CanvasGraphics(Canvas canvas, double xCoordinateRange, double yCoordinateRange,
            int dimension, IList<double[][]> triangles, List<Tuple<int, int, int>> originalParallelPointsShown,
                                                        List<Tuple<int, int, int>> transposedParallelPointsShown, double pointSize = 0.8)
        {
            this.pointSize = pointSize;
            this.canvas = canvas;
            this.xCoordinateRange = xCoordinateRange;
            this.yCoordinateRange = yCoordinateRange;
            this.triangles = triangles;
            this.dimension = dimension;
            this.triangles = triangles;
            this.originalParallelPointsShown = originalParallelPointsShown;
            this.transposedParallelPointsShown = transposedParallelPointsShown;
            parallelUniverse = new ParallelCoordinates(dimension);
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            bgWorker.WorkerSupportsCancellation = true;
            currentCanvasScale = 1;
        }
        private Point canvasCoordinates(Point point)
        {
            //translate coordinates of a point to the corresponding coordinates on (or out of the) canvas.
            double X = point.X * canvas.ActualWidth / (2 * xCoordinateRange) + canvas.ActualWidth / 2;
            double Y = -point.Y * canvas.ActualHeight / (2 * yCoordinateRange) + canvas.ActualHeight / 2;
            return new Point(X, Y);
        }
        public void clearCanvasPoints()
        {
            //iterate backwards over the canvas' children while removing ellipses/images. Backwards motion does not change remaining indices.
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                if ((canvas.Children[i] is Ellipse) || (canvas.Children[i] is Image)) canvas.Children.Remove(canvas.Children[i]);
            }
        }
        public void drawAxes()
        {
            foreach (double xCoordinate in parallelUniverse.getAxesCoordinates())
            {
                Line axis = new Line();
                Point lowerPoint = canvasCoordinates(new Point(xCoordinate, -yCoordinateRange));
                axis.X1 = lowerPoint.X;
                axis.Y1 = lowerPoint.Y;
                Point upperPoint = canvasCoordinates(new Point(xCoordinate, yCoordinateRange));
                axis.X2 = upperPoint.X;
                axis.Y2 = upperPoint.Y;
                axis.StrokeThickness = 2;
                axis.Stroke = System.Windows.Media.Brushes.Black;
                axis.RenderTransform = new TransformGroup();
                canvas.Children.Add(axis);
            }
            Line horizontalAxis = new Line();
            Point leftPoint = canvasCoordinates(new Point(-xCoordinateRange, 0));
            Point rightPoint = canvasCoordinates(new Point(xCoordinateRange, 0));
            horizontalAxis.X1 = leftPoint.X;
            horizontalAxis.Y1 = leftPoint.Y;
            horizontalAxis.X2 = rightPoint.X;
            horizontalAxis.Y2 = rightPoint.Y;
            horizontalAxis.StrokeThickness = 2;
            horizontalAxis.Stroke = System.Windows.Media.Brushes.Black;
            horizontalAxis.RenderTransform = new TransformGroup();
            canvas.Children.Add(horizontalAxis);
        }
        public void clearAxes()
        {
            //iterate backwards over the canvas' children while removing lines. Backwards motion does not change remaining indices.
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                //Debug.WriteLine(canvas1.Children[i].GetType());
                if (canvas.Children[i] is Line) canvas.Children.Remove(canvas.Children[i]);
            }
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            canvas.Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Wait; }), DispatcherPriority.Send);

            double[][] currentTransformMatrix = e.Argument as double[][];
            double[][][] transformedTriangles = new double[triangles.Count][][];
            int batchSize = (triangles.Count + Environment.ProcessorCount - 1) / Environment.ProcessorCount; //(which is triangles/processors rounded up)
            Parallel.For(0, Environment.ProcessorCount, (batch, loopState) =>
            {
                for (int i = batch * batchSize; i < Math.Min((batch + 1) * batchSize, triangles.Count); i++)
                {
                    if (bgWorker.CancellationPending)
                    {
                        loopState.Stop();
                        //wait for rendering to finish before releasing mouse
                        canvas.Dispatcher.BeginInvoke(new Action(delegate { Mouse.OverrideCursor = Cursors.Arrow; }),
                            DispatcherPriority.ApplicationIdle);
                        e.Cancel = true;
                        return;
                    }
                    double[][] transformedTriangle = new double[3][];
                    for (int k = 0; k < 3; k++)
                    {
                        transformedTriangle[k] = applyTransformToPoint(triangles[i][k], currentTransformMatrix);
                    }
                    lock (transformedTriangles)
                    {
                        transformedTriangles[i] = transformedTriangle;
                    }
                }
            });
            List<Point[]>[] pointSets = parallelUniverse.getPlanePointsFromTriangles(transformedTriangles, dimension,
                                                                  originalParallelPointsShown, transposedParallelPointsShown);

            //            int totalDrawingNumber = originalParallelPointsShown.Count + transposedParallelPointsShown.Count;

            Object returnedData = pointSets;
            e.Result = returnedData;
        }
        private void bgWorker_RunWorkerCompleted(object sender,
                                       RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
            if (e.Cancelled)
            {
                return;
            }
            if (e.Error != null)
                throw e.Error;

            List<Point[]>[] pointSets = e.Result as List<Point[]>[];
            clearCanvasPoints();

            int originalsCount = originalParallelPointsShown.Count;
            int transposedCount = transposedParallelPointsShown.Count;
            int totalCount = originalsCount + transposedCount;
            foreach (bool isTransposedSet in new bool[] { false, true })
            {
                List<Point[]> relevantPointSet = (isTransposedSet ? pointSets[1] : pointSets[0]);
                int relevantCount = isTransposedSet ? transposedCount : originalsCount;
                SolidColorBrush[] brushes = new SolidColorBrush[relevantCount];
                for (int i = 0; i < relevantCount; i++)
                {
                    brushes[i] = new SolidColorBrush(coloringFunction(i, isTransposedSet));
                    brushes[i].Freeze();
                }
                foreach (Point[] planePoints in relevantPointSet)
                {
                    for (int i = 0; i < relevantCount; i++)
                    {
                        if (double.IsInfinity(planePoints[i].X) || double.IsInfinity(planePoints[i].Y))
                            continue;
                        Ellipse ellipse = new Ellipse();
                        canvas.Children.Add(ellipse);
                        ellipse.Fill = brushes[i];
                        ellipse.Width = pointSize;
                        ellipse.Height = pointSize;
                        Point canvasPoint = canvasCoordinates(planePoints[i]);
                        Canvas.SetTop(ellipse, canvasPoint.Y);
                        Canvas.SetLeft(ellipse, canvasPoint.X);

                    }
                }
                {
                    //wait for rendering to finish before releasing mouse
                    canvas.Dispatcher.BeginInvoke(new Action(delegate { Mouse.OverrideCursor = Cursors.Arrow; }),
                                                DispatcherPriority.ApplicationIdle);
                }
            }
        }
        private double[] applyTransformToPoint(double[] point, double[][] transform)
        {
            //transforms point with current transformation. Note that transformations here, in the spirit of C#, 
            //are right multiplicative. Also note that the multiplication is done (artificially) with homogeneous coordinates.
            double[] result = new double[dimension];
            for (int j = 0; j < dimension; j++)
                for (int k = 0; k < dimension + 1; k++)
                    lock (transform)
                        result[j] += (k < dimension ? point[k] : 1) * transform[k][j];
            return result;
        }
        public void reDraw(double[][] transform, IList<double[][]> triangles)
        {
            this.triangles = triangles;
            currentCanvasOrigin = new Point(canvas.ActualHeight / 2, canvas.ActualWidth / 2);
            currentCanvasScale = 1;

            double[][] currentTransformThreadCopy = new double[dimension + 1][];
            for (int i = 0; i < dimension + 1; i++)
                currentTransformThreadCopy[i] = transform[i].Clone() as double[];
            if (!bgWorker.IsBusy)
                bgWorker.RunWorkerAsync(currentTransformThreadCopy);
            else
            {
                bgWorker.CancelAsync();
                bgWorker = new BackgroundWorker();
                bgWorker.DoWork += bgWorker_DoWork;
                bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
                bgWorker.WorkerSupportsCancellation = true;
                bgWorker.RunWorkerAsync(currentTransformThreadCopy);
            }
            clearAxes();
            drawAxes();
        }
        public void stopRender()
        {
            bgWorker.CancelAsync();
        }
        public void changeSize(double widthChangeFactor, double heightChangeFactor)
        {
            if (widthChangeFactor == Double.PositiveInfinity)//on window creation (which triggers a changeSize with undefined factors).
            {
                //unit matrix!
                double[][] unit = new double[dimension + 1][];
                for (int i = 0; i < dimension + 1; i++)
                {
                    unit[i] = new double[dimension + 1];
                    unit[i][i] = 1;
                }
                reDraw(unit, triangles);
                return;
            }
            foreach (UIElement element in canvas.Children)
            {
                if (element is Line)
                {
                    Line line = element as Line;
                    line.X1 = line.X1 * widthChangeFactor;
                    line.Y1 = line.Y1 * heightChangeFactor;
                    line.X2 = line.X2 * widthChangeFactor;
                    line.Y2 = line.Y2 * heightChangeFactor;
                    continue;
                }
                else if (element is Ellipse)
                {
                    //Debug.WriteLine("height of ellipse rectangle: " + (element as Ellipse).ActualHeight);
                    //Debug.WriteLine("width of ellipse rectangle: " + (element as Ellipse).ActualWidth);

                    Canvas.SetTop(element, Canvas.GetTop(element) * heightChangeFactor);
                    Canvas.SetLeft(element, Canvas.GetLeft(element) * widthChangeFactor);
                }
                else if (element is Image)
                {
                    Image image = element as Image;
                    //image.RenderSize = new Size(image.ActualWidth * widthChangeFactor, image.ActualHeight * heightChangeFactor);
                    Transform resize = new ScaleTransform(widthChangeFactor, heightChangeFactor);
                    TransformGroup resizeGroup = image.RenderTransform as TransformGroup;
                    resizeGroup.Children.Add(resize);
                }
            }
        }
        public void performScaleTransform(double scale)
        {
            //            foreach (UIElement element in canvas.Children)
            //{
            //    if (element is Image)
            //    {
            //        ScaleTransform s = new ScaleTransform(scale, scale);
            //        Image image = element as Image;
            //        TransformGroup group = image.RenderTransform as TransformGroup;
            //        //the renderTransformOrigin coordinates values are relative to the size of the element - (0,0) is top-left corner,
            //        //(1,1) is bottom-right. Currently the transform is centered on the axes (so they can be left in their original position).
            //        //Consider centering on the point the mouse is at (and correcting axes height/width post scaling).
            //        //                    double originLeft = -Canvas.GetLeft(image) + canvas1.ActualWidth / 2 + currentCanvasTranslation.X;
            //        //                    double originTop = -Canvas.GetTop(image) + canvas1.ActualHeight / 2 + currentCanvasTranslation.Y;
            //        Point imageOrigin = canvas.TranslatePoint(currentCanvasOrigin, image);
            //        image.RenderTransformOrigin = new Point(imageOrigin.X / image.ActualWidth,
            //            imageOrigin.Y / image.ActualHeight);
            //        group.Children.Add(s);
            //    }
            //    if (element is Line)
            //    {//vertical axes need to be moved, not scaled
            //        Line line = element as Line;
            //        bool isHorizontal = line.X2 != line.X1;
            //        TransformGroup group = line.RenderTransform as TransformGroup;
            //        TranslateTransform t;
            //        //can't access top/left of the Line's (will get NaN), and X/Y values don't update on translation.
            //        Point p = line.TranslatePoint(new Point(0, 0), canvas);
            //        if (isHorizontal)
            //        {
            //            double translation = (scale - 1) * (line.Y1 - canvas.ActualHeight / 2) * currentCanvasScale;
            //            t = new TranslateTransform(0, translation);
            //        }
            //        else
            //        {
            //            double translation = (scale - 1) * (line.X1 - canvas.ActualWidth / 2) * currentCanvasScale;
            //            t = new TranslateTransform(translation, 0);
            //        }
            //        group.Children.Add(t);
            //    }

            //}
            //currentCanvasScale *= scale;
        }
        public void performTranslateTransfrom(double dx, double dy)
        {
            double smoothing = 1;
            TranslateTransform t = new TranslateTransform(dx / smoothing, dy / smoothing);
            currentCanvasOrigin += new Vector(dx / smoothing, dy / smoothing);

            foreach (UIElement element in canvas.Children)
            {
                if (element is Image)
                {
                    Image image = element as Image;
                    TransformGroup group = image.RenderTransform as TransformGroup;
                    group.Children.Add(t);
                }
                if (element is Line)
                {
                    Line line = element as Line;
                    bool isHorizontal = line.X1 < line.X2;
                    TransformGroup group = line.RenderTransform as TransformGroup;
                    //move horizontal axis only up and down, verticals only right and left.
                    group.Children.Add(new TranslateTransform(isHorizontal ? 0 : dx / smoothing, isHorizontal ? dy / smoothing : 0));
                }
                if (element is Ellipse)
                {
                    Canvas.SetTop(element, Canvas.GetTop(element) + dy / smoothing);
                    Canvas.SetLeft(element, Canvas.GetLeft(element) + dx / smoothing);
                }
            }
        }

        public delegate Color ColoringFunction(int pointIndex, bool isTransposed);
        internal static ColoringFunction getArbitraryColoringFunction(int originalPoints, int transposedPoints)
        {
            //use reflection to get a list of color properties, to choose from randomaly
            Type colorsType = typeof(Colors);
            PropertyInfo[] infos = colorsType.GetProperties();
            List<Color> colors = new List<Color>();
            foreach (PropertyInfo info in infos)
                colors.Add((Color)info.GetValue(null, null));
            Random rnd = new Random();
            List<Color> chosenColors = colors.OrderBy(x => rnd.Next()).Take(originalPoints + transposedPoints).ToList();
            return delegate(int pointIndex, bool isTransposed)
            {
                //might be more plane point than the whole Colors list, so use modulus for this case.
                return chosenColors[(isTransposed ? pointIndex + originalPoints : pointIndex) % chosenColors.Count];
            };
        }

        internal static ColoringFunction getGradientColoringFunction(Color currentOriginalPointsColor, Color currentTransposedPointsColor,
                                                                    int originalPoints, int transposedPoints)
        {
            return delegate(int pointIndex, bool isTransposed)
            {
                Color relevantColor = isTransposed ? currentTransposedPointsColor : currentOriginalPointsColor;
                int relevantSize = isTransposed ? transposedPoints : originalPoints;
                //convert to HSL
                Tuple<double, double, double> hsv = ColorToHSV(relevantColor);
                //now mess with the luminosity to get shades
                Tuple<double,double,double> newHsv = new Tuple<double,double,double>(hsv.Item1,hsv.Item2,(float)(pointIndex + 1) / (relevantSize + 1));
                //and back to RGB
                Color newColor = ColorFromHSV(newHsv.Item1, newHsv.Item2, newHsv.Item3);
                return newColor;
            };
        }

        public static Tuple<double,double,double> ColorToHSV(Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            System.Drawing.Color drawColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            double hue = drawColor.GetHue();
            double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            double value = max / 255d;
            return new Tuple<double, double, double> ( hue, saturation, value );
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, (byte)v, (byte)t, (byte)p);
            else if (hi == 1)
                return Color.FromArgb(255, (byte)q, (byte)v, (byte)p);
            else if (hi == 2)
                return Color.FromArgb(255, (byte)p, (byte)v, (byte)t);
            else if (hi == 3)
                return Color.FromArgb(255, (byte)p, (byte)q, (byte)v);
            else if (hi == 4)
                return Color.FromArgb(255, (byte)t, (byte)p, (byte)v);
            else
                return Color.FromArgb(255, (byte)v, (byte)p, (byte)q);
        }

        internal static ColoringFunction getSolidColoringFunction(Color currentOriginalPointsColor, Color currentTransposedPointsColor)
        {
            return delegate(int pointIndex, bool isTransposed)
            {
                return isTransposed ? currentTransposedPointsColor : currentOriginalPointsColor;
            };
        }
    }
}
