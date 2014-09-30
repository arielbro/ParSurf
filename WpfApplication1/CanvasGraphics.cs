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

namespace ParSurf
{
    public class CanvasGraphics
    {
        private Canvas canvas;
        private double xCoordinateRange;
        private double yCoordinateRange;
        private BackgroundWorker bgWorker;
        private double currentCanvasScale;
        private Point currentCanvasOrigin;
        private double pointSize = 0.8;
        private ParallelCoordinates parallelUniverse;
        private IList<double[][]> triangles;
        private int dimension;

        public CanvasGraphics(Canvas canvas, double xCoordinateRange, double yCoordinateRange, int dimension, 
                                IList<double[][]> triangles)
        {
            this.canvas = canvas;
            this.xCoordinateRange = xCoordinateRange;
            this.yCoordinateRange = yCoordinateRange;
            this.triangles = triangles;
            this.dimension = dimension;
            this.triangles = triangles;
            parallelUniverse = new ParallelCoordinates(dimension);
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            bgWorker.WorkerSupportsCancellation = true;
            currentCanvasOrigin = new Point(canvas.ActualHeight / 2, canvas.ActualWidth / 2);
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
                        e.Cancel = true;
                        return;
                    }
                    double[][] transformedTriangle = new double[3][];
                    for(int k=0;k<3;k++)
                    {
                        transformedTriangle[k] = applyTransformToPoint(triangles[i][k], currentTransformMatrix);
                    }
                    lock (transformedTriangles)
                    {
                        transformedTriangles[i] = transformedTriangle;
                    }
                }
            });
            List<Point[]>[] pointSets = parallelUniverse.getPlanePointsFromTriangles(transformedTriangles,dimension);
            Brush[] colors = new Brush[] { Brushes.Red, Brushes.Green };
            Point[] imagesStartingPoints = new Point[2];
            DrawingImage[] imageDrawings = new DrawingImage[2];

            //csv output for iman
            //string csvfilePath = "C:\\Users\\arielbro\\Desktop\\" + DateTime.Now.ToString("HH-mm-ss tt") + "render_capture" +".csv";
            //File.Create(csvfilePath).Close();
            //string delimiter = ",";
            //string[][] outputLines = new string[points[0].Count][];
            //for (int i = 0; i < points[0].Count; i++) outputLines[i] = new string[4];
            //int currLine = 0;

            for (int i = 0; i < 2; i++)
            {
                GeometryDrawing ellipseDraw = new GeometryDrawing();
                ellipseDraw.Brush = colors[i];
                GeometryGroup ellipses = new GeometryGroup();
                ellipses.FillRule = FillRule.Nonzero;

                double minX = 0;
                double minY = 0;

                foreach (Point[] pointSet in pointSets[i])
                {
                    //outputLines[currLine][i] = point.X.ToString();
                    //outputLines[currLine][i+2] = point.Y.ToString();
                    //currLine++;

                    for (int k = 0; k < dimension - 2; k++)
                    {
                        Point point = pointSet[k];
                        if (bgWorker.CancellationPending) { e.Cancel = true; return; }
                        if (Double.IsInfinity(point.X) || Double.IsInfinity(point.Y) ||
                            Double.IsNaN(point.X) || Double.IsNaN(point.Y)) continue;
                        Point canvasPoint = canvasCoordinates(point);
                        //save render time by not submitting points that exceed the canvas' dimensions.
                        //if (canvasPoint.X > canvas1.ActualWidth || canvasPoint.X < 0 ||
                        //    canvasPoint.Y > canvas1.ActualHeight || canvasPoint.Y < 0) continue; //skipped so we can pan and resize the canvas.
                        //get minimal positions of points to set the drawing position inside the canvas (remember that it can contain points with
                        //negative X/Y values, which will be used for panning and zooming.
                        minX = Math.Min(minX, canvasPoint.X);
                        minY = Math.Min(minY, canvasPoint.Y);
                        EllipseGeometry ellipse = new EllipseGeometry(canvasPoint, pointSize, pointSize);
                        ellipses.Children.Add(ellipse);
                    }
                    }
                //do this so the drawing will be correctly aligned in the canvas, in case the minimal X/Y of points is higher than 0.
                ellipses.Children.Add(new EllipseGeometry(new Point(0, 0), 0, 0));
                imagesStartingPoints[i] = new Point(minX, minY);
                ellipses.Freeze();
                ellipseDraw.Geometry = ellipses;
                ellipseDraw.Freeze();
                DrawingImage ellipsesDrawingImage = new DrawingImage(ellipseDraw);
                // Freeze the objects for performance benefits and for enabling thread ownership transfer
                ellipsesDrawingImage.Freeze();
                imageDrawings[i] = ellipsesDrawingImage;
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

            bool isWaitTriggered = false;
            if (Mouse.OverrideCursor != Cursors.Wait)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                isWaitTriggered = true;
            }
            Object[] resultData = e.Result as Object[];
            DrawingImage[] drawingImages = resultData[0] as DrawingImage[];
            Point[] drawingsStartingPoints = resultData[1] as Point[];

            Image[] ellipsesImages = new Image[2];
            for (int i = 0; i < 2; i++)
            {
                Image ellipsesImage = new Image();
                ellipsesImage.Source = drawingImages[i];
                ellipsesImage.Stretch = Stretch.None;
                ellipsesImage.RenderTransform = new TransformGroup();
                ellipsesImages[i] = ellipsesImage;
            }

            clearCanvasPoints();

            for (int i = 0; i < 2; i++)
            {
                canvas.Children.Add(ellipsesImages[i]);
                Canvas.SetTop(ellipsesImages[i], drawingsStartingPoints[i].Y);
                Canvas.SetLeft(ellipsesImages[i], drawingsStartingPoints[i].X);
            }
            if (isWaitTriggered)
            {//wait for canvas to finish drawing and then release the mouse. If called without Triggering wait, some other 
             //part of the code is already forcing finishing, so no need to here.
                canvas.Dispatcher.Invoke(new Action(delegate { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.Render);
            }
        }
        private double[] applyTransformToPoint(double[] point, double[][] transform)
        {
            //transforms point with current transformation. Note that transformations here, in the spirit of C#, 
            //are right multiplicative. Also note that the multiplication is done (artificially) with homogeneous coordinates.
            double[] result = new double[dimension];
            for (int j = 0; j < dimension; j++)
                for (int k = 0; k < dimension + 1; k++)
                    lock(transform)
                        result[j] += (k < dimension ? point[k] : 1) * transform[k][j];
            return result;
        }
        public void reDraw(double[][] transform)
        {
            currentCanvasOrigin = new Point(canvas.ActualHeight / 2, canvas.ActualWidth / 2);
            currentCanvasScale = 1;

            double[][] currentTransformThreadCopy = new double[dimension + 1][];
            for(int i=0; i<dimension+1; i++)
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
            if(widthChangeFactor == Double.PositiveInfinity)//on window creation (which triggers a changeSize with undefined factors).
            {
                //unit matrix!
                double[][] unit = new double[dimension+1][];
                for (int i = 0; i < dimension + 1; i++)
                {
                    unit[i] = new double[dimension + 1];
                    unit[i][i] = 1;
                }
                reDraw(unit);
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
                        foreach (UIElement element in canvas.Children)
            {
                if (element is Image)
                {
                    ScaleTransform s = new ScaleTransform(scale, scale);
                    Image image = element as Image;
                    TransformGroup group = image.RenderTransform as TransformGroup;
                    //the renderTransformOrigin coordinates values are relative to the size of the element - (0,0) is top-left corner,
                    //(1,1) is bottom-right. Currently the transform is centered on the axes (so they can be left in their original position).
                    //Consider centering on the point the mouse is at (and correcting axes height/width post scaling).
                    //                    double originLeft = -Canvas.GetLeft(image) + canvas1.ActualWidth / 2 + currentCanvasTranslation.X;
                    //                    double originTop = -Canvas.GetTop(image) + canvas1.ActualHeight / 2 + currentCanvasTranslation.Y;
                    Point imageOrigin = canvas.TranslatePoint(currentCanvasOrigin, image);
                    image.RenderTransformOrigin = new Point(imageOrigin.X / image.ActualWidth,
                        imageOrigin.Y / image.ActualHeight);
                    group.Children.Add(s);
                }
                if (element is Line)
                {//vertical axes need to be moved, not scaled
                    Line line = element as Line;
                    bool isHorizontal = line.X2 != line.X1;
                    TransformGroup group = line.RenderTransform as TransformGroup;
                    TranslateTransform t;
                    //can't access top/left of the Line's (will get NaN), and X/Y values don't update on translation.
                    Point p = line.TranslatePoint(new Point(0, 0), canvas);
                    if (isHorizontal)
                    {
                        double translation = (scale - 1) * (line.Y1 - canvas.ActualHeight / 2) * currentCanvasScale;
                        t = new TranslateTransform(0, translation);
                    }
                    else
                    {
                        double translation = (scale - 1) * (line.X1 - canvas.ActualWidth / 2) * currentCanvasScale;
                        t = new TranslateTransform(translation, 0);
                    }
                    group.Children.Add(t);
                }

            }
            currentCanvasScale *= scale;
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
    }
}
