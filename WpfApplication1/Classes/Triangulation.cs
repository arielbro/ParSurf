using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics;




namespace Triangulation
{

    public class Point
    {

        /*self-explanatory*/




        public double xCoordinate;

        public double yCoordinate;

        public double zCoordinate;




        public Point()
        {

            xCoordinate = 0;

            yCoordinate = 0;

            zCoordinate = 0;

        }

        public Point(double x, double y, double z)
        {

            xCoordinate = x;

            yCoordinate = y;

            zCoordinate = z;

        }

        public Point(Tuple<double, double, double> P)
        {

            xCoordinate = P.Item1;

            yCoordinate = P.Item2;

            zCoordinate = P.Item3;

        }

        public double dist(Point B)
        {

            return Math.Pow(Math.Pow(this.xCoordinate - B.xCoordinate, 2) + Math.Pow(this.yCoordinate - B.yCoordinate, 2) + Math.Pow(this.zCoordinate - B.zCoordinate, 2), 0.5);

        }

        public Point Copy()
        {

            return new Point(xCoordinate, yCoordinate, zCoordinate);

        }

        public override bool Equals(object other)
        {
            if (other.GetType() != typeof(Point))
                return false;
            Point B = other as Point;
            if ((this.xCoordinate == B.xCoordinate) && (this.yCoordinate == B.yCoordinate) && (this.zCoordinate == B.zCoordinate))
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return (int)(13 * xCoordinate + 29 * yCoordinate + 17 * zCoordinate);
        }

        public static HashSet<Point> tuplesToPoints(HashSet<Tuple<double, double, double>> tupleSet)
        {

            /*tuples to points, sets to sets*/

            HashSet<Point> PointSet = new HashSet<Point>();

            foreach (Tuple<double, double, double> T in tupleSet)

                PointSet.Add(new Point(T));

            return PointSet;

        }

        public static HashSet<Point> doubleArraysToPoints(IEnumerable<double[]> doubleArrays)
        {
            HashSet<Point> points = new HashSet<Point>();
            foreach (double[] doubleArray in doubleArrays)
                points.Add(new Point(doubleArray[0], doubleArray[1], doubleArray[2]));
            return points;
        }

        public void print()
        {

            Console.WriteLine("(" + this.xCoordinate + "," + this.yCoordinate + "," + this.zCoordinate + ")");

        }

        public double[] convertToDoubleArray()
        {
            return new double[] { xCoordinate, yCoordinate, zCoordinate };
        }





    }

    public class Edge
    {

        Point pointA;

        Point pointB;

        public Edge(Point A, Point B)
        {

            pointA = A;

            pointB = B;

        }

        public Edge() : this(new Point(), new Point()) { }




        public double len()
        {

            return pointA.dist(pointB);

        }

        public Boolean Equals(Edge B)
        {

            /*is it?*/

            return ((pointA.Equals(B.pointA)) && (pointB.Equals(B.pointB))) || ((pointA.Equals(B.pointB)) && (pointB.Equals(B.pointA)));

        }

    }




    public class Triangle
    {

        public Point pointA;

        public Point pointB;

        public Point pointC;




        public Triangle()
            : this(new Point(), new Point(), new Point())
        {

        }

        public Triangle(Point A, Point B, Point C)
        {

            pointA = A;

            pointB = B;

            pointC = C;

        }
        public Boolean hasPoint(Point P)
        {
            if (this.pointA.Equals(P) || this.pointB.Equals(P) || this.pointC.Equals(P))
                return true;
            return false;
        }
        public void print()
        {

            Console.WriteLine("+++++");

            pointA.print();

            pointB.print();

            pointC.print();

            Console.WriteLine("+++++");

        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Triangle))
                return false;
            Triangle T = obj as Triangle;
            if ((pointA.Equals(T.pointA)) && (((pointB.Equals(T.pointB)) && (pointC.Equals(T.pointC))) || ((pointB.Equals(T.pointC)) && (pointC.Equals(T.pointB)))))

                return true;

            if ((pointA.Equals(T.pointB)) && (((pointB.Equals(T.pointC)) && (pointC.Equals(T.pointA))) || ((pointB.Equals(T.pointA)) && (pointC.Equals(T.pointC)))))

                return true;

            if ((pointA.Equals(T.pointC)) && (((pointB.Equals(T.pointA)) && (pointC.Equals(T.pointB))) || ((pointB.Equals(T.pointB)) && (pointC.Equals(T.pointA)))))

                return true;

            return false;

        }
        public override int GetHashCode()
        {
            return pointA.GetHashCode() + pointB.GetHashCode() + pointC.GetHashCode();
        }
        public double[][] convertToDoubleArrays()
        {
            return new double[][]{pointA.convertToDoubleArray(),pointB.convertToDoubleArray(),
                                 pointC.convertToDoubleArray()};
        }

        public static List<double[][]> convertTrianglesToListOfDoubleArrays(IEnumerable<Triangle> triangles)
        {
            List<double[][]> res = new List<double[][]>();
            foreach (Triangle triangle in triangles)
            {
                res.Add(triangle.convertToDoubleArrays());
            }
            return res;
        }


    }

    public class Tetrahedron
    {

        public Point pointA;
        public Point pointB;
        public Point pointC;
        public Point pointD;
        public Point centerPoint;
        public double tetrahedronRadius;

        public Tetrahedron(Point A, Point B, Point C, Point D)
        {
            pointA = A;
            pointB = B;
            pointC = C;
            pointD = D;
            centerPoint = new Point();
            tetrahedronRadius = 0;
        }

        public Tetrahedron()
        {

            pointD = new Point();
            centerPoint = new Point();
            tetrahedronRadius = 0;

        }

        public override int GetHashCode()
        {
            return pointA.GetHashCode() + pointB.GetHashCode() + pointC.GetHashCode() + pointD.GetHashCode();
        }


        public static Tetrahedron TetrahedronFromTetrahedronbByFacets(TetrahedronByFacets T)
        {

            Tetrahedron newTetrahedron = new Tetrahedron();

            newTetrahedron.pointA = T.A.pointA;

            newTetrahedron.pointB = T.A.pointB;

            newTetrahedron.pointC = T.A.pointC;

            if (!(T.B.pointA.Equals(T.A.pointA) || T.B.pointA.Equals(T.A.pointB) || T.B.pointA.Equals(T.A.pointC)))

                newTetrahedron.pointD = T.B.pointA;

            else if (!(T.B.pointB.Equals(T.A.pointA) || T.B.pointB.Equals(T.A.pointB) || T.B.pointB.Equals(T.A.pointC)))

                newTetrahedron.pointD = T.B.pointB;

            else

                newTetrahedron.pointD = T.B.pointC;




            return newTetrahedron;

        }

        public Point center()
        {

            /* finding the center of the circumsphere is not for the weak-kneed of us, 

             * but fot the brave of you, this is the place to go to 

             * http://mathworld.wolfram.com/Circumsphere.html   */


            if (!centerPoint.Equals(new Point()))
                return centerPoint;


            Point center = new Point();




            double aDet = Calculate_a();




            if (aDet == 0)
            {

                Console.WriteLine("div by zero");

                //this.print();

            }

            center.xCoordinate = CalculateDx() / (2 * aDet);

            center.yCoordinate = CalculateDy() / (2 * aDet);

            center.zCoordinate = CalculateDz() / (2 * aDet);


            this.centerPoint = center;

            return center;



        }

        public double Volume()
        {

            return (Math.Abs(this.Calculate_a()) / 6.0);

        }

        public double radius()
        {

            /*here we measure the length of the circumsphere's radius*/
            if (tetrahedronRadius != 0)
                return tetrahedronRadius;

            double delta = Math.Pow(this.CalculateDx(), 2) + Math.Pow(this.CalculateDy(), 2) + Math.Pow(this.CalculateDz(), 2) - 4 * Calculate_a() * Calculate_c();

            if (delta < 0)
                return 0;
            /*

           {

               Console.WriteLine("mistake, delta=" + delta +  + Calculate_a() + Calculate_c() );

               Console.WriteLine("Dx: " + this.CalculateDx() + " Dy: " + this.CalculateDy() + " Dz: " + this.CalculateDz());

               Console.WriteLine("a: " + this.Calculate_a() + " c: " + this.Calculate_c() );

               this.print();

           }*/

            double a = Calculate_a();

            if (a == 0)
            {

                Console.WriteLine("div by zero");

                //this.print();

            }

            if (a < 0)

                a *= -1.0;
            this.tetrahedronRadius = Math.Sqrt(delta) / (2 * a);
            return Math.Sqrt(delta) / (2 * a);

        }

        /* auxiliary functions pretty much to the end */

        public double CalculateDx()
        {



            double[,] matrixX = new double[4, 4];




            fillRowX(ref matrixX, this.pointA, 0);

            fillRowX(ref matrixX, this.pointB, 1);

            fillRowX(ref matrixX, this.pointC, 2);

            fillRowX(ref matrixX, this.pointD, 3);




            return determinant(matrixX);




        }

        public double CalculateDy()
        {

            double[,] matrixY = new double[4, 4];




            fillRowY(ref matrixY, this.pointA, 0);

            fillRowY(ref matrixY, this.pointB, 1);

            fillRowY(ref matrixY, this.pointC, 2);

            fillRowY(ref matrixY, this.pointD, 3);




            return (-1.0) * determinant(matrixY);

        }

        public double CalculateDz()
        {

            double[,] matrixZ = new double[4, 4];




            fillRowZ(ref matrixZ, this.pointA, 0);

            fillRowZ(ref matrixZ, this.pointB, 1);

            fillRowZ(ref matrixZ, this.pointC, 2);

            fillRowZ(ref matrixZ, this.pointD, 3);




            return determinant(matrixZ);

        }

        public double Calculate_a()
        {

            double[,] matrix_a = new double[4, 4];




            fillRow_a(ref matrix_a, this.pointA, 0);

            fillRow_a(ref matrix_a, this.pointB, 1);

            fillRow_a(ref matrix_a, this.pointC, 2);

            fillRow_a(ref matrix_a, this.pointD, 3);

            return determinant(matrix_a);

        }

        public double Calculate_c()
        {

            double[,] matrix_c = new double[4, 4];




            fillRow_c(ref matrix_c, this.pointA, 0);

            fillRow_c(ref matrix_c, this.pointB, 1);

            fillRow_c(ref matrix_c, this.pointC, 2);

            fillRow_c(ref matrix_c, this.pointD, 3);




            return determinant(matrix_c);

        }

        public static void fillRowX(ref double[,] matrix, Point p, int Row)
        {

            matrix[Row, 0] = Math.Pow(p.xCoordinate, 2) + Math.Pow(p.yCoordinate, 2) + Math.Pow(p.zCoordinate, 2);

            matrix[Row, 1] = p.yCoordinate;

            matrix[Row, 2] = p.zCoordinate;

            matrix[Row, 3] = 1;




        }

        public static void fillRowY(ref double[,] matrix, Point p, int Row)
        {

            matrix[Row, 0] = Math.Pow(p.xCoordinate, 2) + Math.Pow(p.yCoordinate, 2) + Math.Pow(p.zCoordinate, 2);

            matrix[Row, 1] = p.xCoordinate;

            matrix[Row, 2] = p.zCoordinate;

            matrix[Row, 3] = 1;




        }

        public static void fillRowZ(ref double[,] matrix, Point p, int Row)
        {

            matrix[Row, 0] = Math.Pow(p.xCoordinate, 2) + Math.Pow(p.yCoordinate, 2) + Math.Pow(p.zCoordinate, 2);

            matrix[Row, 1] = p.xCoordinate;

            matrix[Row, 2] = p.yCoordinate;

            matrix[Row, 3] = 1;




        }

        public static void fillRow_a(ref double[,] matrix, Point p, int Row)
        {

            matrix[Row, 0] = p.xCoordinate;

            matrix[Row, 1] = p.yCoordinate;

            matrix[Row, 2] = p.zCoordinate;

            matrix[Row, 3] = 1;




        }

        public static void fillRow_c(ref double[,] matrix, Point p, int Row)
        {

            matrix[Row, 0] = Math.Pow(p.xCoordinate, 2) + Math.Pow(p.yCoordinate, 2) + Math.Pow(p.zCoordinate, 2);

            matrix[Row, 1] = p.xCoordinate;

            matrix[Row, 2] = p.yCoordinate;

            matrix[Row, 3] = p.zCoordinate;




        }

        public static double determinant(double[,] m)
        {

            /* our old friend, the determinant, unembellished*/

            return

               m[0, 3] * m[1, 2] * m[2, 1] * m[3, 0] - m[0, 2] * m[1, 3] * m[2, 1] * m[3, 0] -

               m[0, 3] * m[1, 1] * m[2, 2] * m[3, 0] + m[0, 1] * m[1, 3] * m[2, 2] * m[3, 0] +

               m[0, 2] * m[1, 1] * m[2, 3] * m[3, 0] - m[0, 1] * m[1, 2] * m[2, 3] * m[3, 0] -

               m[0, 3] * m[1, 2] * m[2, 0] * m[3, 1] + m[0, 2] * m[1, 3] * m[2, 0] * m[3, 1] +

               m[0, 3] * m[1, 0] * m[2, 2] * m[3, 1] - m[0, 0] * m[1, 3] * m[2, 2] * m[3, 1] -

               m[0, 2] * m[1, 0] * m[2, 3] * m[3, 1] + m[0, 0] * m[1, 2] * m[2, 3] * m[3, 1] +

               m[0, 3] * m[1, 1] * m[2, 0] * m[3, 2] - m[0, 1] * m[1, 3] * m[2, 0] * m[3, 2] -

               m[0, 3] * m[1, 0] * m[2, 1] * m[3, 2] + m[0, 0] * m[1, 3] * m[2, 1] * m[3, 2] +

               m[0, 1] * m[1, 0] * m[2, 3] * m[3, 2] - m[0, 0] * m[1, 1] * m[2, 3] * m[3, 2] -

               m[0, 2] * m[1, 1] * m[2, 0] * m[3, 3] + m[0, 1] * m[1, 2] * m[2, 0] * m[3, 3] +

               m[0, 2] * m[1, 0] * m[2, 1] * m[3, 3] - m[0, 0] * m[1, 2] * m[2, 1] * m[3, 3] -

               m[0, 1] * m[1, 0] * m[2, 2] * m[3, 3] + m[0, 0] * m[1, 1] * m[2, 2] * m[3, 3];

        }

        new public void print()
        {

            Console.WriteLine("****");

            pointA.print();

            pointB.print();

            pointC.print();

            pointD.print();

            Console.WriteLine("****");

        }

    }

    public class TetrahedronByFacets
    {

        /* just another representation */

        public Triangle A;

        public Triangle B;

        public Triangle C;

        public Triangle D;




        public TetrahedronByFacets(Triangle a, Triangle b, Triangle c, Triangle d)
        {

            this.A = a;

            this.B = b;

            this.C = c;

            this.D = d;

        }

        public TetrahedronByFacets()

            : this(new Triangle(), new Triangle(), new Triangle(), new Triangle())
        {

        }

        public static TetrahedronByFacets convertFromTetrahedron(Tetrahedron T)
        {

            Triangle A = new Triangle(T.pointA, T.pointB, T.pointC);

            Triangle B = new Triangle(T.pointA, T.pointB, T.pointD);

            Triangle C = new Triangle(T.pointA, T.pointC, T.pointD);

            Triangle D = new Triangle(T.pointB, T.pointC, T.pointD);

            return new TetrahedronByFacets(A, B, C, D);

        }

        public Boolean hasFacet(Triangle T)
        {

            if (this.A.Equals(T) || this.B.Equals(T) || this.C.Equals(T) || this.D.Equals(T))

                return true;

            return false;

        }

        public void print()
        {

            Tetrahedron.TetrahedronFromTetrahedronbByFacets(this).print();

        }




    }




    class Program
    {
        /*
        static void Main(string[] args)

        {

            HashSet<Tuple<double, double, double>> testSet = new HashSet<Tuple<double, double, double>> { new Tuple<double, double, double>(0, 0, 0), new Tuple<double, double, double>(0, 0, 1), new Tuple<double, double, double>(0, 1, 0), new Tuple<double, double, double>(0, 1, 1), new Tuple<double, double, double>(1, 0, 0), new Tuple<double, double, double>(1, 0, 1), new Tuple<double, double, double>(1, 1, 0), new Tuple<double, double, double>(1, 1, 1) };//, new Tuple<double, double, double>(0.5, 0.5, 0.5) };

            /*Triangle T1 = new Triangle(new Point(0, 0, 0), new Point(0, 1, 0), new Point(0, 0, 1));

            Point P1 = new Point(0, 0.4, 0.5);

            if (onTheSamePlane(T1, P1))

                Console.WriteLine("on the same plane");

            if (pointInTriangle(T1,P1))

                Console.WriteLine("in triangle");

            Console.WriteLine("area of T:" + areaOfTriangle(T1));

            foreach (Point P in Point.tuplesToPoints(testSet))

                P.print();

             /

            //HashSet<Tetrahedron> triangulation = triangulate(Point.tuplesToPoints(testSet));

            Console.WriteLine("**results**");




            HashSet<Triangle> triangulation = triangulateAlternative(Point.tuplesToPoints(testSet));

            foreach (Triangle T in triangulation)

                T.print();

            Console.WriteLine("number of triangles:" + triangulation.Count());




        }*/

        static double maximalMinimalDistance(IEnumerable<Point> points)
        {
            /* 
             * Given a set of points, returns the maximal distance between a point in it to its nearest neighbor.
             * Used to heuristically define which points are reasonable neighbors and which are not.
             * */
            double maxMin = double.MinValue;
            List<Point> pointsList = points.ToList();
            for (int i = 0; i < pointsList.Count - 1; i++)
            {
                Point curPoint = pointsList[i];
                double minDist = double.MaxValue;
                foreach (Point otherPoint in points)
                    if (!otherPoint.Equals(curPoint))
                        minDist = Math.Min(minDist, curPoint.dist(otherPoint));
                maxMin = Math.Max(maxMin, minDist);
            }
            return maxMin;
        }

        static void eliminateCover(ref HashSet<Tetrahedron> triangulation, Tetrahedron Cover)
        {

            HashSet<Tetrahedron> badTetrahedrons = new HashSet<Tetrahedron>();

            foreach (Tetrahedron T in triangulation)
            {

                if (haveCommonVertex(T, Cover))
                    badTetrahedrons.Add(T);

            }

            foreach (Tetrahedron T in badTetrahedrons)
                triangulation.Remove(T);

        }
        static Boolean haveCommonVertex(Tetrahedron A, Tetrahedron B)
        {

            if (hasTheVertex(A, B.pointA) || hasTheVertex(A, B.pointB) || hasTheVertex(A, B.pointC) || hasTheVertex(A, B.pointD))
                return true;

            return false;

        }

        static Boolean hasTheVertex(Tetrahedron T, Point P)
        {

            if (P.Equals(T.pointA) || P.Equals(T.pointB) || P.Equals(T.pointC) || P.Equals(T.pointD))
                return true;

            return false;

        }

        static Boolean pointInTriangle(Triangle T, Point P)
        {

            //given they are on the same plane

            double eps = 0.00000001;

            Triangle A = new Triangle(T.pointA, T.pointB, P);

            Triangle B = new Triangle(T.pointA, T.pointC, P);

            Triangle C = new Triangle(T.pointC, T.pointB, P);

            return ((areaOfTriangle(A) + areaOfTriangle(B) + areaOfTriangle(C) - areaOfTriangle(T)) < eps * areaOfTriangle(T));

        }

        static Boolean PointInTetrahedron(Tetrahedron T, Point P)
        {

            double epsilon = 0.000000001;

            Tetrahedron A = new Tetrahedron(T.pointA, T.pointB, T.pointC, P);

            Tetrahedron B = new Tetrahedron(T.pointA, T.pointB, T.pointD, P);

            Tetrahedron C = new Tetrahedron(T.pointA, T.pointD, T.pointC, P);

            Tetrahedron D = new Tetrahedron(T.pointD, T.pointB, T.pointC, P);

            //P.print();

            //Console.WriteLine(A.Volume() + B.Volume() + C.Volume() + D.Volume() + "   " + T.Volume());

            return (A.Volume() + B.Volume() + C.Volume() + D.Volume() - T.Volume() < epsilon * T.Volume());

        }

        public static HashSet<Point> uniqueOnly(HashSet<Point> Points)
        {
            HashSet<Point> uniquePoints = new HashSet<Point>();
            Boolean add = true;
            foreach (Point P in Points)
            {
                if (uniquePoints.Contains(P))
                    add = false;
                if (add)
                    uniquePoints.Add(P);
                add = true;
            }
            return uniquePoints;
        }
        public static HashSet<Triangle> triangulateAlternative2(HashSet<Point> Points)
        {
            int i = 0;
            Points = uniqueOnly(Points);
            HashSet<TetrahedronByFacets> triangulation = new HashSet<TetrahedronByFacets>();

            TetrahedronByFacets Cover = TetrahedronByFacets.convertFromTetrahedron(CoverTetrahedron(Points));

            triangulation.Add(Cover);

            /*

            Console.Write("Cover:");

            Cover.print();

            Console.WriteLine(Points.Count);

            */

            foreach (Point P in Points)
            {

                HashSet<TetrahedronByFacets> newTetrahedrons = new HashSet<TetrahedronByFacets>();
                HashSet<TetrahedronByFacets> oldTetrahedrons = new HashSet<TetrahedronByFacets>();

                //Debug.Write(triangulation.Count+"  ");
                i += 1;
                foreach (TetrahedronByFacets T in triangulation)
                {

                    doPoint(ref newTetrahedrons, ref triangulation, ref oldTetrahedrons, P, T);
                }



                //   Console.WriteLine("/*/");

                //   Console.WriteLine(newTetrahedrons.Count);




                foreach (TetrahedronByFacets T in newTetrahedrons)
                {
                    triangulation.Add(T);
                }
                foreach (TetrahedronByFacets T in oldTetrahedrons)
                {

                    triangulation.Remove(T);
                }
            }

            /*

            Console.Write("before:");

            foreach (TetrahedronByFacets T in triangulation)

                T.print();

            Console.WriteLine(triangulation.Count);

            */

            //deleteCover(ref triangulation, Cover);




            /*




            Console.Write("after:");

            foreach (TetrahedronByFacets T in triangulation)

                T.print();

            */


            return deleteCover2(facets(ref triangulation), Cover);

            //return uniqueFacets(ref triangulation);

            //return deleteCover2( uniqueFacets(ref triangulation), Cover);


        }
        static HashSet<Triangle> deleteCover2(HashSet<Triangle> triangulation, TetrahedronByFacets Cover)
        {

            HashSet<Triangle> temp = new HashSet<Triangle>();
            Tetrahedron tempCover = Tetrahedron.TetrahedronFromTetrahedronbByFacets(Cover);
            foreach (Triangle T in triangulation)
            {

                if (T.hasPoint(tempCover.pointA) || T.hasPoint(tempCover.pointB) || T.hasPoint(tempCover.pointC) || T.hasPoint(tempCover.pointD))
                    continue;
                temp.Add(T);

            }

            return temp;

        }
        static void deleteCover(ref HashSet<TetrahedronByFacets> triangulation, TetrahedronByFacets Cover)
        {

            HashSet<TetrahedronByFacets> temp = new HashSet<TetrahedronByFacets>();

            foreach (TetrahedronByFacets T in triangulation)
            {

                if (T.hasFacet(Cover.A) || T.hasFacet(Cover.B) || T.hasFacet(Cover.C) || T.hasFacet(Cover.D))

                    temp.Add(T);

            }

            foreach (TetrahedronByFacets T in temp)

                triangulation.Remove(T);

        }
        static HashSet<Triangle> facets(ref HashSet<TetrahedronByFacets> triangulation)
        {

            Dictionary<Triangle, int> Triangles = new Dictionary<Triangle, int>();

            HashSet<Triangle> result = new HashSet<Triangle>();

            foreach (TetrahedronByFacets T in triangulation)

                addTetraToDict(ref Triangles, T);

            foreach (KeyValuePair<Triangle, int> Element in Triangles)
            {

                // if (Element.Value == 1)

                result.Add(Element.Key);

            }

            return result;

        }
        static IEnumerable<Triangle> uniqueFacets(ref HashSet<TetrahedronByFacets> triangulation)
        {
            HashSet<Triangle> triangles = new HashSet<Triangle>();
            HashSet<Triangle> duplicateTriangles = new HashSet<Triangle>();
            foreach (TetrahedronByFacets T in triangulation)
                foreach (Triangle triangle in new Triangle[] { T.A, T.B, T.C, T.D })
                {
                    if (triangles.Contains(triangle))
                        duplicateTriangles.Add(triangle);
                    triangles.Add(triangle);
                }
            return triangles.Except(duplicateTriangles);
        }

        static void addTetraToDict(ref Dictionary<Triangle, int> Triangles, TetrahedronByFacets T)
        {

            addTriangleToDict(ref Triangles, T.A);

            addTriangleToDict(ref Triangles, T.B);

            addTriangleToDict(ref Triangles, T.C);

            addTriangleToDict(ref Triangles, T.D);

        }

        static void addTriangleToDict(ref Dictionary<Triangle, int> Triangles, Triangle T)
        {

            Triangle foundKey = null;

            foreach (KeyValuePair<Triangle, int> Element in Triangles)
            {

                if (Element.Key.Equals(T))
                {

                    foundKey = Element.Key;

                    break;

                }

            }

            if (foundKey != null)

                Triangles[foundKey] += 1;

            else

                Triangles.Add(T, 1);

        }

        /*

        static void newTetraPointOnEdge(HashSet<TetrahedronByFacets> newTetrahedrons, HashSet<TetrahedronByFacets> existingTetrahedrons, 

        {




        }*/

        static void doPoint(ref HashSet<TetrahedronByFacets> newTetrahedrons, ref HashSet<TetrahedronByFacets> existingTetrahedrons, ref HashSet<TetrahedronByFacets> oldTetrahedrons, Point P, TetrahedronByFacets T)
        {

            Tetrahedron newT = Tetrahedron.TetrahedronFromTetrahedronbByFacets(T);

            if (pointOnEdge(P, newT.pointA, newT.pointB))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointA, newT.pointC, newT.pointD)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointB, newT.pointC, newT.pointD)));

                oldTetrahedrons.Add(T);

            }

            else if (pointOnEdge(P, newT.pointA, newT.pointC))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointA, newT.pointB, newT.pointD)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointC, newT.pointB, newT.pointD)));

                oldTetrahedrons.Add(T);

            }

            else if (pointOnEdge(P, newT.pointA, newT.pointD))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointA, newT.pointC, newT.pointB)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointD, newT.pointC, newT.pointB)));

                oldTetrahedrons.Add(T);

            }

            else if (pointOnEdge(P, newT.pointC, newT.pointB))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointC, newT.pointA, newT.pointD)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointB, newT.pointA, newT.pointD)));

                oldTetrahedrons.Add(T);

            }

            else if (pointOnEdge(P, newT.pointC, newT.pointD))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointC, newT.pointA, newT.pointB)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointD, newT.pointA, newT.pointB)));

                oldTetrahedrons.Add(T);

            }

            else if (pointOnEdge(P, newT.pointD, newT.pointB))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointD, newT.pointC, newT.pointA)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, newT.pointB, newT.pointC, newT.pointA)));

                oldTetrahedrons.Add(T);

            }

            else if (pointInTriangle(T.A, P))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.B.pointA, T.B.pointB, T.B.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.C.pointA, T.C.pointB, T.C.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.D.pointA, T.D.pointB, T.D.pointC)));

                oldTetrahedrons.Add(T);

            }

            else if (pointInTriangle(T.B, P))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.A.pointA, T.A.pointB, T.A.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.C.pointA, T.C.pointB, T.C.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.D.pointA, T.D.pointB, T.D.pointC)));

                oldTetrahedrons.Add(T);

            }

            else if (pointInTriangle(T.C, P))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.B.pointA, T.B.pointB, T.B.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.A.pointA, T.A.pointB, T.A.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.D.pointA, T.D.pointB, T.D.pointC)));

                oldTetrahedrons.Add(T);

            }

            else if (pointInTriangle(T.D, P))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.B.pointA, T.B.pointB, T.B.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.C.pointA, T.C.pointB, T.C.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.A.pointA, T.A.pointB, T.A.pointC)));

                oldTetrahedrons.Add(T);

            }

            else if (PointInTetrahedron(newT, P))
            {

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.A.pointA, T.A.pointB, T.A.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.B.pointA, T.B.pointB, T.B.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.C.pointA, T.C.pointB, T.C.pointC)));

                newTetrahedrons.Add(TetrahedronByFacets.convertFromTetrahedron(new Tetrahedron(P, T.D.pointA, T.D.pointB, T.D.pointC)));

                oldTetrahedrons.Add(T);

            }

            //It Is Not A Code Duplication!!It Is Not A Code Duplication!!It Is Not A Code Duplication!!It Is Not A Code Duplication!!It Is Not A Code Duplication!!




            //foreach (TetrahedronByFacets Tet in newTetrahedrons)

            //    Tet.print();



        }

        static Boolean pointOnEdge(Point P, Point A, Point B)
        {

            double epsilon = 0.00000001;

            return (P.dist(A) + P.dist(B) - A.dist(B) < epsilon * A.dist(B));

        }

        static double areaOfTriangle(Triangle T)
        {

            Point AB = new Point(T.pointB.xCoordinate - T.pointA.xCoordinate, T.pointB.yCoordinate - T.pointA.yCoordinate, T.pointB.zCoordinate - T.pointA.zCoordinate);

            Point AC = new Point(T.pointC.xCoordinate - T.pointA.xCoordinate, T.pointC.yCoordinate - T.pointA.yCoordinate, T.pointC.zCoordinate - T.pointA.zCoordinate);

            double res = Math.Pow(AB.yCoordinate * AC.zCoordinate - AB.zCoordinate * AC.yCoordinate, 2) + Math.Pow(AB.zCoordinate * AC.xCoordinate - AB.xCoordinate * AC.zCoordinate, 2) + Math.Pow(AB.xCoordinate * AC.yCoordinate - AB.yCoordinate * AC.xCoordinate, 2);

            return 0.5 * Math.Pow(res, 0.5);

        }

        public static IEnumerable<Triangle> triangulateAlternative(HashSet<Point> Points)
        {

            /*prologue

             * the idea is to contain the set of point in a tetrahedron and add points to the triangulation one-by-one,

             * each time eliminating tetrahedrons in the triangulation that aren't coherent with the new point and creating

             * new ones instead.

             * for more: http://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm

             */

            HashSet<Tetrahedron> triangulation = new HashSet<Tetrahedron>();

            Points = uniqueOnly(Points);
            double epsilon = 0.0000001;
            double maxMinDistance = maximalMinimalDistance(Points);
            double maximalAllowedRadius = 3 * maxMinDistance + epsilon;

            Tetrahedron Cover = CoverTetrahedron(Points);

            //Cover.print();
            /*
            Tetrahedron check = new Tetrahedron(new Point(0, 0, 0), new Point(0, 0, 1), new Point(0, 1, 0), new Point(1, 0, 0));
            double a = check.Volume();
            Point b = check.center();
            double c = check.radius();
            Point d = new Point(1,1,0);
            double e = b.dist(d);
            Tetrahedron check2 = new Tetrahedron(new Point(0, 0, 0), new Point(-18, 41, -55), new Point(0, 1, 0), new Point(1, 0, 0));
            Point b2 = check2.center();
            double c2 = check2.radius();
            double e2 = b2.dist(d);*/
            triangulation.Add(Cover);
            int i = 0;
            double dist, radius;
            Point center;
            Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();
            Stopwatch sw3 = new Stopwatch();
            Stopwatch sw4 = new Stopwatch();
            Stopwatch sw5 = new Stopwatch();
            Stopwatch sw6 = new Stopwatch();
            Stopwatch sw7 = new Stopwatch();
            Stopwatch sw8 = new Stopwatch();
            sw3.Start();
            foreach (Point P in Points)
            {
                ++i;

                HashSet<Tetrahedron> badTetrahedrons = new HashSet<Tetrahedron>();


                /* chapter 3 in which we find for each tetrahedron in the triangulation if the new point is in its circumsphere*/
                sw1.Start();
                foreach (Tetrahedron T in triangulation)
                {
                    center = T.center();
                    dist = center.dist(P);
                    radius = T.radius();
                    if ((dist - radius) < epsilon)
                        badTetrahedrons.Add(T);
                }

                sw1.Stop();
                /* chapter 4 in which we the border of the area in the triangulation damaged by the new point*/

                HashSet<Triangle> polygon = new HashSet<Triangle>();

                sw2.Start();
                foreach (Tetrahedron T in badTetrahedrons)
                {

                    TetrahedronByFacets F = TetrahedronByFacets.convertFromTetrahedron(T);
                    /*
                    polygon.Add(F.A);
                                        
                    polygon.Add(F.B);
                                        
                    polygon.Add(F.C);
                                       
                    polygon.Add(F.D);
                    */
                    addToPolygon(ref polygon, F.A);
                    addToPolygon(ref polygon, F.B);
                    addToPolygon(ref polygon, F.C);
                    addToPolygon(ref polygon, F.D);
                }
                sw2.Stop();
                //clearPolygon(ref polygon);
                /* chapter 5 in which we fix the damaged area and add new tetrahedrons*/




                sw4.Start();
                foreach (Triangle T in polygon)
                {

                    Tetrahedron newTetrahedron = new Tetrahedron(T.pointA, T.pointB, T.pointC, P);

                    if (newTetrahedron.Volume() < 0.0000000000001)
                    {
                        HashSet<Tetrahedron> temp = new HashSet<Tetrahedron>();
                        HashSet<Tetrahedron> temp2 = new HashSet<Tetrahedron>();
                        foreach (Tetrahedron tet in triangulation)
                        {
                            TetrahedronByFacets tempTet = TetrahedronByFacets.convertFromTetrahedron(tet);
                            if (T.Equals(tempTet.A) || T.Equals(tempTet.B) || T.Equals(tempTet.C) || T.Equals(tempTet.D))
                                temp.Add(tet);


                        }

                        foreach (Tetrahedron bad in badTetrahedrons)
                        {
                            TetrahedronByFacets tempbad = TetrahedronByFacets.convertFromTetrahedron(bad);
                            if (T.Equals(tempbad.A) || T.Equals(tempbad.B) || T.Equals(tempbad.C) || T.Equals(tempbad.D))
                                temp2.Add(bad);

                        }
                        double dist1 = temp.ToList()[0].center().dist(P) - temp.ToList()[0].radius();
                        double dist2 = temp.ToList()[1].center().dist(P) - temp.ToList()[1].radius();
                        throw new Exception("triangulation failed - tetrahedron with zero volume");
                    }
                    if (newTetrahedron.radius() == 0)
                    {
                        double what = newTetrahedron.Volume();
                        throw new Exception("triangulation failed - tetrahedron with zero radius");
                    }
                    TetrahedronByFacets newTetFacets = TetrahedronByFacets.convertFromTetrahedron(newTetrahedron);
                    TetrahedronByFacets coverByFacets = TetrahedronByFacets.convertFromTetrahedron(Cover);

                    triangulation.Add(newTetrahedron);


                }

                sw4.Stop();
                foreach (Tetrahedron T in badTetrahedrons)
                    triangulation.Remove(T);


            }

            sw3.Stop();
            eliminateCover(ref triangulation, Cover);

            HashSet<TetrahedronByFacets> triangulationByFacets = new HashSet<TetrahedronByFacets>();
            foreach (Tetrahedron T in triangulation)
                triangulationByFacets.Add(TetrahedronByFacets.convertFromTetrahedron(T));
            Console.WriteLine("checking the tetrahedrons={0}", sw1.Elapsed);
            Console.WriteLine("adding triangles to polygon={0}", sw2.Elapsed);
            Console.WriteLine("the whole function={0}", sw3.Elapsed);
            Console.WriteLine("adding new triangles={0}", sw4.Elapsed);
            sw5.Start();
            int iteration = 0;
            int maxIterations = 50;
            while(true)
            {
                
                HashSet<Triangle> front = new HashSet<Triangle>(uniqueFacets(ref triangulationByFacets));
                int numOfTets = triangulationByFacets.Count();
                foreach (TetrahedronByFacets tet in triangulationByFacets.ToList())
                {
                    //find tetrahedrons that have a too big face, that is also on the front.
                    foreach (Triangle T in new Triangle[] { tet.A, tet.B, tet.C, tet.D })
                    {
                        if (Math.Max(Math.Max(T.pointA.dist(T.pointB), T.pointA.dist(T.pointC)), T.pointB.dist(T.pointC)) >
                                                                                                            maximalAllowedRadius)
                            if (front.Contains(T))
                                triangulationByFacets.Remove(tet);
                    }
                }
                iteration++;
                if (numOfTets == triangulationByFacets.Count() || iteration >= maxIterations)
                {
                    break;
                }
            }
            ////remove tetrahedrons that clearly don't represent neighboring points
            //foreach (TetrahedronByFacets tet in triangulationByFacets.ToList())
            //    foreach (Triangle T in new Triangle[] { tet.A, tet.B, tet.C, tet.D })
            //        if (Math.Max(Math.Max(T.pointA.dist(T.pointB), T.pointA.dist(T.pointC)), T.pointB.dist(T.pointC)) > 
            //                                                                                                    maximalAllowedRadius)
            //            triangulationByFacets.Remove(tet);
//            IEnumerable<Triangle> results = uniqueFacets(ref triangulationByFacets);
            //HashSet<Triangle> results = new HashSet<Triangle>();
            //foreach (TetrahedronByFacets tet in triangulationByFacets)
            //    foreach (Triangle T in new Triangle[] { tet.A, tet.B, tet.C, tet.D })
            //        if (Math.Max(Math.Max(T.pointA.dist(T.pointB), T.pointA.dist(T.pointC)), T.pointB.dist(T.pointC))
            //                                                                                        <= maximalAllowedRadius)
            //            results.Add(T);
            sw5.Stop();
            Console.WriteLine("removing triangles={0}", sw5.Elapsed);
            return uniqueFacets(ref triangulationByFacets);

        }

        static Boolean onTheSamePlane(Triangle T, Point P)
        {

            Tetrahedron test = new Tetrahedron(P, T.pointA, T.pointB, T.pointC);

            if (test.Calculate_a() == 0)

                return true;

            return false;

        }
        static void addToPolygon(ref HashSet<Triangle> polygon, Triangle T)
        {
            //in case the triangle already exists we drop both
            if (polygon.Contains(T))
                polygon.Remove(T);
            else
                polygon.Add(T);
        }
        //static void clearPolygon(ref HashSet<Triangle> polygon)
        //{

        //    //we want only the triangles that appeared once (thus were shared only by one bad triangle)

        //    HashSet<Triangle> badTrianglesLocal = new HashSet<Triangle>();


        //    int sizeOfList = polygon.Count();

        //    for (int i = 0; i < sizeOfList; ++i)
        //    {

        //        for (int j = i + 1; j < sizeOfList; ++j)
        //        {

        //            if (polygon[i].Equals(polygon[j]))
        //            {

        //                badTrianglesLocal.Add(polygon[i]);

        //                badTrianglesLocal.Add(polygon[j]);

        //            }

        //        }

        //    }

        //    foreach (Triangle B in badTrianglesLocal)
        //        polygon.Remove(B);

        //}

        //static Boolean facetIsShared(HashSet<Tetrahedron> badTetrahedrons, Triangle facet, Tetrahedron ownerOfFacet)
        //{

        //    /*here we check whether the given facet is belongs to some tetrahedron in the set*/

        //    foreach (Tetrahedron T in badTetrahedrons)
        //    {

        //        TetrahedronByFacets F = TetrahedronByFacets.convertFromTetrahedron(T);

        //        if (T.Equals(ownerOfFacet))

        //            continue;

        //        if (F.A.Equals(facet) || F.B.Equals(facet) || F.C.Equals(facet) || F.D.Equals(facet))

        //            return true;

        //    }

        //    return false;

        //}

        static Tetrahedron CoverTetrahedron(HashSet<Point> Points)
        {

            double maxDistance = 0;

            Point CenterOfCircumscribedCircle = new Point();

            /* chapter 1 in which we find a containing circle for the set of points */

            foreach (Point P in Points)
            {

                foreach (Point Q in Points)
                {

                    if (P.dist(Q) > maxDistance)
                    {

                        CenterOfCircumscribedCircle = P;

                        maxDistance = P.dist(Q);

                    }

                }

            }

            /* chapter 2 in which we find a containing tetrahedron for the circle */

            double TetraRadius = 6 * maxDistance;

            Tetrahedron HugeTetrahedron = new Tetrahedron();

            //CenterOfCircumscribedCircle.print();

            HugeTetrahedron.pointA = CenterOfCircumscribedCircle.Copy();//A is just bellow the center

            HugeTetrahedron.pointA.zCoordinate -= TetraRadius;




            HugeTetrahedron.pointB = CenterOfCircumscribedCircle.Copy();//B is to the right and a bit higher

            HugeTetrahedron.pointB.xCoordinate += TetraRadius * Math.Cos(Math.PI / 6.0);

            HugeTetrahedron.pointB.zCoordinate += TetraRadius * Math.Sin(Math.PI / 6.0);




            HugeTetrahedron.pointC = CenterOfCircumscribedCircle.Copy();//C is closer and a bit higher and left=

            HugeTetrahedron.pointC.xCoordinate -= TetraRadius * Math.Sin(Math.PI / 6.0);

            HugeTetrahedron.pointC.yCoordinate += TetraRadius * Math.Cos(Math.PI / 6.0);

            HugeTetrahedron.pointC.zCoordinate += TetraRadius * Math.Sin(Math.PI / 6.0);




            HugeTetrahedron.pointD = CenterOfCircumscribedCircle.Copy();//C is farther and a bit higher and left

            HugeTetrahedron.pointD.xCoordinate -= TetraRadius * Math.Sin(Math.PI / 6.0);

            HugeTetrahedron.pointD.yCoordinate -= (TetraRadius * Math.Cos(Math.PI / 6.0));

            HugeTetrahedron.pointD.zCoordinate += TetraRadius * Math.Sin(Math.PI / 6.0);




            return HugeTetrahedron;




        }

    }

}
