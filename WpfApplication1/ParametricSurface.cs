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
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;


namespace ParSurf
{
    [Serializable()]
    public class ParametricSurface
    {
        public int dimension;
        public string Name;
        private double[] urange;
        private double[] vrange;
        private Boolean uclosed;
        private Boolean vclosed;
        public delegate double[] CoordinatesFunction(double u, double v, Dictionary<string, double> parameters);
        public CoordinatesFunction coordinates;
        public Dictionary<string, double> parameters;

        public ParametricSurface(string Name, int dimension, CoordinatesFunction coordinates, double[] urange, double[] vrange, Dictionary<String,double> parameters = null, Boolean uclosed = false, Boolean vclosed = false)
        {
            this.dimension = dimension;
            this.coordinates = coordinates;
            this.parameters = parameters;
            this.Name = Name;
            this.urange = urange;
            this.vrange = vrange;
            this.uclosed = uclosed;
            this.vclosed = vclosed;
        }
        public ParametricSurface(Boolean uclosed = false, Boolean vclosed = false)
        {
            this.urange = new double[] { 0, 1 };
            this.vrange = new double[] { 0, 1 };
            this.uclosed = uclosed;
            this.vclosed = vclosed;
        }
        
        public IList<double[][]> triangulate(int usteps = 100, int vsteps = 100)
        {
            //// create a mesh points in a snake scale like pattern - 
            //// for each "row" v_i, iterate over u values, for each adjecent pair, create a triangle with a 
            //// lower left corner (v_(i-1) and u1) and upper right corner (v_(i+1) and u2), up to modulus (if closed)
            if (this.coordinates == null) throw new Exception("a parametric surface needs a coordinate function before triangulating");

            List<double [][]> triangles = new List<double[][]>();
            double epsilon = 0.0001; //this fixes the problem of mobius and klein having gaps in the seam area.
            for (double ustep = 0; ustep < usteps; ustep++)
            {
                for (double vstep = 0; vstep < vsteps; vstep++)
                {
                    double u1 = urange[0] + ustep / usteps * (urange[1] - urange[0]);
                    double u2 = urange[0] + ((ustep + 1) / usteps * (urange[1] - urange[0])) % (urange[1] - urange[0] + epsilon);
                    if (u2 < u1 && !uclosed) break;
                    double v = vrange[0] + vstep / vsteps * (vrange[1] - vrange[0]);
                    double vup = vrange[0] + ((vstep + 1) / vsteps * (vrange[1] - vrange[0])) % (vrange[1] - vrange[0] + epsilon);
                    double vdown = vrange[0] + ((vstep - 1) / vsteps * (vrange[1] - vrange[0])) % (vrange[1] - vrange[0] + epsilon);

                    double[] point1 = coordinates(u1, v, parameters);
                    double[] point2 = coordinates(u2, v, parameters);
                    double[] pointUpperRight = coordinates(u2, vup, parameters);
                    double[] pointLowerLeft = coordinates(u1, vdown, parameters);
                    if (vclosed || vup > v)
                        triangles.Add(new double[][] { point1, point2, pointUpperRight });
                    if (vclosed || vdown < v)
                        triangles.Add(new double[][] { point2, point1, pointLowerLeft });
                }
            }
            return triangles;
        }

        public static double[] kleinBottleFigureEigthPoint(double theta, double v)
        {
            //adjusted to run from [0,1]^2
            theta = 2 * Math.PI * theta;
            v = 2 * Math.PI * v;
            const double r = 3;
            double x = (r + Math.Cos(theta / 2) * Math.Sin(v) - Math.Sin(theta / 2) * Math.Sin(2 * v)) * Math.Cos(theta);
            double y = (r + Math.Cos(theta / 2) * Math.Sin(v) - Math.Sin(theta / 2) * Math.Sin(2 * v)) * Math.Sin(theta);
            double z = Math.Sin(theta / 2) * Math.Sin(v) + Math.Cos(theta / 2) * Math.Sin(2 * v);
            return new double[]{ x, y, z };
        }
        public static double[] torusPoint(double theta, double phi)
        {
            theta = 2 * Math.PI * theta;
            phi = 2 * Math.PI * phi;
            const double r = 1;
            const double R = 3;
            double x = (R + r * Math.Cos(phi)) * Math.Cos(theta);
            double y = (R + r * Math.Cos(phi)) * Math.Sin(theta);
            double z = r * Math.Sin(phi);
            return new double[] { x, y, z };
        }
        public static double[] spherePoint(double theta, double phi, Dictionary<string, double> parameters)
        {
            theta = Math.PI * theta;
            phi = 2 * Math.PI * phi;
            double r = parameters["radius"];
            double x = r * Math.Cos(phi)*Math.Sin(theta);
            double y = r * Math.Sin(phi)*Math.Sin(theta);
            double z = r * Math.Cos(theta);
            return new double[]{ x, y, z };
        }
        public static double[] cylinderPoint(double theta, double z)
        {
            theta = 2 * Math.PI * theta;
            const double r = 3;
            z = 10 * (z - 0.5);
            double x = r * Math.Cos(theta);
            double y = r * Math.Sin(theta);
            return new double[]{ x, y, z };
        }
        public static double[] conePoint(double theta, double r)
        {
            theta = 2 * Math.PI * theta;
            const double length = 3;

            double x = length * r * Math.Sin(theta);
            double y = length * r;
            double z = length * r * Math.Cos(theta);
            return new double[]{ x, y, z };
        }
        public static double[] mobiusPoint(double theta, double v)
        {
            //v runs from -1 to 1 
            v = 2 * v - 1;
            theta = 2 * Math.PI * theta;
            //http://mathworld.wolfram.com/MoebiusStrip.html
            double w = 1;
            double R = 3;
            double x = (R + w * v * Math.Cos(0.5 * theta)) * Math.Cos(theta);
            double y = (R + w * v * Math.Cos(0.5 * theta)) * Math.Sin(theta);
            double z = w * v * Math.Sin(0.5 * theta);
            return new double[]{ x, y, z };
        }
        public static double[] sinWaves(double theta, double phi)
        {
            double numberOfPeriods = 1;
            theta = (theta - 0.5) * 2 * Math.PI * numberOfPeriods;
            phi = (phi - 0.5) * 2 * Math.PI * numberOfPeriods;
            //http://mathworld.wolfram.com/MoebiusStrip.html
            double x = theta;
            double y = phi;
            double z = Math.Sin(theta) + Math.Sin(phi);
            return new double[]{ x, y, z };
        }
        public static CoordinatesFunction xTimesSineReciporal(double xExponent = 1, double numberOfPeriods = 4)
        {
            CoordinatesFunction cordFunc = (theta, phi, parameters) =>
            {
                theta = (theta - 0.5) * 2 * Math.PI * numberOfPeriods;
                phi = (phi - 0.5) * 2 * Math.PI * numberOfPeriods;
                if (phi == 0 || theta == 0)
                {
                    if (theta != 0)
                        return new double[]{theta, phi, Math.Pow(theta, xExponent) * Math.Sin(1 / theta)};
                    if (phi != 0)
                        return new double[]{theta, phi, Math.Pow(phi, xExponent) * Math.Sin(1 / phi)};
                    return new double[]{0, 0, 0};
                } 
                //http://mathworld.wolfram.com/MoebiusStrip.html
                double x = theta;
                double y = phi;
                double z = Math.Pow(theta, xExponent) * Math.Sin(1 / theta) + Math.Pow(phi, xExponent) * Math.Sin(1 / phi);
                return new double[]{ x, y, z };
            };
            return cordFunc;
        }
        public static double[] kleinBottlePoint(double theta, double u)
        {
            //bottle shape. u was mended to range from 0 to 2pi.
            theta = 2 * Math.PI * theta;
            u = 2 * Math.PI * u;
            //http://en.wikipedia.org/wiki/Klein_bottle
            u = u * 0.5;
            double x = -(2 / 15.0) * Math.Cos(u) * (3 * Math.Cos(theta) - 30 * Math.Sin(u) + 90 * Math.Pow(Math.Cos(u), 4) * Math.Sin(u)
                - 60 * Math.Pow(Math.Cos(u), 6) * Math.Sin(u) + 5 * Math.Cos(u) * Math.Sin(u) * Math.Cos(theta));
            double y = -(1 / 15.0) * Math.Sin(u) * (3 * Math.Cos(theta) - 3 * Math.Pow(Math.Cos(u), 2) * Math.Cos(theta) - 48 * Math.Pow(Math.Cos(u), 4) * Math.Cos(theta)
                + 48 * Math.Pow(Math.Cos(u), 6) * Math.Cos(theta) - 60 * Math.Sin(u) + 5 * Math.Cos(u) * Math.Sin(u) * Math.Cos(theta)
                - 5 * Math.Pow(Math.Cos(u), 3) * Math.Sin(u) * Math.Cos(theta) - 80 * Math.Pow(Math.Cos(u), 5) * Math.Sin(u) * Math.Cos(theta) +
                80 * Math.Pow(Math.Cos(u), 7) * Math.Sin(u) * Math.Cos(theta));
            double z = (2 / 15.0) * (3 + 5 * Math.Cos(u) * Math.Sin(u)) * Math.Sin(theta);
            return new double[] { x, y, z };
        }
        public static double[] someShape4D(double theta, double u)
        {
            theta = 2 * Math.PI * theta;
            u = 2 * Math.PI * u;
            double x = 4*Math.Sin(theta)*Math.Cos(u);
            double y = 4*Math.Cos(theta)*Math.Cos(u);
            double z = 4*Math.Sin(u);
            double w = 4*Math.Sin(theta)*Math.Sin(u);
            return new double[]{x,y,z,w};
        }
        public static double[] someOtherShape4D(double u, double v)
        {
            v = 10 * Math.PI * v;
            u = 10 * Math.PI * u;
            double x =  (v*v*v - v*u*u + 2*u);
            double y =  (u*v - 2*v);
            double z =  (u*u*u - 2*u*u+ v);
            double w =  (u*v - u*u - v*v);
            return new double[] { x, y, z, w };
        }
        public static double[] someShape7D(double u, double v)
        {
            u = 5 * u;
            v = 5 * v;
            double x1 = Math.Log(1 + u);
            double x2 = Math.Log(1 + v);
            double x3 = 3 * Math.Sin(u)*Math.Sin(v);
            double x4 = 2 * Math.Log(1 + u)*Math.Sin(u);
            double x5 = 1 * Math.Tan(u);
            double x6 = 3 * Math.Sqrt(Math.Sin(u)+Math.Sin(v));
            double x7 = 5 * 1/(Math.Sin(Math.Log(1 + u*v)));
            return new double[]{x1,x2,x3,x4,x5,x6,x7};
        }
        public static double[] cusp(double u, double v)
        {
            u = 2 * Math.PI * u;
            v = 2 * v;
            double x = v * Math.Sin(u);
            double y = Math.Sqrt(v);
            double z = v * Math.Cos(u);
            return new double[] { x, y, z };
        }
        public static double[] cusp6D(double u, double v)
        {
            u = 2 * Math.PI * u;
            v = 2 * v;
            double x = v * Math.Sin(u);
            double y = Math.Sqrt(v);
            double z = v * Math.Cos(u);
            double w = Math.Sqrt(v);
            double t = v * Math.Cos(u);
            double s = v * Math.Sin(u);
            return new double[] { x, y, z, w, t, s};
        }
        public static double[] flatTorus(double theta, double phi, Dictionary<string,double> param)
        {
            //flat torus - https://en.wikipedia.org/wiki/Torus#Flat_torus
            theta = 2 * Math.PI * (theta);
            phi = 2 * Math.PI * (phi);
            double R = param["radius1"];
            double P = param["radius2"];
            double x = R * Math.Cos(theta);
            double y = R * Math.Sin(theta);
            double z = P * Math.Cos(phi);
            double w = P * Math.Sin(phi);
            return new double[] { x, y, z, w};
        }
        public static double[] bellShape(double theta, double phi)
        {
            theta = 2 * Math.PI * (theta);
            phi = 2 * Math.PI * (phi);
            const double R = 1;
            const double P = 3;
            double x = R * phi * Math.Cos(theta);
            double y = R * Math.Sin(theta);
            double z = P * Math.Cos(phi);
            return new double[] { x, y, z};
        }
        public static double[] ship(double theta, double phi)
        {
            theta = 2 * Math.PI * (theta);
            phi = 2.5 * Math.PI * (phi);
            const double R = 1;
            const double P = 3;
            double x = R * phi * Math.Cos(theta);
            double y = R * Math.Sin(theta);
            double z = P * Math.Cos(phi);
            return new double[] { x, y, z };
        }
        public static double[] klein4D(double theta, double phi)
        {
            theta = 2 * Math.PI * theta;
            phi = 2 * Math.PI * phi;
            const double R = 1;
            const double P = 3;
            const double epsilon = 0.1;
            double x = R * (Math.Cos(theta / 2) * Math.Cos(phi) - Math.Sin(theta / 2) * Math.Sin(2 * phi));
            double y = R * (Math.Sin(theta / 2) * Math.Cos(phi) + Math.Cos(theta / 2) * Math.Sin(2 * phi));
            double z = P * Math.Cos(theta) * (1 + epsilon * Math.Sin(phi));
            double w = P * Math.Sin(theta) * (1 + epsilon * Math.Sin(phi));
            return new double[] { x, y, z, w };
        }
        public static double[] kleinBottleFigureEigth4DPoint(double theta, double v)
        {
            //adjusted to run from [0,1]^2
            theta = 2 * Math.PI * theta;
            v = 2 * Math.PI * v;
            const double r = 3;
            double x = (r + Math.Cos(theta / 2) * Math.Sin(v) - Math.Sin(theta / 2) * Math.Sin(2 * v)) * Math.Cos(theta);
            double y = (r + Math.Cos(theta / 2) * Math.Sin(v) - Math.Sin(theta / 2) * Math.Sin(2 * v)) * Math.Sin(theta);
            double z = Math.Sin(theta / 2) * Math.Sin(v) + Math.Cos(theta / 2) * Math.Sin(2 * v);
            double w = Math.Cos(v);
            return new double[] { x, y, z , w};
        }

        /*
         * this method returns a method that defines the coordinates of a spiral with specific parameters
         */
        public static CoordinatesFunction spiral(double width, double radiusToWholeTurnsRatio)
        {
            //as the spiralAngle increases, so does the radius. The width is then a circle perpendicular to the angle vector.
            //for the parametric representation of a circle in 3d along an axis:
            //http://math.stackexchange.com/questions/73237/parametric-equation-of-a-circle-in-3d-space

            CoordinatesFunction cordFunc =  (theta, phi, parameters) =>
            {
                theta = -2 * Math.PI * theta;
                phi = 2 * Math.PI * phi;
                Point3D center = new Point3D((theta / (2 * Math.PI)) * radiusToWholeTurnsRatio * Math.Cos(theta),
                                               (theta / (2 * Math.PI)) * radiusToWholeTurnsRatio * Math.Sin(theta), 0);
                //compute u, a vector in direction of center such that center+u is on the circle, and v is a pi/2 rotation of u along
                //an axis perpendicular to the direction of center.
                Vector3D axis = new Vector3D(-center.Y, center.X, 0);
                Vector3D u = new Vector3D(center.X, center.Y, center.Z);
                u.Normalize();
                Vector3D v = new Vector3D(0, 0, -1);
                Point3D coord = center + width * u * Math.Cos(phi) + width * v * Math.Sin(phi);
                return new double[] { coord.X, coord.Y, coord.Z };
            };
            return cordFunc;
        }
        public static double[] plane(double x, double y)
        {
            double z = 0;
            return new double[] { x, y, z };
        }
        public static double[] xAxis(double l, double phi, Dictionary<string,double> parameters)
        {
            phi = 2 * Math.PI * phi;
            double length = 7;
            double radius = 0.1;
            double conicLength = 0.5;
            double conicMaximalRadius = 0.2;
            double x, y, z;
            x = length * l;

            if (l * length > length - conicLength)
            {
                radius = conicMaximalRadius * length * (1 - l) / conicLength;
            }
            y = radius * Math.Cos(phi);
            z = radius * Math.Sin(phi);

            return new double[]{ x, y, z };
        }
        public static double[] yAxis(double l, double phi, Dictionary<string, double> parameters)
            {

                phi = 2 * Math.PI * phi;
                double length = 7;
                double radius = 0.1;
                double conicLength = 0.5;
                double conicMaximalRadius = 0.2;
                double x, y, z;
                y = length * l;

                if (l * length > length - conicLength)
                {
                    radius = conicMaximalRadius * length * (1 - l) / conicLength;
                }
                x = radius * Math.Cos(phi);
                z = radius * Math.Sin(phi);

                return new double[]{ x, y, z };

            }
        public static double[] zAxis(double l, double phi, Dictionary<string, double> parameters)
            {
                phi = 2 * Math.PI * phi;
                double length = 7;
                double radius = 0.1;
                double conicLength = 0.5;
                double conicMaximalRadius = 0.2;
                double x, y, z;
                z = length * l;

                if (l * length > length - conicLength)
                {
                    radius = conicMaximalRadius * length * (1 - l) / conicLength;
                }
                x = radius * Math.Cos(phi);
                y = radius * Math.Sin(phi);

                return new double[]{ x, y, z };
            }
    }

}
