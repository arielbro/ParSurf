using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace ParSurf
{
    [Serializable]
    public abstract class Surface
    {
        static public Dictionary<string, double> mathConsts = new Dictionary<string, double>() { { "Pi", Math.PI }, { "pi", Math.PI }, { "E", Math.E }, { "e", Math.E } };
        
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

    [Serializable]
    public class ParametricSurface : Surface
    {
        public Dictionary<string, double> parameters;
        [NonSerialized]
        private IList<NCalc.Expression> formulae;
        //no need saving Expressions for those, because they will be compiled only once on triangulate
        private IList<string> variableRangesStrings;
        private IList<string> formulaeStrings;

        public ParametricSurface(string name, int dimension, IList<string> formulaeStrings,
                                 IList<string> variableRangesStrings, Dictionary<string, double> parameters = null)
            : base(name, dimension)
        {
            this.parameters = parameters ?? new Dictionary<string, double>();
            this.formulaeStrings = formulaeStrings;//actuall expressions will be cached when getPoint first called
            Debug.Assert(formulaeStrings.Count == dimension);
            this.variableRangesStrings = variableRangesStrings;
        }
        public double[] getPoint(double u, double v)
        {
            double[] point = new double[dimension];
            if (formulae == null)
            {
                formulae = new NCalc.Expression[dimension];
                for (int i = 0; i < dimension; i++)
                {
                    formulae[i] = new NCalc.Expression(formulaeStrings[i]);
                }
            }
            for (int i = 0; i < dimension; i++)
            {
                formulae[i].Parameters["u"] = u;
                formulae[i].Parameters["v"] = v;
                foreach (string parameter in parameters.Keys)
                    formulae[i].Parameters[parameter] = parameters[parameter];
                foreach (KeyValuePair<string, double> param in Surface.mathConsts)
                {
                    formulae[i].Parameters.Add(param.Key, param.Value);
                }
                object value = formulae[i].Evaluate();
                if (value.GetType() == typeof(int))
                    point[i] = (int)value;
                else//assuming double
                    point[i] = (double)value;
            }
            return point;
        }
        public override IList<double[][]> triangulate(int usteps, int vsteps)
        {
            //check to see if triangles were already created

            //// create a mesh points in a snake scale like pattern - 
            //// for each "row" v_i, iterate over u values, for each adjecent pair, create a triangle with a 
            //// lower left corner (v_(i-1) and u1) and upper right corner (v_(i+1) and u2), up to modulus (if closed)

            List<double[][]> triangles = new List<double[][]>();
            double epsilon = 0.0001; //this fixes the problem of mobius and klein having gaps in the seam area.
            double[] urange = new double[2];
            double[] vrange = new double[2];
            for (int i = 0; i < 4; i++)
            {
                NCalc.Expression exp = new NCalc.Expression(variableRangesStrings[i]);
                foreach (string param in parameters.Keys)
                {
                    exp.Parameters[param] = parameters[param];
                }
                foreach (KeyValuePair<string, double> param in Surface.mathConsts)
                {
                    exp.Parameters.Add(param.Key, param.Value);
                }
                object value = exp.Evaluate();
                double[] range = i < 2 ? urange : vrange;
                if (value.GetType() == typeof(int))
                    range[i < 2 ? i : i - 2] = (int)value;
                else//assuming double
                    range[i < 2 ? i : i - 2] = (double)value;
            }
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

                    double[] point1 = getPoint(u1, v);
                    double[] point2 = getPoint(u2, v);
                    double[] pointUpperRight = getPoint(u2, vup);
                    double[] pointLowerLeft = getPoint(u1, vdown);
                    if (vup > v)
                        triangles.Add(new double[][] { point1, point2, pointUpperRight });
                    if (vdown < v)
                        triangles.Add(new double[][] { point2, point1, pointLowerLeft });
                }
            }
            return triangles;
        }
        public static ParametricSurface getAxis(int axisNumber)
        {
            //creates the expressions required for representing an axis in 3D.
            //axisNumber varies from 0 to 2, representing x,y,z respectively. 
            double length = 7;
            double conicLength = 0.5;
            double conicMaximalRadius = 0.2;
            double pipeRadius = 0.1;

            string longitual = "u*length";
            string horizontal = "if(u*length>length-conicLength," +
                                "Cos(v)*conicMaximalRadius*length*(1-u)/conicLength," +
                                "Cos(v)*pipeRadius)";
            string vertical = "if(u*length>length-conicLength," +
                              "Sin(v)*conicMaximalRadius*length*(1-u)/conicLength," +
                              "Sin(v)*pipeRadius)";

            string[] expressionStrings = new string[] { longitual, horizontal, vertical };
            Dictionary<string, double> parameters = new Dictionary<string, double>();
            parameters["conicLength"] = conicLength;
            parameters["conicMaximalRadius"] = conicMaximalRadius;
            parameters["pipeRadius"] = pipeRadius;
            parameters["length"] = length;
            //parameters["pi"] = Math.PI;
            string[] coordinatesStrings = new string[3];
            for (int i = 0; i < 3; i++)
                coordinatesStrings[(i + axisNumber) % 3] = expressionStrings[i];
            string[] variableRangesStrings = new string[] { "0", "1", "0", "2*pi" };
            return new ParametricSurface("axis" + axisNumber, 3, coordinatesStrings, variableRangesStrings, parameters);
        }
    }
}
