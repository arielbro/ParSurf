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
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using NCalc;
using System.IO;
using ParSurf;
using System.Runtime.Serialization.Formatters.Binary;
namespace ParSurf
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int parallelResolution = 10;
        private int renderResolution = 150;
        private List<Point3D[]> renderTriangles;
        private List<Point3D[]> parallelTriangles;
        double pointSize = 1;
        ParametricSurface shape;
        String[] previousFormulae;
        private CanvasGraphics canvasGraphics;
        private ViewPortGraphics viewPortGraphics;
        private double xCoordinateRange;
        private double yCoordinateRange;

        public MainWindow()
        {
            InitializeComponent();
            //List<ParametricSurface> surfaces = new List<ParametricSurface>();
            //ParametricSurface surface = new ParametricSurface("Flat Torus", ParametricSurface.spherePoint, new double[] { 0, 1 }, new double[] { 0, 1 }, new Dictionary<string, double>() { { "radius", 1 } });
            //surfaces.Add(surface);
            //using (Stream stream = File.Open("Surfaces.bin", FileMode.Create))
            //{
            //    BinaryFormatter bin = new BinaryFormatter();
            //    bin.Serialize(stream, surfaces);
            //}
            List<ParametricSurface> surfaces = new List<ParametricSurface>();
            using (Stream stream = File.Open("Surfaces.bin", FileMode.OpenOrCreate))
            {
                BinaryFormatter bin = new BinaryFormatter();
                surfaces = (List<ParametricSurface>)bin.Deserialize(stream);
            }
            ParametricSurface surface = surfaces[0];

            CloseableTabItem x = new CloseableTabItem();
            x.SetHeader(new TextBlock { Text = "Tab!" });
            Frame frame = new Frame();
            ParametricSurface surface1 = new ParametricSurface();
            
            surface1.coordinates = ParametricSurface.flatTorus;
            frame.Content = new Page3D(surface.triangulate(5,5),surface.triangulate(30,30));

            //surface.coordinates = ParametricSurface.cusp6D;
            //frame.Content = new PageND(surface.triangulate(30, 30), surface.triangulate(150, 150), 6);

            //surface.coordinates = ParametricSurface.flatTorus;
            //frame.Content = new Page3D(surface.triangulate(30, 30), surface.triangulate(200, 200));
            x.Content = frame;
            tabControl1.Items.Add(x);
            //CloseableTabItem y = new CloseableTabItem();
            //y.SetHeader(new TextBlock { Text = "Tab!112111111111111111111111111111123" });
            //y.Content = frame;
            //tabControl1.Items.Add(y);
            //CloseableTabItem z = new CloseableTabItem();
            //z.SetHeader(new TextBlock { Text = "Tab!3333333333333456" });
            //z.Content = frame;
            //tabControl1.Items.Add(z);
            //CloseableTabItem w = new CloseableTabItem();
            //w.SetHeader(new TextBlock { Text = "Tab!75555555555555555555555555555555555555555589" });
            //w.Content = frame;
            //tabControl1.Items.Add(w);
            
            CloseableTabItem x1 = new CloseableTabItem();
            x1.SetHeader(new TextBlock { Text = "Tab!1231412412431312" });
            
            Frame frame1 = new Frame();
            ParametricSurface surface2 = new ParametricSurface();

            surface2.coordinates = ParametricSurface.flatTorus;
            frame1.Content = new Page3D(surface.triangulate(5, 5), surface.triangulate(30, 30));

            //surface.coordinates = ParametricSurface.cusp6D;
            //frame.Content = new PageND(surface.triangulate(30, 30), surface.triangulate(150, 150), 6);

            //surface.coordinates = ParametricSurface.flatTorus;
            //frame.Content = new Page3D(surface.triangulate(30, 30), surface.triangulate(200, 200));
            x1.Content = frame1;
            tabControl1.Items.Add(x1);
        }
        private void closeTab(object source, RoutedEventArgs args)
        {
        }
        private void parametric_select_item_checked(object sender, RoutedEventArgs e)
        {
        //    //uncheck others (create a mutually exclusive choice scenario)
        //    foreach (Control item in MenuItem_parametric_shape.Items)
        //    {
        //        if (!(item is MenuItem))
        //            continue;
        //        MenuItem menuItem = item as MenuItem;
        //        if (menuItem.IsCheckable && menuItem != sender)
        //            menuItem.IsChecked = false;
        //    }

        //    //resets current viewport/canvas
        //    viewPortGraphics.reset();
        //    canvasGraphics.clearCanvasPoints();
        //    canvasGraphics.clearAxes();

        //    //load the appropriate shape (WARNING: UGLY CODING AHEAD)
        //    shape = new ParametricSurface(false, false);
        //    if (sender == MenuItem_plane)
        //        shape.coordinates = ParametricSurface.plane;
        //    else if (sender == MenuItem_mobius)
        //        shape.coordinates = ParametricSurface.mobiusPoint;
        //    else if (sender == MenuItem_bottle)
        //        shape.coordinates = ParametricSurface.kleinBottlePoint;
        //    else if (sender == MenuItem_figure_8)
        //        shape.coordinates = ParametricSurface.kleinBottleFigureEigthPoint;
        //    else if (sender == MenuItem_cone)
        //        shape.coordinates = ParametricSurface.conePoint;
        //    else if (sender == MenuItem_cylinder)
        //        shape.coordinates = ParametricSurface.cylinderPoint;
        //    else if (sender == MenuItem_sin)
        //        shape.coordinates = ParametricSurface.sinWaves;
        //    else if (sender == MenuItem_torus)
        //        shape.coordinates = ParametricSurface.torusPoint;
        //    else if (sender == MenuItem_torus)
        //        shape.coordinates = ParametricSurface.spherePoint;
        //    else throw new Exception("menu parametric shape selection does not make sense");
        //    //parallelTriangles = shape.triangulate(parallelResolution,parallelResolution);
        //    //renderTriangles = shape.triangulate(renderResolution, renderResolution);
        //    //viewPortGraphics.generate_viewport_object(renderTriangles);
        //    //viewPortGraphics.generate_3d_axes(renderResolution);
        }
        private void graphicSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InputNumberForm form = new InputNumberForm();
            System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
            if (formStatus == System.Windows.Forms.DialogResult.OK)
            {
                if (sender == pointSizeMenuItem)
                {
                    if (Double.IsNaN(form.result) || form.result <= 0)
                    {
                        MessageBox.Show("Input for parameter must be a positive double");
                        return;
                    }
                    pointSize = form.result;
                    //canvasGraphics.reDraw(); 
                    return;
                }
                else if (Double.IsNaN(form.result) || form.result <= 0 ||
                       Convert.ToInt32(form.result) != form.result)
                {
                    MessageBox.Show("Input for parameter must be a positive integer");
                    return;
                }
                else if (sender == renderResolutionMenuItem)
                    renderResolution = Convert.ToInt32(form.result);
                else if (sender == parallelResoltuionMenuItem)
                    parallelResolution = Convert.ToInt32(form.result);

                //resets current viewport
                Transform3D currentTransform = viewPortGraphics.getCurrentTransform();
                viewPortGraphics.reset();
                //regenerate shapes (keeping original transformations)
                viewPortGraphics.generate_viewport_object(shape.triangulate(renderResolution,renderResolution),
                                                        currentTransform);
                viewPortGraphics.generate_3d_axes(renderResolution);
                //canvasGraphics.reDraw();
            }
        }
        private void graphicSettingsNew_Click(object sender, RoutedEventArgs e)
        {
            FormulaInputForm form = new FormulaInputForm();
            if (previousFormulae != null)
            {
                form.setFormulas(previousFormulae);
            }
            System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
            if (formStatus == System.Windows.Forms.DialogResult.OK)
            {
                NCalc.Expression xExp = new NCalc.Expression(form.XFormula);
                NCalc.Expression yExp = new NCalc.Expression(form.YFormula);
                NCalc.Expression zExp = new NCalc.Expression(form.ZFormula);

                //test the input expressions
                foreach (NCalc.Expression exp in new NCalc.Expression[] { xExp, yExp, zExp })
                {
                    exp.Parameters.Add("u", 0);
                    exp.Parameters.Add("t", 0);
                }
                try
                {
                    double xTest = Convert.ToDouble(xExp.Evaluate());
                    double yTest = Convert.ToDouble(yExp.Evaluate());
                    double zTest = Convert.ToDouble(zExp.Evaluate());
                }
                catch
                {
                    //String errors = xTest.HasValue ? "" : "X ";
                    //errors += yTest.HasValue ? "" : "Y ";
                    //errors += zTest.HasValue ? "" : "Z ";
                    //MessageBox.Show("The following coordinate formulae contain errors:" + errors);
                    MessageBox.Show("The formulae entered are not valid");
                    //MenuItem_new.IsChecked = false;
                    return;
                }
                //clear parameters after test
                foreach (NCalc.Expression exp in new NCalc.Expression[] { xExp, yExp, zExp })
                {
                    exp.Parameters.Remove("u");
                    exp.Parameters.Remove("t");
                }
                //save formulae to memory, after validating
                previousFormulae = new String[] { form.XFormula, form.YFormula, form.ZFormula };

                //create delegate coordinates function
                ParametricSurface.CoordinatesFunction cordFunc = (u, t, parameters) =>
                {
                    foreach (NCalc.Expression exp in new NCalc.Expression[] { xExp, yExp, zExp })
                    {
                        exp.Parameters.Add("u", u);
                        exp.Parameters.Add("t", t);
                    }
                    double x = Convert.ToDouble(xExp.Evaluate());
                    double y = Convert.ToDouble(yExp.Evaluate());
                    double z = Convert.ToDouble(zExp.Evaluate());
                    foreach (NCalc.Expression exp in new NCalc.Expression[] { xExp, yExp, zExp })
                    {
                        exp.Parameters.Remove("u");
                        exp.Parameters.Remove("t");
                    }
                    return new double[]{x, y, z};
                };
                shape = new ParametricSurface(false, false);
                shape.coordinates = cordFunc;

                //uncheck all shapes items (including new, no point in it checked)
                //foreach (Control item in MenuItem_parametric_shape.Items)
                //{
                //    if (!(item is MenuItem))
                //        continue;
                //    MenuItem menuItem = item as MenuItem;
                //    if (menuItem.IsCheckable)
                //        menuItem.IsChecked = false;
                //}

                //resets current viewport
                Transform3D currentTransform = viewPortGraphics.getCurrentTransform();
                viewPortGraphics.reset();
                //regenerate shapes (keeping original transformations)
                viewPortGraphics.generate_viewport_object(shape.triangulate(renderResolution, renderResolution),
                                                        currentTransform);
                viewPortGraphics.generate_3d_axes(renderResolution);
                //canvasGraphics.reDraw();
            }
        }

        private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double widthFactor = 1;
            double heightFactor = 1;
            if (e.PreviousSize.Width != 0)
            {
                widthFactor = e.NewSize.Width / e.PreviousSize.Width;
                heightFactor = e.NewSize.Height / e.PreviousSize.Height;
            }
            else
            {
                widthFactor = e.NewSize.Width / 1000;
                heightFactor = e.NewSize.Height / 500;
            }
            foreach (ContentControl tab in tabControl1.Items)
            {
                ((Page)((ContentControl)tab.Content).Content).Width *= widthFactor;
                ((Page)((ContentControl)tab.Content).Content).Height *= heightFactor;
            }
        }
    }
}