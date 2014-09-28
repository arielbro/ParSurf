using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParSurf
{
    [Serializable()]
    public abstract class Surface
    {
        public int dimension;
        public string name;
        public IList<double[]> triangles;

        public Surface(string name, int dimension)
        {
            this.name = name;
            this.dimension = dimension;
        }

        public abstract IList<double[][]> triangulate(int usteps, int vsteps);
    }

    [Serializable()]
    public class ParametricSurface : Surface
    {
        public int dimension;
        public string name;
        private double[] urange;
        private double[] vrange;
        private Boolean uclosed;
        private Boolean vclosed;
        public delegate double[] CoordinatesFunction(double u, double v, Dictionary<string, double> parameters);
        public CoordinatesFunction coordinates;
        public Dictionary<string, double> parameters;

        public ParametricSurface(string name, int dimension, CoordinatesFunction coordinates, 
                                double[] urange, double[] vrange, Dictionary<String, double> parameters = null) : base(name, dimension)
        {
            this.dimension = dimension;
            this.coordinates = coordinates;
            this.parameters = parameters;
            this.urange = urange;
            this.vrange = vrange;
        }

        public override IList<double[][]> triangulate(int usteps, int vsteps)
        {
            //check to see if triangles were already created

            //// create a mesh points in a snake scale like pattern - 
            //// for each "row" v_i, iterate over u values, for each adjecent pair, create a triangle with a 
            //// lower left corner (v_(i-1) and u1) and upper right corner (v_(i+1) and u2), up to modulus (if closed)
            if (this.coordinates == null) throw new Exception("a parametric surface needs a coordinate function before triangulating");

            List<double[][]> triangles = new List<double[][]>();
            double epsilon = 0.0001; //this fixes the problem of mobius and klein having gaps in the seam area.
            for (double ustep = 0; ustep < usteps; ustep++)
            {
                for (double vstep = 0; vstep < vsteps; vstep++)
                {
                    double u1 = urange[0] + ustep / usteps * (urange[1] - urange[0]);
                    double u2 = urange[0] + ((ustep + 1) / usteps * (urange[1] - urange[0])) % (urange[1] - urange[0] + epsilon);
                    if (u2 < u1) break;
                    double v = vrange[0] + vstep / vsteps * (vrange[1] - vrange[0]);
                    double vup = vrange[0] + ((vstep + 1) / vsteps * (vrange[1] - vrange[0])) % (vrange[1] - vrange[0] + epsilon);
                    double vdown = vrange[0] + ((vstep - 1) / vsteps * (vrange[1] - vrange[0])) % (vrange[1] - vrange[0] + epsilon);

                    double[] point1 = coordinates(u1, v, parameters);
                    double[] point2 = coordinates(u2, v, parameters);
                    double[] pointUpperRight = coordinates(u2, vup, parameters);
                    double[] pointLowerLeft = coordinates(u1, vdown, parameters);
                    if (vup > v)
                        triangles.Add(new double[][] { point1, point2, pointUpperRight });
                    if (vdown < v)
                        triangles.Add(new double[][] { point2, point1, pointLowerLeft });
                }
            }
            return triangles;
        }

        public static double[] xAxis(double l, double phi, Dictionary<string, double> parameters)
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

            return new double[] { x, y, z };
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

            return new double[] { x, y, z };

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

            return new double[] { x, y, z };
        }
    }
}
