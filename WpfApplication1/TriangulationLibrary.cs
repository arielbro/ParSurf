using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace ParSurf
{
    class TriangulationLibrary
    {
        public class Edge
        {
            Point3D Point3DA;
            Point3D Point3DB;
            public Edge(Point3D A, Point3D B)
            {
                Point3DA = A;
                Point3DB = B;
            }
            public Edge() : this(new Point3D(), new Point3D()) { }

            public double len()
            {
                return (Point3DA - Point3DB).Length;
            }
            public Boolean Equals(Edge B)
            {
                /*is it?*/
                return ((Point3DA == B.Point3DA) && (Point3DB == B.Point3DB)) || ((Point3DA == B.Point3DB) && (Point3DB == B.Point3DA));
            }
        }
        public class Triangle
        {
            public Point3D Point3DA;
            public Point3D Point3DB;
            public Point3D Point3DC;

            public Triangle()
                : this(new Point3D(), new Point3D(), new Point3D())
            {
            }
            public Triangle(Point3D A, Point3D B, Point3D C)
            {
                Point3DA = A;
                Point3DB = B;
                Point3DC = C;
            }
            public override Boolean Equals(Object T0)
            {
                if (T0.GetType() != typeof(Triangle))
                    return false;
                Triangle T = T0 as Triangle;
                if ((Point3DA == T.Point3DA) && (((Point3DB == T.Point3DB) && (Point3DC == T.Point3DC)) || ((Point3DB == T.Point3DC) && (Point3DC == T.Point3DB))))
                    return true;
                if ((Point3DA == T.Point3DB) && (((Point3DB == T.Point3DC) && (Point3DC == T.Point3DA)) || ((Point3DB == T.Point3DA) && (Point3DC == T.Point3DC))))
                    return true;
                if ((Point3DA == T.Point3DC) && (((Point3DB == T.Point3DA) && (Point3DC == T.Point3DB)) || ((Point3DB == T.Point3DB) && (Point3DC == T.Point3DA))))
                    return true;
                return false;
            }
            public void print()
            {
                Debug.WriteLine("+++++");
                Debug.WriteLine(Point3DA);
                Debug.WriteLine(Point3DB);
                Debug.WriteLine(Point3DC);
                Debug.WriteLine("+++++");
            }
            public override int GetHashCode()
            {
                //written explicitly so that triangles with same vertices but different orders will be counted equal
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 0;
                    hash += Point3DA.GetHashCode();
                    hash += Point3DB.GetHashCode();
                    hash += Point3DC.GetHashCode();
                    return hash;
                }
            }
        }
        public class Tetrahedron : Triangle
        {
            public Point3D Point3DD;
            public Tetrahedron(Point3D A, Point3D B, Point3D C, Point3D D)
                : base(A, B, C)
            {
                Point3DD = D;
            }
            public Tetrahedron()
            {
                Point3DD = new Point3D();
            }
            public Point3D center()
            {
                /* finding the center of the circumsphere is not for the weak-kneed of us, 
                 * but fot the brave of you, this is the place to go to 
                 * http://mathworld.wolfram.com/Circumsphere.html   */

                Point3D center = new Point3D();

                double aDet = Calculate_a();

                if (aDet == 0)
                {
                    Debug.WriteLine("div by zero");
                    Debug.WriteLine(this);
                }
                center.X = CalculateDx() / (2 * aDet);
                center.Y = CalculateDy() / (2 * aDet);
                center.Z = CalculateDz() / (2 * aDet);

                return center;

            }
            public double radius()
            {
                /*here we measure the length of the circumsphere's radius*/
                double delta = Math.Pow(this.CalculateDx(), 2) + Math.Pow(this.CalculateDy(), 2) + Math.Pow(this.CalculateDz(), 2) - 4 * Calculate_a() * Calculate_c();
                if (delta < 0)
                {
                    Debug.WriteLine("mistake, delta=" + delta + +Calculate_a() + Calculate_c());
                    Debug.WriteLine("Dx: " + this.CalculateDx() + " Dy: " + this.CalculateDy() + " Dz: " + this.CalculateDz());
                    Debug.WriteLine("a: " + this.Calculate_a() + " c: " + this.Calculate_c());
                    Debug.WriteLine(this);
                }
                double a = Calculate_a();
                if (a == 0)
                {
                    Debug.WriteLine("div by zero");
                    Debug.WriteLine(this);
                }
                if (a < 0)
                    a *= -1.0;
                return Math.Sqrt(delta) / (2 * a);
            }
            /* auxiliary functions pretty much to the end */
            public double CalculateDx()
            {

                double[,] matrixX = new double[4, 4];

                fillRowX(ref matrixX, this.Point3DA, 0);
                fillRowX(ref matrixX, this.Point3DB, 1);
                fillRowX(ref matrixX, this.Point3DC, 2);
                fillRowX(ref matrixX, this.Point3DD, 3);

                return determinant(matrixX);

            }
            public double CalculateDy()
            {
                double[,] matrixY = new double[4, 4];

                fillRowY(ref matrixY, this.Point3DA, 0);
                fillRowY(ref matrixY, this.Point3DB, 1);
                fillRowY(ref matrixY, this.Point3DC, 2);
                fillRowY(ref matrixY, this.Point3DD, 3);

                return (-1.0) * determinant(matrixY);
            }
            public double CalculateDz()
            {
                double[,] matrixZ = new double[4, 4];

                fillRowZ(ref matrixZ, this.Point3DA, 0);
                fillRowZ(ref matrixZ, this.Point3DB, 1);
                fillRowZ(ref matrixZ, this.Point3DC, 2);
                fillRowZ(ref matrixZ, this.Point3DD, 3);

                return determinant(matrixZ);
            }
            public double Calculate_a()
            {
                double[,] matrix_a = new double[4, 4];

                fillRow_a(ref matrix_a, this.Point3DA, 0);
                fillRow_a(ref matrix_a, this.Point3DB, 1);
                fillRow_a(ref matrix_a, this.Point3DC, 2);
                fillRow_a(ref matrix_a, this.Point3DD, 3);
                return determinant(matrix_a);
            }
            public double Calculate_c()
            {
                double[,] matrix_c = new double[4, 4];

                fillRow_c(ref matrix_c, this.Point3DA, 0);
                fillRow_c(ref matrix_c, this.Point3DB, 1);
                fillRow_c(ref matrix_c, this.Point3DC, 2);
                fillRow_c(ref matrix_c, this.Point3DD, 3);

                return determinant(matrix_c);
            }
            public static void fillRowX(ref double[,] matrix, Point3D p, int Row)
            {
                matrix[Row, 0] = Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2);
                matrix[Row, 1] = p.Y;
                matrix[Row, 2] = p.Z;
                matrix[Row, 3] = 1;

            }
            public static void fillRowY(ref double[,] matrix, Point3D p, int Row)
            {
                matrix[Row, 0] = Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2);
                matrix[Row, 1] = p.X;
                matrix[Row, 2] = p.Z;
                matrix[Row, 3] = 1;

            }
            public static void fillRowZ(ref double[,] matrix, Point3D p, int Row)
            {
                matrix[Row, 0] = Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2);
                matrix[Row, 1] = p.X;
                matrix[Row, 2] = p.Y;
                matrix[Row, 3] = 1;

            }
            public static void fillRow_a(ref double[,] matrix, Point3D p, int Row)
            {
                matrix[Row, 0] = p.X;
                matrix[Row, 1] = p.Y;
                matrix[Row, 2] = p.Z;
                matrix[Row, 3] = 1;

            }
            public static void fillRow_c(ref double[,] matrix, Point3D p, int Row)
            {
                matrix[Row, 0] = Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2);
                matrix[Row, 1] = p.X;
                matrix[Row, 2] = p.Y;
                matrix[Row, 3] = p.Z;

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
                Debug.WriteLine("****");
                Debug.WriteLine(Point3DA);
                Debug.WriteLine(Point3DB);
                Debug.WriteLine(Point3DC);
                Debug.WriteLine(Point3DD);
                Debug.WriteLine("****");
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
                Triangle A = new Triangle(T.Point3DA, T.Point3DB, T.Point3DC);
                Triangle B = new Triangle(T.Point3DA, T.Point3DB, T.Point3DD);
                Triangle C = new Triangle(T.Point3DA, T.Point3DC, T.Point3DD);
                Triangle D = new Triangle(T.Point3DB, T.Point3DC, T.Point3DD);
                return new TetrahedronByFacets(A, B, C, D);
            }

        }
        /*
                static void Main(string[] args)
                {
                    HashSet<Tuple<double, double, double>> testSet = new HashSet<Tuple<double, double, double>> { new Tuple<double, double, double>(0, 0, 0), new Tuple<double, double, double>(0, 0, 1), new Tuple<double, double, double>(0, 1, 0), new Tuple<double, double, double>(0, 1, 1), new Tuple<double, double, double>(1, 0, 0), new Tuple<double, double, double>(1, 0, 1), new Tuple<double, double, double>(1, 1, 0), new Tuple<double, double, double>(1, 1, 1) };//, new Tuple<double, double, double>(0.5, 0.5, 0.5) };
                    Triangle T1 = new Triangle(new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 0, 1));
                    System.Windows.Media.Media3D.Point3D P1 = new Point3D(0, 0.4, 0.5);
                    if (onTheSamePlane(T1, P1))
                        Debug.WriteLine("on the same plane");
                    if (Point3DInTriangle(T1, P1))
                        Debug.WriteLine("in triangle");
                    Debug.WriteLine("area of T:" + areaOfTriangle(T1));
                    foreach (Point3D P in tuplesToPoints(testSet))
                        Debug.WriteLine(P);
                    HashSet<Tetrahedron> triangulation = triangulate(tuplesToPoints(testSet));
                    Debug.WriteLine("**results**");
                    foreach (Tetrahedron T in triangulation)
                        Debug.WriteLine(T);
                    Debug.WriteLine("number of triangles:" + triangulation.Count());
                }
         */
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
            if (hasTheVertex(A, B.Point3DA) || hasTheVertex(A, B.Point3DB) || hasTheVertex(A, B.Point3DC) || hasTheVertex(A, B.Point3DD))
                return true;
            return false;
        }
        static Boolean hasTheVertex(Tetrahedron T, Point3D P)
        {
            if (P.Equals(T.Point3DA) || P.Equals(T.Point3DB) || P.Equals(T.Point3DC) || P.Equals(T.Point3DD))
                return true;
            return false;
        }
        static Boolean Point3DInTriangle(Triangle T, Point3D P)
        {
            //given they are on the same plane
            double eps = 0.0001;
            Triangle A = new Triangle(T.Point3DA, T.Point3DB, P);
            Triangle B = new Triangle(T.Point3DA, T.Point3DC, P);
            Triangle C = new Triangle(T.Point3DC, T.Point3DB, P);
            return ((areaOfTriangle(A) + areaOfTriangle(B) + areaOfTriangle(C) - areaOfTriangle(T)) < eps);
        }
        static double areaOfTriangle(Triangle T)
        {
            Point3D AB = new Point3D(T.Point3DB.X - T.Point3DA.X, T.Point3DB.Y - T.Point3DA.Y, T.Point3DB.Z - T.Point3DA.Z);
            Point3D AC = new Point3D(T.Point3DC.X - T.Point3DA.X, T.Point3DC.Y - T.Point3DA.Y, T.Point3DC.Z - T.Point3DA.Z);
            double res = Math.Pow(AB.Y * AC.Z - AB.Z * AC.Y, 2) + Math.Pow(AB.Z * AC.X - AB.X * AC.Z, 2) + Math.Pow(AB.X * AC.Y - AB.Y * AC.X, 2);
            return 0.5 * Math.Pow(res, 0.5);
        }

        static public HashSet<Tetrahedron> triangulate(IEnumerable<double[]> points)
        {
            HashSet<Point3D> points3D = new HashSet<Point3D>();
            foreach (double[] point in points)
                points3D.Add(new Point3D(point[0], point[1], point[2]));
            return triangulate(points3D);
        }
        static public HashSet<Tetrahedron> triangulate(HashSet<Point3D> Point3Ds)
        {
            /*prologue
             * the idea is to contain the set of Point3D in a tetrahedron and add Point3Ds to the triangulation one-by-one,
             * each time eliminating tetrahedrons in the triangulation that aren't coherent with the new Point3D and creating
             * new ones instead.
             * for more: http://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm
             */
            HashSet<Tetrahedron> triangulation = new HashSet<Tetrahedron>();
            Tetrahedron Cover = CoverTetrahedron(Point3Ds);
            //Debug.WriteLine(Cover);
            triangulation.Add(Cover);
            foreach (Point3D P in Point3Ds)
            {
                HashSet<Tetrahedron> badTetrahedrons = new HashSet<Tetrahedron>();

                //Debug.WriteLine("**new interation**");
                //Debug.WriteLine(P);
                /* chapter 3 in which we find for each tetrahedron in the triangulation if the new Point3D is in its circumsphere*/
                foreach (Tetrahedron T in triangulation)
                {
                    //Debug.WriteLine(T);
                    //Debug.WriteLine("Dist: " + (T.center() - P).Length + " R: " + T.radius());
                    //Debug.WriteLine(T);
                    if ((T.center() - P).Length < T.radius())
                        badTetrahedrons.Add(T);
                }
                //Debug.WriteLine("found badies");
                /* chapter 4 in which we the border of the area in the triangulation damaged by the new Point3D*/
                List<Triangle> polygon = new List<Triangle>();
                HashSet<Tetrahedron> actuallyGoodTetrahedrons = new HashSet<Tetrahedron>();
                foreach (Tetrahedron T in badTetrahedrons)
                {

                    TetrahedronByFacets F = TetrahedronByFacets.convertFromTetrahedron(T);
                    if (onTheSamePlane(F.A, P) || onTheSamePlane(F.B, P) || onTheSamePlane(F.C, P) || onTheSamePlane(F.D, P))
                    {
                        Debug.WriteLine("?");
                        Debug.WriteLine(T);
                        Debug.WriteLine(P);
                        // **** from here ... 

                        //creating a tetrahedron from 4 Point3D on the same plane is considered bad manners
                        if (!(Point3DInTriangle(F.A, P) || Point3DInTriangle(F.B, P) || Point3DInTriangle(F.C, P) || Point3DInTriangle(F.D, P)))
                        {
                            //if a Point3D is on the same plane as one of the triangles but outside the tetrahedron than we want to leave that tetrahedron in the triangulation
                            //later we will exclude the good tetrahedrons from the bad ones
                            actuallyGoodTetrahedrons.Add(T);
                            Debug.WriteLine("!");
                            continue;
                        }
                        //if a Point3D is in one or two of the triangles than we exlude those triangles
                        if (!Point3DInTriangle(F.A, P))
                            polygon.Add(F.A);
                        if (!Point3DInTriangle(F.B, P))
                            polygon.Add(F.B);
                        if (!Point3DInTriangle(F.C, P))
                            polygon.Add(F.C);
                        if (!Point3DInTriangle(F.D, P))
                            polygon.Add(F.D);
                        continue;
                        // **** ...to here - it is all my idea, so no promises there 
                    }
                    //Debug.WriteLine(F.A);
                    //Debug.WriteLine(F.B);
                    //Debug.WriteLine(F.C);
                    //Debug.WriteLine(F.D);
                    //if (!facetIsShared(badTetrahedrons, F.A,T))
                    polygon.Add(F.A);
                    //if (!facetIsShared(badTetrahedrons, F.B,T))
                    polygon.Add(F.B);
                    //if (!facetIsShared(badTetrahedrons, F.C,T))
                    polygon.Add(F.C);
                    //if (!facetIsShared(badTetrahedrons, F.D,T))
                    polygon.Add(F.D);
                    //Debug.WriteLine(polygon.Count());
                }
                clearPolygon(ref polygon);
                /* chapter 5 in which we fix the damaged area and add new tetrahedrons*/
                foreach (Tetrahedron T in actuallyGoodTetrahedrons)
                    badTetrahedrons.Remove(T);
                foreach (Tetrahedron T in badTetrahedrons)
                    triangulation.Remove(T);
                foreach (Triangle T in polygon)
                {
                    Tetrahedron newTetrahedron = new Tetrahedron(T.Point3DA, T.Point3DB, T.Point3DC, P);
                    triangulation.Add(newTetrahedron);
                    //Debug.WriteLine(newTetrahedron);
                }
                Debug.WriteLine("number of triangles:" + triangulation.Count());
            }
            eliminateCover(ref triangulation, Cover);
            return triangulation;
        }
        static Boolean onTheSamePlane(Triangle T, Point3D P)
        {
            Tetrahedron test = new Tetrahedron(P, T.Point3DA, T.Point3DB, T.Point3DC);
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
                    if (T1.Equals(T2))
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
                    if (polygon[i].Equals(polygon[j]))
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
        static Boolean facetIsShared(HashSet<Tetrahedron> badTetrahedrons, Triangle facet, Tetrahedron ownerOfFacet)
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
        static Tetrahedron CoverTetrahedron(HashSet<Point3D> Point3Ds)
        {
            double maxDistance = 0;
            Point3D CenterOfCircumscribedCircle = new Point3D();
            /* chapter 1 in which we find a containing circle for the set of Point3Ds */
            foreach (Point3D P in Point3Ds)
            {
                foreach (Point3D Q in Point3Ds)
                {
                    if ((P - Q).Length > maxDistance)
                    {
                        CenterOfCircumscribedCircle = P;
                        maxDistance = (P - Q).Length;
                    }
                }
            }

            /* chapter 2 in which we find a containing tetrahedron for the circle */
            double TetraRadius = 6 * maxDistance;
            Tetrahedron HugeTetrahedron = new Tetrahedron();
            Debug.WriteLine(CenterOfCircumscribedCircle);
            //Point3D is a struct, so it is copied on assignment.
            HugeTetrahedron.Point3DA = CenterOfCircumscribedCircle;//A is just bellow the center
            HugeTetrahedron.Point3DA.Z -= TetraRadius;
            HugeTetrahedron.Point3DB = CenterOfCircumscribedCircle;//B is to the right and a bit higher
            HugeTetrahedron.Point3DB.X += TetraRadius * Math.Cos(Math.PI / 6.0);
            HugeTetrahedron.Point3DB.Z += TetraRadius * Math.Sin(Math.PI / 6.0);
            HugeTetrahedron.Point3DC = CenterOfCircumscribedCircle;//C is closer and a bit higher and left
            HugeTetrahedron.Point3DC.X += TetraRadius * Math.Sin(Math.PI / 6.0);
            HugeTetrahedron.Point3DC.Z += TetraRadius * Math.Sin(Math.PI / 6.0);
            HugeTetrahedron.Point3DC.Y += TetraRadius * Math.Cos(Math.PI / 6.0);
            HugeTetrahedron.Point3DD = CenterOfCircumscribedCircle;//C is farther and a bit higher and left
            HugeTetrahedron.Point3DD.X += TetraRadius * Math.Sin(Math.PI / 6.0);
            HugeTetrahedron.Point3DD.Z += TetraRadius * Math.Sin(Math.PI / 6.0);
            HugeTetrahedron.Point3DD.Y -= (TetraRadius * Math.Cos(Math.PI / 6.0));

            return HugeTetrahedron;

        }
        public static HashSet<Point3D> tuplesToPoints(HashSet<Tuple<double, double, double>> tupleSet)
        {
            /*tuples to points, sets to sets*/
            HashSet<Point3D> PointSet = new HashSet<Point3D>();
            foreach (Tuple<double, double, double> T in tupleSet)
                PointSet.Add(new Point3D(T.Item1, T.Item2, T.Item3));
            return PointSet;
        }
        static public List<double[][]> convertDelaunayToTriangles(HashSet<Tetrahedron> delaunayTriangulation)
        {
            /* Given a set of tetrahedrons constituting a Delaunay 'triangulation', and under the assumption that they 
             * represent a surface, create a triangle mesh of that surface, by taking only the triangle faces of the tetrahedrons
             * that are unique to one tetrahedron. */
            Dictionary<Triangle, int> triangleAppearances = new Dictionary<Triangle, int>();
            foreach (Tetrahedron tetra in delaunayTriangulation)
            {
                //extract triangle faces from the tetrahedron (tetra stores them in fields, not array, so no better way)
                Triangle[] triangleFaces = new Triangle[]{new Triangle(tetra.Point3DA,tetra.Point3DB,tetra.Point3DC),
                                                      new Triangle(tetra.Point3DA,tetra.Point3DB,tetra.Point3DD),
                                                      new Triangle(tetra.Point3DA,tetra.Point3DC,tetra.Point3DD),
                                                      new Triangle(tetra.Point3DB,tetra.Point3DC,tetra.Point3DD)};
                foreach (Triangle triangleFace in triangleFaces)
                {
                    if (triangleAppearances.ContainsKey(triangleFace))
                        triangleAppearances[triangleFace] += 1;
                    else
                        triangleAppearances[triangleFace] = 1;
                }
            }
            //assumption - each triangle face that is not exterior is shared by two tetrahedrons exactly. Assert!
            //use double[][] over Triangle for client code, List over HashSet as well because canvas managers needs to index elemenets.
            List<double[][]> triangles = new List<double[][]>();
            foreach (Triangle triangle in triangleAppearances.Keys)
            {
                //Debug.Assert(triangleAppearances[triangle] <= 2);
                int appear = triangleAppearances[triangle];
                if (triangleAppearances[triangle] == 1)
                {
                    Point3D[] originalTriangle = new Point3D[] { triangle.Point3DA, triangle.Point3DB, triangle.Point3DC };
                    double[][] convertedTriangle = new double[3][];
                    for (int i = 0; i < 3; i++)
                    {
                        convertedTriangle[i] = new double[] { originalTriangle[i].X, originalTriangle[i].Y, originalTriangle[i].Z };
                    }
                    triangles.Add(convertedTriangle);
                }
            }
            return triangles;
        }
    }
}
