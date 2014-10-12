using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Diagnostics;
using W3b.Sine;

namespace ParSurf
{
    class ParallelCoordinates
    {
        private int dimension;
        private double[] axesCoordinates;
        private List<double[]> points = new List<double[]>();
        private List<ParallelCoordinatesLine> lines = new List<ParallelCoordinatesLine>();
        private List<ParallelCoordinatesPlane> planes = new List<ParallelCoordinatesPlane>();

        public ParallelCoordinates(int dimension)
        {
            this.dimension = dimension;
            axesCoordinates = new double[dimension];
            for (int i = 0; i < dimension; i++) axesCoordinates[i] = i - (dimension - 1) / 2.0;
        }
        public ParallelCoordinates(ParallelCoordinates universe)
        {
            this.dimension = universe.dimension;
            axesCoordinates = (double[])universe.axesCoordinates.Clone();
            points = new List<double[]>(universe.points);
            lines = new List<ParallelCoordinatesLine>(universe.lines);
            planes = new List<ParallelCoordinatesPlane>(universe.planes);
        }
        public double[] getAxesCoordinates()
        {
            double[] resArray = new double[dimension];
            axesCoordinates.CopyTo(resArray, 0);
            return resArray;
        }

        public void addLine(double[] p1, double[] p2)
        {
            lines.Add(new ParallelCoordinatesLine(this, p1, p2));
        }
        public void addPlane(double[] p1, double[] p2, double[] p3)
        {
            planes.Add(new ParallelCoordinatesPlane(this, p1, p2, p3));
        }
        public void clearLines()
        {
            lines = new List<ParallelCoordinatesLine>();
        }
        public void clearPlanes()
        {
            planes = new List<ParallelCoordinatesPlane>();
        }
        public List<Point>[] getPlanePoints()
        {
            List<Point>[] points = new List<Point>[2];//first list contains original (reds), other transposed (green)
            points[0] = new List<Point>();
            points[1] = new List<Point>();
            foreach (ParallelCoordinatesPlane plane in planes)
            {
                Point[] planePoints = plane.represent();
                if (planePoints != null)
                    foreach (bool transposed in new bool[] { false, true })
                        for (int i = transposed ? dimension - 2 : 0; i < (transposed ? 2 * (dimension - 2) : dimension - 2); i++)
                            points[transposed ? 1 : 0].Add(planePoints[i]);
            }
            return points;
        }
        public static bool isSameLine(Point p1, Point p2, Point p3, Point p4)
        {
            double epsilon = 0.0001;
            double slope = (p2.Y - p1.Y) / (p2.X - p1.X);
            return Math.Abs((p3.X - p1.X) * slope + p1.Y - p3.Y) < epsilon;
        }
        public static Point line_intersection_by_four_points(Point p1, Point p2, Point p3, Point p4)
        {
            double epsilon = 0.0001;
            double Px;
            double Py;

            double denominator = ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));
            //if the denominator is zero, there are two options - 1. same line (any point is good as intersection), 2. parallel (ideal point should be returned, currently generic).
            //we exclude lines parallel to the y axis (because no parallel coordinates use will give out these kind of lines), so can assume the slopes are well defined.
            if (Math.Abs(denominator) < epsilon)//for numerical reasons, substitute equality for nearness
            {
                //double slope = (p2.Y - p1.Y) / (p2.X - p1.X);
                //bool isSameLine = Math.Abs((p3.X - p1.X) * slope + p1.Y - p3.Y) < epsilon;
                //if (isSameLine)
                //{
                //    //thumb rule - in 3d, if we have points on an x1 or x3 parallel line, we might get the same point as two representives of the line. To avoid this, 
                //    // we arbitrarily choose to take the middle (in 3d, it means that lines that can be described by two points won't get one representing point)
                //    Px = (p2.X + p1.X) / 2;
                //    Py = (p2.Y + p1.Y) / 2;
                //    return new Point(Px, Py);
                //} commented out because this brought up problems - for now, ignore points with this property. Also, figure out how
                // the definition of plane by three points enables such ambiguity in choise of plane points.
                return new Point(Double.NaN, Double.NaN);
            }
            //Debug.WriteLine("hi");
            Px = ((p1.X * p2.Y - p1.Y * p2.X) * (p3.X - p4.X) - (p1.X - p2.X) * (p3.X * p4.Y - p3.Y * p4.X)) / denominator;
            Py = ((p1.X * p2.Y - p1.Y * p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X * p4.Y - p3.Y * p4.X)) / denominator;
            //Debug.WriteLine("hi" + Px + "        " + Py);
            return new Point(Px, Py);
        }

        class ParallelCoordinatesLine
        {
            private ParallelCoordinates universe;
            private double[] p1;
            private double[] p2;

            public ParallelCoordinatesLine(ParallelCoordinates universe, double[] p1, double[] p2)
            {
                this.universe = universe;
                if (p1.Length != universe.dimension || p2.Length != universe.dimension)
                    throw new Exception("Line constructor recieved point arguments with incompatible dimension");
                this.p1 = p1;
                this.p2 = p2;
            }

            public Point computeLij(int i, int j)
            {
                Point p1i = new Point(universe.axesCoordinates[i], p1[i]);
                Point p1j = new Point(universe.axesCoordinates[j], p1[j]);
                Point p2i = new Point(universe.axesCoordinates[i], p2[i]);
                Point p2j = new Point(universe.axesCoordinates[j], p2[j]);
                return line_intersection_by_four_points(p1i, p1j, p2i, p2j);
            }
            public Point[] represent()
            {
                Point[] points = new Point[universe.dimension - 1];
                for (int i = 0; i < universe.dimension - 1; i++)
                    points[i] = computeLij(i, i + 1);
                return points;
            }
        }

        class ParallelCoordinatesPlane
        {
            //notice that plane is immutable - hence we can cache preivous represent() results
            private ParallelCoordinates universe;
            private double[] p1;
            private double[] p2;
            private double[] p3;
            private Point[] representCache;

            public ParallelCoordinatesPlane(ParallelCoordinates universe, double[] p1, double[] p2, double[] p3)
            {
                this.universe = universe;
                if (p1.Length != universe.dimension || p2.Length != universe.dimension || p3.Length != universe.dimension)
                    throw new Exception("Plane constructor recieved point arguments with incompatible dimension");
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
            }

            public Point[] represent()
            {
                if (representCache != null)
                    return representCache;
                ParallelCoordinatesLine line1 = new ParallelCoordinatesLine(universe, p1, p2);
                ParallelCoordinatesLine line2 = new ParallelCoordinatesLine(universe, p2, p3);
                Point[] line1Points = line1.represent();
                Point[] line2Points = line2.represent();

                //for dimension n, the plane will have 2(n-2) points representing it. The last (n-2) come from inverting axes locations.
                Point[] planePoints = new Point[2 * (universe.dimension - 2)];
                if (isSameLine(line1Points[0], line1Points[1], line2Points[0], line2Points[1]))
                {
                    //for (int i = 0; i < universe.dimension - 2; i++)
                    //{
                    //    planePoints[i] = new Point(Double.PositiveInfinity, Double.PositiveInfinity);
                    //    planePoints[universe.dimension - 2 + i] = new Point(Double.PositiveInfinity, Double.PositiveInfinity);
                    //}
                    //return planePoints;
                    return null;
                }
                for (int i = 0; i < universe.dimension - 2; i++)
                {
                    planePoints[i] = line_intersection_by_four_points(line1Points[i], line1Points[i + 1], line2Points[i], line2Points[i + 1]);

                }
                for (int i = 0; i < universe.dimension - 2; i++)
                {
                    //place the i'th axis in front of the i+1,i+2 axes.
                    //We assume that whatever the actual axes positions are, the distances between them are 1.
                    universe.axesCoordinates[i] = universe.axesCoordinates[i] + 3;
                    line1Points[i] = line1.computeLij(i, i + 1);
                    line2Points[i] = line2.computeLij(i, i + 1);
                    universe.axesCoordinates[i] = universe.axesCoordinates[i] - 3;
                }
                for (int i = 0; i < universe.dimension - 2; i++)
                {
                    planePoints[universe.dimension - 2 + i] = line_intersection_by_four_points(line1Points[i], line1Points[i + 1], line2Points[i], line2Points[i + 1]);
                }
                representCache = planePoints;
                return planePoints;
            }
        }

        private static Point3D[] triangleProjection(double[][] triangle, Tuple<int, int, int> axes)
        {
            return new Point3D[]{new Point3D(triangle[0][axes.Item1],triangle[0][axes.Item2],triangle[0][axes.Item3]),
                                 new Point3D(triangle[1][axes.Item1],triangle[1][axes.Item2],triangle[1][axes.Item3]),
                                 new Point3D(triangle[2][axes.Item1],triangle[2][axes.Item2],triangle[2][axes.Item3])};
        }
        private static double[][] planeCoefficientsFromTriangleRn(double[][] triangle, int dimension)
        {
            //Plane equation from three points - in Rn, (n-2) independant equations determine a plane.
            //It can be futher constrainted that the i'th equation will consist of the variables (Xi,Xi+1, Xi+2) alone.
            double[][] res = new double[dimension - 2][];
            for (int i = 0; i < dimension - 2; i++)
                res[i] = planeCoefficientsFromTriangle3D(triangleProjection(triangle, new Tuple<int, int, int>(i, i + 1, i + 2)));
            return res;
        }
        //R3 only method (which is used in the Rn case)
        private static double[] planeCoefficientsFromTriangle3D(Point3D[] triangle)
        {
            //plane equation from three points
            Point3D p1 = triangle[0];
            Point3D p2 = triangle[1];
            Point3D p3 = triangle[2];
            //Debug.Assert(!p1.Equals(p2) && !p2.Equals(p3) && !p3.Equals(p1));
            double c1 = (p1.Y - p2.Y) * (p1.Z - p3.Z) - (p1.Z - p2.Z) * (p1.Y - p3.Y);
            double c2 = (p1.Z - p2.Z) * (p1.X - p3.X) - (p1.X - p2.X) * (p1.Z - p3.Z);
            double c3 = (p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X);
            double c0 = c1 * p1.X + c2 * p1.Y + c3 * p1.Z;
            return new double[] { c1, c2, c3, c0 };
        }
        public List<Point[]>[] getPlanePointsFromTriangles(double[][][] triangles, int dimension,
                       List<Tuple<int, int, int>> originalPlanePointsToCompute, List<Tuple<int, int, int>> transposedPLanePointsToCompute)
        {
            //using equation from P.167 (5.78) on Alfred's book for representative points of p-flat using defining equations.
            List<Point[]> globalOriginalPoints = new List<Point[]>();
            List<Point[]> globalTransposedPoints = new List<Point[]>();
            double offset = axesCoordinates[0];//Alfred's formula is obtained for axes with X1=0.
            foreach (double[][] triangle in triangles)
            {
                if (triangle == null)
                {//encountered during scaling via the viewport, for some reason. investigate at some point...
                    continue;
                }
                Point[] originalPoints = new Point[originalPlanePointsToCompute.Count];
                Point[] transposedPoints = new Point[transposedPLanePointsToCompute.Count];
                for (int i = 0; i < originalPlanePointsToCompute.Count; i++)
                {
                    Tuple<int, int, int> indices = originalPlanePointsToCompute[i];
                    double[] coefficients = planeCoefficientsFromTriangle3D(triangleProjection(triangle, indices));
                    double c1 = coefficients[0];
                    double c2 = coefficients[1];
                    double c3 = coefficients[2];
                    double c0 = coefficients[3];
                    double sumCoefs = c1 + c2 + c3;
                    if (sumCoefs == 0)
                        originalPoints[i] = new Point(Double.PositiveInfinity, Double.PositiveInfinity);
                    else
                        originalPoints[i] = new Point((c2 + 2 * c3) / sumCoefs + offset, c0 / sumCoefs);
                }
                //warning - some code repretition...
                for (int i = 0; i < transposedPLanePointsToCompute.Count; i++)
                {
                    Tuple<int, int, int> indices = transposedPLanePointsToCompute[i];
                    double[] coefficients = planeCoefficientsFromTriangle3D(triangleProjection(triangle, indices));
                    double c1 = coefficients[0];
                    double c2 = coefficients[1];
                    double c3 = coefficients[2];
                    double c0 = coefficients[3];
                    double sumCoefs = c1 + c2 + c3;
                    if (sumCoefs == 0)
                        transposedPoints[i] = new Point(Double.PositiveInfinity, Double.PositiveInfinity);
                    else
                        transposedPoints[i] = new Point((3 * c1 + c2 + 2 * c3) / sumCoefs + offset, c0 / sumCoefs);
                }
                globalOriginalPoints.Add(originalPoints);
                globalTransposedPoints.Add(transposedPoints);
            }
            return new List<Point[]>[] { globalOriginalPoints, globalTransposedPoints };
        }

        //obsolete method (3D planes only). Delete after refactoring is finished.
        public List<Point>[] get3DPlanePointsFromTriangles(List<Point3D[]> triangles)
        {
            List<Point> originalPoints = new List<Point>();//don't use hashsets -- for Iman's CSV writes, I want an ordering on the red/green
            List<Point> transposedPoints = new List<Point>();//dots, so that I can treat sibling pairs.

            foreach (Point3D[] triangle in triangles)
            {
                if (triangle == null)
                    continue;
                double[] coefficients = planeCoefficientsFromTriangle3D(triangle);
                double c1 = coefficients[0];
                double c2 = coefficients[1];
                double c3 = coefficients[2];
                double c0 = coefficients[3];

                double denominator = c1 + c2 + c3;
                if (denominator == 0) continue;//ideal points skipped
                double offset = axesCoordinates[0];//this formula is obtained for axes with X1=0.
                originalPoints.Add(new Point((c2 + 2 * c3) / denominator + offset, c0 / denominator));//roughly page 152 from Alfred's book
                transposedPoints.Add(new Point((3 * c1 + c2 + 2 * c3) / denominator + offset, c0 / denominator));
            }
            return new List<Point>[] { originalPoints, transposedPoints };
        }
    }
}