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

            xCoordinate=0;

            yCoordinate=0;

            zCoordinate=0;

        }

        public Point(double x,double y,double z)

        {

            xCoordinate=x;

            yCoordinate=y;

            zCoordinate=z;

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

        public Point Copy(Point B)

        {

            return new Point(B.xCoordinate,B.yCoordinate,B.zCoordinate);

        }

        public Boolean isEqual(Point B)
        {
            if ((this.xCoordinate == B.xCoordinate) && (this.yCoordinate == B.yCoordinate) && (this.zCoordinate == B.zCoordinate))
                return true;
            return false;
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
            return new double[]{xCoordinate, yCoordinate, zCoordinate};
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

       public Edge():this(new Point(),new Point()){}




       public double len()

       {

           return pointA.dist(pointB);

       }

       public Boolean isEqual(Edge B)

       {

           /*is it?*/

           return ((pointA == B.pointA) && (pointB == B.pointB)) || ((pointA == B.pointB) && (pointB == B.pointA));

       }

   }




    public class Triangle

    {

        public Point pointA;

        public Point pointB;

        public Point pointC;




        public Triangle() : this(new Point(),new Point(),new Point())

        {

        }

        public Triangle(Point A,Point B,Point C)

        {

            pointA = A;

            pointB = B;

            pointC = C;

        }
        public Boolean hasPoint(Point P)
        {
            if (this.pointA.isEqual(P) || this.pointB.isEqual(P) || this.pointC.isEqual(P))
                return true;
            return false;
        }
        public Boolean isEqual(Triangle T)

        {

            if ((pointA == T.pointA) && (((pointB == T.pointB) && (pointC == T.pointC)) || ((pointB == T.pointC) && (pointC == T.pointB))))

                return true;

            if ((pointA == T.pointB) && (((pointB == T.pointC) && (pointC == T.pointA)) || ((pointB == T.pointA) && (pointC == T.pointC))))

                return true;

            if ((pointA == T.pointC) && (((pointB == T.pointA) && (pointC == T.pointB)) || ((pointB == T.pointB) && (pointC == T.pointA))))

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

        public double[][] convertToDoubleArrays()
        {
            return new double[][]{pointA.convertToDoubleArray(),pointB.convertToDoubleArray(),
                                 pointC.convertToDoubleArray()};
        }

        public static List<double[][]> convertTrianglesToListOfDoubleArrays(HashSet<Triangle> triangles)
        {
            List<double[][]> res = new List<double[][]>();
            foreach (Triangle triangle in triangles)
            {
                res.Add(triangle.convertToDoubleArrays());
            }
            return res;  
        }


    }

    public class Tetrahedron : Triangle

    {

        public Point pointD;

        public Tetrahedron(Point A,Point B,Point C,Point D):base(A,B,C)

        {

            pointD = D;

        }

        public Tetrahedron()

        {

            pointD = new Point();

        }

        public static Tetrahedron TetrahedronFromTetrahedronbByFacets(TetrahedronByFacets T)

        {

            Tetrahedron newTetrahedron = new Tetrahedron();

            newTetrahedron.pointA = T.A.pointA;

            newTetrahedron.pointB = T.A.pointB;

            newTetrahedron.pointC = T.A.pointC;

            if (!(T.B.pointA.Equals(T.A.pointA) ||  T.B.pointA.Equals(T.A.pointB) || T.B.pointA.Equals(T.A.pointC) ))

                newTetrahedron.pointD = T.B.pointA;

            else if (!(T.B.pointB.Equals(T.A.pointA) ||  T.B.pointB.Equals(T.A.pointB) || T.B.pointB.Equals(T.A.pointC) ))

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

           

            Point center = new Point();




            double aDet=Calculate_a();




            if (aDet == 0)

            {

                Console.WriteLine("div by zero");

                this.print();

            }

            center.xCoordinate = CalculateDx() / (2 * aDet);

            center.yCoordinate = CalculateDy() / (2 * aDet);

            center.zCoordinate = CalculateDz() / (2 * aDet);




            return center;

 

        }

        public double Volume()

        {

            return (Math.Abs(this.Calculate_a())/6.0);

        }




        public double radius()

        {

            /*here we measure the length of the circumsphere's radius*/

            double delta = Math.Pow(this.CalculateDx(), 2) + Math.Pow(this.CalculateDy(), 2) + Math.Pow(this.CalculateDz(), 2) - 4 * Calculate_a() * Calculate_c();

            if (delta < 0)

            {

                Console.WriteLine("mistake, delta=" + delta +  + Calculate_a() + Calculate_c() );

                Console.WriteLine("Dx: " + this.CalculateDx() + " Dy: " + this.CalculateDy() + " Dz: " + this.CalculateDz());

                Console.WriteLine("a: " + this.Calculate_a() + " c: " + this.Calculate_c() );

                this.print();

            }

            double a = Calculate_a();

            if (a == 0)

            {

                Console.WriteLine("div by zero");

                this.print();

            }

            if (a < 0)

                a *= -1.0;

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




            return (-1.0)*determinant(matrixY);

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










        public static void fillRowX(ref double[,] matrix,Point p,int Row)

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

            if (this.A.isEqual(T) || this.B.isEqual(T) || this.C.isEqual(T) || this.D.isEqual(T))

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

            double eps = 0.00001;

            Triangle A = new Triangle(T.pointA, T.pointB, P);

            Triangle B = new Triangle(T.pointA, T.pointC, P);

            Triangle C = new Triangle(T.pointC, T.pointB, P);

            return ((areaOfTriangle(A) + areaOfTriangle(B) + areaOfTriangle(C) - areaOfTriangle(T)) < eps * areaOfTriangle(T));

        }

        static Boolean PointInTetrahedron(Tetrahedron T, Point P)

        {

            double epsilon = 0.000001;

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
                foreach (Point Q in uniquePoints)
                {
                    if (P.isEqual(Q))
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    uniquePoints.Add(P);
                add = true;
            }
            return uniquePoints;
        }
        public static HashSet<Triangle>  triangulateAlternative(HashSet<Point> Points)

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
        static HashSet<Triangle> deleteCover2( HashSet<Triangle> triangulation, TetrahedronByFacets Cover)
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
        static HashSet<Triangle> uniqueFacets(ref HashSet<TetrahedronByFacets> triangulation)

        {

            Dictionary<Triangle, int> Triangles = new Dictionary<Triangle, int>();

            HashSet<Triangle> result = new HashSet<Triangle>();

            foreach (TetrahedronByFacets T in triangulation)

                addTetraToDict(ref Triangles, T);

            foreach (KeyValuePair<Triangle, int> Element in Triangles)

            {

                if (Element.Value == 1)

                    result.Add(Element.Key);

            }

            return result;

        }

        static void addTetraToDict(ref Dictionary<Triangle, int> Triangles, TetrahedronByFacets T)

        {

            addTriangleToDict(ref Triangles,  T.A);

            addTriangleToDict(ref Triangles,  T.B);

            addTriangleToDict(ref Triangles, T.C);

            addTriangleToDict(ref Triangles, T.D);

        }




        static void addTriangleToDict(ref Dictionary<Triangle, int> Triangles, Triangle T)

        {

            Triangle foundKey = null;

            foreach (KeyValuePair<Triangle, int> Element in Triangles)

            {

                if (Element.Key.isEqual(T))

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

            double epsilon = 0.00001;

            return (P.dist(A) + P.dist(B) - A.dist(B) < epsilon * A.dist(B));

        }

        static double areaOfTriangle(Triangle T)

        {

            Point AB=new Point(T.pointB.xCoordinate-T.pointA.xCoordinate,T.pointB.yCoordinate-T.pointA.yCoordinate,T.pointB.zCoordinate-T.pointA.zCoordinate);

            Point AC=new Point(T.pointC.xCoordinate-T.pointA.xCoordinate,T.pointC.yCoordinate-T.pointA.yCoordinate,T.pointC.zCoordinate-T.pointA.zCoordinate);

            double res = Math.Pow(AB.yCoordinate * AC.zCoordinate - AB.zCoordinate * AC.yCoordinate, 2) + Math.Pow(AB.zCoordinate * AC.xCoordinate - AB.xCoordinate * AC.zCoordinate, 2) + Math.Pow(AB.xCoordinate * AC.yCoordinate - AB.yCoordinate * AC.xCoordinate, 2);

            return 0.5*Math.Pow(res,0.5);

        }




        static HashSet<Tetrahedron> triangulate(HashSet<Point> Points)

        {

            /*prologue

             * the idea is to contain the set of point in a tetrahedron and add points to the triangulation one-by-one,

             * each time eliminating tetrahedrons in the triangulation that aren't coherent with the new point and creating

             * new ones instead.

             * for more: http://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm

             */

            HashSet<Tetrahedron> triangulation = new HashSet<Tetrahedron>();

            Tetrahedron Cover = CoverTetrahedron(Points);

            //Cover.print();

            triangulation.Add(Cover);

            foreach (Point P in Points)

            {

                HashSet<Tetrahedron> badTetrahedrons= new HashSet<Tetrahedron>();




                //Console.WriteLine("**new interation**");

                //P.print();

                /* chapter 3 in which we find for each tetrahedron in the triangulation if the new point is in its circumsphere*/

                foreach(Tetrahedron T in triangulation)

                {

                    //T.print();

                    //Console.WriteLine("Dist: " + T.center().dist(P) + " R: " + T.radius());

                    //T.print();

                    if (T.center().dist(P) < T.radius())

                        badTetrahedrons.Add(T);

                }

                //Console.WriteLine("found badies");

                /* chapter 4 in which we the border of the area in the triangulation damaged by the new point*/

                List<Triangle> polygon = new List<Triangle>();

                HashSet<Tetrahedron> actuallyGoodTetrahedrons= new HashSet<Tetrahedron>();

                foreach(Tetrahedron T in badTetrahedrons)

                {




                    TetrahedronByFacets F = TetrahedronByFacets.convertFromTetrahedron(T);

                    if (onTheSamePlane(F.A, P) || onTheSamePlane(F.B, P) || onTheSamePlane(F.C, P) || onTheSamePlane(F.D, P))

                    {

                        Console.WriteLine("?");

                        T.print();

                        P.print();

                        // **** from here ... 




                        //creating a tetrahedron from 4 point on the same plane is considered bad manners

                        if (!(pointInTriangle(F.A, P)||pointInTriangle(F.B, P)||pointInTriangle(F.C, P)||pointInTriangle(F.D, P)))

                        {

                            //if a point is on the same plane as one of the triangles but outside the tetrahedron than we want to leave that tetrahedron in the triangulation

                            //later we will exclude the good tetrahedrons from the bad ones

                            actuallyGoodTetrahedrons.Add(T);

                            Console.WriteLine("!");

                            continue;

                        }

                        //if a point is in one or two of the triangles than we exlude those triangles

                        if (!pointInTriangle(F.A, P))

                            polygon.Add(F.A);

                        if (!pointInTriangle(F.B, P))

                            polygon.Add(F.B);

                        if (!pointInTriangle(F.C, P))

                            polygon.Add(F.C);

                        if (!pointInTriangle(F.D, P))

                            polygon.Add(F.D);

                        continue;

                        // **** ...to here - it is all my idea, so no promises there 

                    }

                    //F.A.print();

                    //F.B.print();

                    //F.C.print();

                    //F.D.print();

                    //if (!facetIsShared(badTetrahedrons, F.A,T))

                        polygon.Add(F.A);

                    //if (!facetIsShared(badTetrahedrons, F.B,T))

                        polygon.Add(F.B);

                    //if (!facetIsShared(badTetrahedrons, F.C,T))

                        polygon.Add(F.C);

                    //if (!facetIsShared(badTetrahedrons, F.D,T))

                        polygon.Add(F.D);

                    //Console.WriteLine(polygon.Count());

                }

                clearPolygon(ref polygon);

                /* chapter 5 in which we fix the damaged area and add new tetrahedrons*/

                foreach (Tetrahedron T in actuallyGoodTetrahedrons)

                    badTetrahedrons.Remove(T);

                foreach (Tetrahedron T in badTetrahedrons)

                    triangulation.Remove(T);

                foreach (Triangle T in polygon)

                {

                    Tetrahedron newTetrahedron = new Tetrahedron(T.pointA,T.pointB,T.pointC,P);

                    triangulation.Add(newTetrahedron);

                    //newTetrahedron.print();

                }

                Console.WriteLine("number of triangles:" + triangulation.Count());

                Console.ReadKey();

            }

            eliminateCover(ref triangulation,Cover);

            return triangulation;

        }

        static Boolean onTheSamePlane( Triangle T,Point P)

        {

            Tetrahedron test = new Tetrahedron(P, T.pointA, T.pointB, T.pointC);

            if (test.Calculate_a() == 0)

                return true;

            return false;

        }

        static void clearPolygon(ref List<Triangle> polygon)

        {

            //we want only the triangles that appeared once (thus were shared only by one bad triangle)

            HashSet<Triangle> badTrianglesLocal = new HashSet<Triangle>();/*

            foreach(Triangle T1 in polygon)

            {

                foreach (Triangle T2 in polygon)

                {

                    if (T1 == T2)

                    {

                        continue;

                    }

                    if (T1.isEqual(T2))

                    {

                        badTriangles.Add(T1);

                        badTriangles.Add(T2);

                    }

                }

            }*/

            int sizeOfList = polygon.Count();

            for (int i = 0; i < sizeOfList; ++i)

            {

                for (int j = i + 1; j < sizeOfList; ++j)

                {

                    if (polygon[i].isEqual(polygon[j]))

                    {

                        badTrianglesLocal.Add(polygon[i]);

                        badTrianglesLocal.Add(polygon[j]);

                    }

                }

            }

            foreach (Triangle B in badTrianglesLocal)

            {

                while (polygon.Remove(B)) { };

            }

        }

        static Boolean facetIsShared(HashSet<Tetrahedron> badTetrahedrons, Triangle facet,Tetrahedron ownerOfFacet)

        {

            /*here we check whether the given facet is belongs to some tetrahedron in the set*/

            foreach (Tetrahedron T in badTetrahedrons)

            {

                TetrahedronByFacets F = TetrahedronByFacets.convertFromTetrahedron(T);

                if (T == ownerOfFacet)

                    continue;

                if (F.A.Equals(facet) || F.B.Equals(facet) || F.C.Equals(facet) || F.D.Equals(facet))

                    return true;

            }

            return false;

        }

        static Tetrahedron CoverTetrahedron(HashSet<Point> Points)

        {

            double maxDistance=0;

            Point CenterOfCircumscribedCircle=new Point();

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

            HugeTetrahedron.pointA.Copy(CenterOfCircumscribedCircle);//A is just bellow the center

            HugeTetrahedron.pointA.zCoordinate -= TetraRadius;




            HugeTetrahedron.pointB.Copy(CenterOfCircumscribedCircle);//B is to the right and a bit higher

            HugeTetrahedron.pointB.xCoordinate += TetraRadius * Math.Cos(Math.PI / 6.0);

            HugeTetrahedron.pointB.zCoordinate += TetraRadius * Math.Sin(Math.PI / 6.0);




            HugeTetrahedron.pointC.Copy(CenterOfCircumscribedCircle);//C is closer and a bit higher and left

            HugeTetrahedron.pointC.xCoordinate -= TetraRadius * Math.Sin(Math.PI / 6.0);

            HugeTetrahedron.pointC.yCoordinate += TetraRadius * Math.Cos(Math.PI / 6.0);

            HugeTetrahedron.pointC.zCoordinate += TetraRadius * Math.Sin(Math.PI / 6.0);




            HugeTetrahedron.pointD.Copy(CenterOfCircumscribedCircle);//C is farther and a bit higher and left

            HugeTetrahedron.pointD.xCoordinate -= TetraRadius * Math.Sin(Math.PI / 6.0);

            HugeTetrahedron.pointD.yCoordinate -= (TetraRadius * Math.Cos(Math.PI / 6.0));

            HugeTetrahedron.pointD.zCoordinate += TetraRadius * Math.Sin(Math.PI / 6.0);




            return HugeTetrahedron;




        }

    }

}
