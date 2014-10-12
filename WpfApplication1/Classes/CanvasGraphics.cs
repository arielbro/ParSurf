﻿using System;
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
            canvas.Dispatcher.Invoke(new Action(() => { Mouse.OverrideCursor = Cursors.Wait; }), DispatcherPriority.Send);

            double[][] currentTransformMatrix = e.Argument as double[][];
            double[][][] transformedTriangles = new double[triangles.Count][][];
            const int batchSize = 10;
            Parallel.For(0, triangles.Count / batchSize + 1, (batch, loopState) =>
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
            int totalDrawingNumber = originalParallelPointsShown.Count + transposedParallelPointsShown.Count;
            Point[] imagesStartingPoints = new Point[totalDrawingNumber];
            DrawingImage[] imageDrawings = new DrawingImage[totalDrawingNumber];

            foreach (bool isTransposed in new bool[] { false, true })
            {
                int numberOfDrawings = isTransposed ? transposedParallelPointsShown.Count : originalParallelPointsShown.Count;
                GeometryDrawing[] ellipseDraws = new GeometryDrawing[numberOfDrawings];
                GeometryGroup[] ellipseses = new GeometryGroup[numberOfDrawings];
                double[] minXs = new double[numberOfDrawings];
                double[] minYs = new double[numberOfDrawings];
                for (int i = 0; i < numberOfDrawings; i++)
                {
                    ellipseDraws[i] = new GeometryDrawing();
                    ellipseDraws[i].Brush = new SolidColorBrush(coloringFunction(i, isTransposed));
                    ellipseses[i] = new GeometryGroup();
                    ellipseses[i].FillRule = FillRule.Nonzero;
                }
                foreach (Point[] pointSet in pointSets[isTransposed ? 1 : 0])
                {
                    Debug.Assert(pointSet.Length == numberOfDrawings);
                    for (int k = 0; k < numberOfDrawings; k++)
                    {
                        Point point = pointSet[k];
                        if (bgWorker.CancellationPending) { e.Cancel = true; return; }
                        if (Double.IsInfinity(point.X) || Double.IsInfinity(point.Y) ||
                            Double.IsNaN(point.X) || Double.IsNaN(point.Y)) continue;
                        Point canvasPoint = canvasCoordinates(point);
                        //get minimal positions of points to set the drawing position inside the canvas (remember that it can contain points with
                        //negative X/Y values, which will be used for panning and zooming.
                        minXs[k] = Math.Min(minXs[k], canvasPoint.X);
                        minYs[k] = Math.Min(minYs[k], canvasPoint.Y);
                        EllipseGeometry ellipse = new EllipseGeometry(canvasPoint, pointSize, pointSize);
                        ellipseses[k].Children.Add(ellipse);
                    }
                }
                //do this so the drawing will be correctly aligned in the canvas, in case the minimal X/Y of points is higher than 0.
                for (int k = 0; k < numberOfDrawings; k++)
                {
                    ellipseses[k].Children.Add(new EllipseGeometry(new Point(0, 0), 0, 0));
                    imagesStartingPoints[isTransposed ? originalParallelPointsShown.Count + k : k] = new Point(minXs[k], minYs[k]);
                    ellipseses[k].Freeze();
                    ellipseDraws[k].Geometry = ellipseses[k];
                    ellipseDraws[k].Freeze();
                    DrawingImage ellipsesDrawingImage = new DrawingImage(ellipseDraws[k]);
                    // Freeze the objects for performance benefits and for enabling thread ownership transfer
                    ellipsesDrawingImage.Freeze();
                    imageDrawings[isTransposed ? originalParallelPointsShown.Count + k : k] = ellipsesDrawingImage;
                }
            }
            Object returnedData = new Object[] { imageDrawings, imagesStartingPoints, parallelUniverse };
            e.Result = returnedData;

            //csv output for iman
            //int length = outputLines.GetLength(0);
            //StringBuilder sb = new StringBuilder();
            //for (int index = 0; index < length; index++)
            //    sb.AppendLine(string.Join(delimiter, outputLines[index]));
            //File.AppendAllText(csvfilePath, sb.ToString());
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

            Object[] resultData = e.Result as Object[];
            DrawingImage[] drawingImages = resultData[0] as DrawingImage[];
            Point[] drawingsStartingPoints = resultData[1] as Point[];

            Image[] ellipsesImages = new Image[drawingImages.Length];
            for (int i = 0; i < drawingImages.Length; i++)
            {
                Image ellipsesImage = new Image();
                ellipsesImage.Source = drawingImages[i];
                ellipsesImage.Stretch = Stretch.None;
                ellipsesImage.RenderTransform = new TransformGroup();
                ellipsesImages[i] = ellipsesImage;
            }

            clearCanvasPoints();

            for (int i = 0; i < ellipsesImages.Length; i++)
            {
                canvas.Children.Add(ellipsesImages[i]);
                Canvas.SetTop(ellipsesImages[i], drawingsStartingPoints[i].Y);
                Canvas.SetLeft(ellipsesImages[i], drawingsStartingPoints[i].X);
            }
            {
                //wait for rendering to finish before releasing mouse
                canvas.Dispatcher.BeginInvoke(new Action(delegate { Mouse.OverrideCursor = Cursors.Arrow; }),
                                            DispatcherPriority.ApplicationIdle);
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
                return chosenColors[isTransposed ? pointIndex + originalPoints : pointIndex];
            };
        }

        internal static ColoringFunction getGradientColoringFunction(Color currentOriginalPointsColor, Color currentTransposedPointsColor,
                                                                    int originalPoints, int transposedPoints)
        {
            return delegate(int pointIndex, bool isTransposed)
            {
                Color relevantColor = isTransposed ? currentTransposedPointsColor : currentOriginalPointsColor;
                //go from black to white, but don't choose actual black/white
                float coefficient = (pointIndex + 1) / ((isTransposed ? transposedPoints : originalPoints) + (float)1);
                relevantColor.ScR *= coefficient;
                relevantColor.ScG *= coefficient;
                relevantColor.ScB *= coefficient;
                return relevantColor;
            };
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
