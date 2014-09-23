using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;

namespace ParSurf
{
    public class ViewPortGraphics
    {
        private Viewport3D viewPort;
        private GeometryModel3D geometryModel;
        private ModelVisual3D model;

        public ViewPortGraphics(Viewport3D viewPort)
        {
            this.viewPort = viewPort;
        }
        public void generate_3d_axes(int renderResolution)
        {
            ParametricSurface[] axes = new ParametricSurface[3];
            MeshGeometry3D AxisMesh = new MeshGeometry3D();
            ParametricSurface.CoordinatesFunction[] axesFunctions = new ParametricSurface.CoordinatesFunction[]{
                                                                ParametricSurface.xAxis,ParametricSurface.yAxis,ParametricSurface.zAxis};
            for (int i = 0; i < 3; i++)
            {
                axes[i] = new ParametricSurface(null, 3, axesFunctions[i],new double[]{0,1},new double[]{0,1});
            }
            int triangleIndexAxis = 0;
            for (int i = 0; i < 3; i++)
            {
                IList<double[][]> trianglesAxis = axes[i].triangulate(renderResolution, renderResolution);

                foreach (double[][] triangle in trianglesAxis)
                {
                    foreach (double[] point in triangle)
                        AxisMesh.Positions.Add(new Point3D(point[0], point[1], point[2]));

                    AxisMesh.TriangleIndices.Add(triangleIndexAxis * 3);
                    AxisMesh.TriangleIndices.Add(triangleIndexAxis * 3 + 1);
                    AxisMesh.TriangleIndices.Add(triangleIndexAxis * 3 + 2);

                    triangleIndexAxis++;
                }

                Material materialAxis = new DiffuseMaterial(
                    new SolidColorBrush(Colors.DimGray));
                GeometryModel3D AxisGeometry = new GeometryModel3D(
                    AxisMesh, materialAxis);
                AxisGeometry.BackMaterial = materialAxis;
                ModelVisual3D modelAxis = new ModelVisual3D();
                modelAxis.Content = AxisGeometry;
                modelAxis.Transform = MatrixTransform3D.Identity;
                viewPort.Children.Add(modelAxis);
            }
        }
        public void generate_viewport_object(IList<double[][]> triangles, Transform3D currentTransform = null)
        {
            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            int triangleIndex = 0;

            foreach (double[][] triangle in triangles)
            {
                Debug.Assert(triangle.Length == 3);
                //Debug.Assert(triangle[0].Length == 3);//3D only in viewports!
                foreach (double[] point in triangle)
                {
                    triangleMesh.Positions.Add(new Point3D(point[0], point[1], point[2]));
                }

                triangleMesh.TriangleIndices.Add(triangleIndex * 3);
                triangleMesh.TriangleIndices.Add(triangleIndex * 3 + 1);
                triangleMesh.TriangleIndices.Add(triangleIndex * 3 + 2);

                triangleIndex++;
            }

            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.DeepSkyBlue));
            geometryModel = new GeometryModel3D(
                triangleMesh, material);
            geometryModel.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Firebrick));
            model = new ModelVisual3D();
            model.Content = geometryModel;
            viewPort.Children.Add(model);
            if (currentTransform == null)
            {
                Transform3DGroup group = new Transform3DGroup();
                geometryModel.Transform = group;
            }
            else
                geometryModel.Transform = currentTransform;
        }
        public void performTransform(Transform3D transform)
        {
            //compose the new transform to the current one
            Transform3DGroup currentTransformGroup = geometryModel.Transform as Transform3DGroup;
            currentTransformGroup.Children.Add(transform);
        }
        public void setTransform(Transform3D transform)
        {
            //ovveride the old transform with the new one
            Transform3DGroup group = new Transform3DGroup();
            group.Children.Add(transform);
            geometryModel.Transform = group;

        }
        public Transform3D getCurrentTransform()
        {
            return geometryModel.Transform;
        }
        public void reset()
        {
            //remove only the model and axes, not the cameras.
            viewPort.Children.Remove(model);
        }
        public static double[][] convert3DTransformTo4D(Transform3D transform, int missingAxis)
        {
            int[] axesInND = new int[3];
            for(int i=0; i<3; i++)
                axesInND[i] = i >= missingAxis ? i+1 : i;
            return convert3DTransformToND(transform, 4, axesInND);
        }
        public static double[][] convert3DTransformToND(Transform3D transform, int n, int[] axesInND)
        {
            /* Given an affine transformation in 3D space, and assuming it represents a transformation on a projection of an 
            * ND shape to the axesInND axes space, constructs the affine transformation it represents in ND.
            * More on (affine) transformation with C# matrices here - 
            * http://msdn.microsoft.com/en-us/library/system.windows.media.media3d.matrix3d(v=vs.110).aspx
            **/
            Matrix3D om = transform.Value;
            double[][] originalMatrix = convert3DtransformToArrayForm(transform);//using jagged arrays for performance boost.
            double[][] newMatrix = new double[n+1][];
            for (int i = 0; i < n+1; i++)
            {
                newMatrix[i] = new double[n+1];
            }
            //take the current matrix and fill it in the new one.
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int newI = i == 3 ? n : axesInND[i];
                    int newJ = j == 3 ? n : axesInND[j];
                    newMatrix[newI][newJ] = originalMatrix[i][j];
                }
            }
            //then fill in 1 for k*k cells for k's that are not in axesInND, to represent identity on the new axes
            for(int i=0; i<n;i++)
                if(!axesInND.Contains(i))
                    newMatrix[i][i] = 1;
            return newMatrix;
        }
        public static Transform3D convert4DTransformTo3D(double[][] transform, int missingAxis)
        {
            int[] axesInND = new int[3];
            for (int i = 0; i < 3; i++)
                axesInND[i] = i >= missingAxis ? i + 1 : i;
            return convertNDTransformTo3D(transform, axesInND);
        }
        public static Transform3D convertNDTransformTo3D(double[][] transform, int[] axesInND)
        {
            /* Given an affine transformation in ND space, constructs the affine transformation
             * on a projection of the space to the three axes in axesInND.
             * More on (affine) transformation with C# matrices here - 
            * http://msdn.microsoft.com/en-us/library/system.windows.media.media3d.matrix3d(v=vs.110).aspx
            **/
            Matrix3D newT = new Matrix3D();
            newT.M44 = 1;
            newT.M14 = 0;
            newT.M24 = 0;
            newT.M34 = 0;

            newT.M11 = transform[axesInND[0]][axesInND[0]];
            newT.M12 = transform[axesInND[0]][axesInND[1]];
            newT.M13 = transform[axesInND[0]][axesInND[2]];

            newT.M21 = transform[axesInND[1]][axesInND[0]];
            newT.M22 = transform[axesInND[1]][axesInND[1]];
            newT.M23 = transform[axesInND[1]][axesInND[2]];

            newT.M31 = transform[axesInND[2]][axesInND[0]];
            newT.M32 = transform[axesInND[2]][axesInND[1]];
            newT.M33 = transform[axesInND[2]][axesInND[2]];

            newT.OffsetX = transform[transform.Length - 1][axesInND[0]];
            newT.OffsetY = transform[transform.Length - 1][axesInND[1]];
            newT.OffsetZ = transform[transform.Length - 1][axesInND[2]];

            return new MatrixTransform3D(newT);
        }
        public static double[][] convert3DtransformToArrayForm(Transform3D transform)
        {
            Matrix3D om = transform.Value;
            double[][] res = new double[4][];
            res[0] = new double[4] { om.M11, om.M12, om.M13, om.M14 };
            res[1] = new double[4] { om.M21, om.M22, om.M23, om.M24 };
            res[2] = new double[4] { om.M31, om.M32, om.M33, om.M34 };
            res[3] = new double[4] { om.OffsetX, om.OffsetY, om.OffsetZ, om.M44 };
            return res;
        }
        public static Matrix3D convert3DArrayToTransformForm(double[][] transform)
        {
            Matrix3D om = new Matrix3D(transform[0][0], transform[0][1], transform[0][2], transform[0][3],
                                       transform[1][0], transform[1][1], transform[1][2], transform[1][3],
                                       transform[2][0], transform[2][1], transform[2][2], transform[2][3],
                                       transform[3][0], transform[3][1], transform[3][2], transform[3][3]);
            return om;
        }

        public static IList<double[][]> project4DTrianglesTo3D(IList<double[][]> triangles, int missingAxis)
        {
            int[] axesInND = new int[3];
            for (int i = 0; i < 3; i++)
                axesInND[i] = i >= missingAxis ? i + 1 : i;
            return projectNDTrianglesTo3D(triangles, axesInND);
        }
        public static IList<double[][]> projectNDTrianglesTo3D(IList<double[][]> triangles, int[] axesInND)
        {
            List<double[][]> res = new List<double[][]>();
            foreach (double[][] triangle in triangles)
            {
                Debug.Assert(triangle.Length == 3);
                double[][] newTriangle = new double[3][];
                for (int i = 0; i < 3; i++)
                {
                    double[] point = triangle[i];
                    double[] newPoint = new double[3];
                    for (int j = 0; j < 3; j++)
                        newPoint[j] = point[axesInND[j]];
                    newTriangle[i] = newPoint;
                }
                res.Add(newTriangle);
            }
            return res;
        }
    }
}
