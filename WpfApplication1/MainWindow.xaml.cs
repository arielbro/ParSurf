using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        string[] previousFormulae;
        private CanvasGraphics canvasGraphics;
        private ViewPortGraphics viewPortGraphics;
        private double xCoordinateRange;
        private double yCoordinateRange;
        private List<ParametricSurface> surfaces;
        public MainWindow()
        {
            InitializeComponent();
            //List<ParametricSurface> surfaces = new List<ParametricSurface>();
            //ParametricSurface surface = new ParametricSurface("Flat Torus", 3, ParametricSurface.spherePoint, new double[] { 0, 1 }, new double[] { 0, 1 }, new Dictionary<string, double>() { { "radius", 1 } });
            //surfaces.Add(surface);
            //using (Stream stream = File.Open("Surfaces.bin", FileMode.Create))
            //{
            //    BinaryFormatter bin = new BinaryFormatter();
            //    bin.Serialize(stream, surfaces);
            //}
            using (Stream stream = File.Open("Surfaces.bin", FileMode.OpenOrCreate))
            {
                BinaryFormatter bin = new BinaryFormatter();
                surfaces = (List<ParametricSurface>)bin.Deserialize(stream);
            }
            ParametricSurface surface = surfaces[0];
            for (int i = 0; i < surfaces.Count; i++)
            {
                MenuItem item = new MenuItem();
                item.Header = surfaces[i].Name;
                item.Name = "MenuItem_" + i.ToString();
                item.Click += parametric_select_item_checked;
                MenuItem_savedParametricSurface.Items.Add(item);
            }
            
            CloseableTabItem x = new CloseableTabItem();
            x.SetHeader(new TextBlock { Text = "Tab!" });
            Frame frame = new Frame();
            ParametricSurface surface1 = new ParametricSurface();
            surface1.coordinates = ParametricSurface.flatTorus;
            frame.Content = new Page3D(surface);
            x.Content = frame;
            tabControl1.Items.Add(x);
            
            CloseableTabItem x1 = new CloseableTabItem();
            x1.SetHeader(new TextBlock { Text = "Tab!1231412412431312" });
            Frame frame1 = new Frame();
            ParametricSurface surface2 = new ParametricSurface();
            surface2.coordinates = ParametricSurface.flatTorus;
            frame1.Content = new Page3D(surface);
            x1.Content = frame1;
            tabControl1.Items.Add(x1);
        }
        private void closeTab(object source, RoutedEventArgs args)
        {
        }
        private void parametric_select_item_checked(object sender, RoutedEventArgs e)
        {
            CloseableTabItem newtab = new CloseableTabItem();
            string x =((MenuItem)sender).Name.Replace("MenuItem_", "");
            ParametricSurface surface = surfaces[int.Parse(((MenuItem)sender).Name.Replace("MenuItem_", ""))];
            newtab.SetHeader(new TextBlock { Text = surface.Name });
            Frame frame = new Frame();
            switch(surface.dimension){
                case 3:
                    frame.Content = new Page3D(surface);
                    break;
                case 4:
                    frame.Content = new Page4D(surface.triangulate(5, 5), surface.triangulate(30, 30));
                    break;
                default:
                    frame.Content = new PageND(surface.triangulate(5, 5), surface.triangulate(30, 30), surface.dimension);
                    break;
            }
            newtab.Content = frame;
            tabControl1.Items.Add(newtab);
            newtab.IsSelected = true;            
        }
        private void graphicSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //InputNumberForm form = new InputNumberForm();
            //System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
            //if (formStatus == System.Windows.Forms.DialogResult.OK)
            //{
            //    if (sender == pointSizeMenuItem)
            //    {
            //        if (Double.IsNaN(form.result) || form.result <= 0)
            //        {
            //            MessageBox.Show("Input for parameter must be a positive double");
            //            return;
            //        }
            //        pointSize = form.result;
            //        //canvasGraphics.reDraw(); 
            //        return;
            //    }
            //    else if (Double.IsNaN(form.result) || form.result <= 0 ||
            //           Convert.ToInt32(form.result) != form.result)
            //    {
            //        MessageBox.Show("Input for parameter must be a positive integer");
            //        return;
            //    }
            //    else if (sender == renderResolutionMenuItem)
            //        renderResolution = Convert.ToInt32(form.result);
            //    else if (sender == parallelResoltuionMenuItem)
            //        parallelResolution = Convert.ToInt32(form.result);

            //    //resets current viewport
            //    Transform3D currentTransform = viewPortGraphics.getCurrentTransform();
            //    viewPortGraphics.reset();
            //    //regenerate shapes (keeping original transformations)
            //    viewPortGraphics.generate_viewport_object(shape.triangulate(renderResolution,renderResolution),
            //                                            currentTransform);
            //    viewPortGraphics.generate_3d_axes(renderResolution);
            //    //canvasGraphics.reDraw();
            //}
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
                List<NCalc.Expression> expressions = new List<NCalc.Expression>();
                Dictionary<string, double> expParams = new Dictionary<string, double>();
                foreach (String exp in form.formulas)
                {
                    MatchCollection mc = Regex.Matches(exp, @"\$([A-Za-z0-9_]+)");
                    foreach (Match m in mc)
                    {
                        string temp = m.ToString().Replace("$", "");
                        if(!expParams.ContainsKey(temp))
                            expParams.Add(temp, 0);
                    }
                    exp.Replace("$", "");
                    expressions.Add(new NCalc.Expression(exp));
                }
                
                //test the input expressions
                foreach (NCalc.Expression exp in expressions)
                {
                    exp.Parameters.Add("u", 0);
                    exp.Parameters.Add("t", 0);
                    exp.Parameters.Add("Pi", Math.PI);
                    foreach (KeyValuePair<string, double> param in expParams)
                    {
                        exp.Parameters.Add(param.Key, param.Value);
                    }
                }
                try
                {
                    foreach (NCalc.Expression exp in expressions)
                    {
                        double Test = Convert.ToDouble(exp.Evaluate());
                    }
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
                foreach (NCalc.Expression exp in expressions)
                {
                    exp.Parameters.Remove("u");
                    exp.Parameters.Remove("t");
                    exp.Parameters.Remove("Pi");
                    foreach (KeyValuePair<string, double> param in expParams)
                    {
                        exp.Parameters.Remove(param.Key);
                    }
                }
                //save formulae to memory, after validating
                previousFormulae = (new List<String>(form.formulas)).ToArray();
                //create delegate coordinates function
                ParametricSurface.CoordinatesFunction cordFunc = (u, t, parameters) =>
                {
                    foreach (NCalc.Expression exp in expressions)
                    {
                        exp.Parameters.Add("Pi", Math.PI);
                        exp.Parameters.Add("u", u);
                        exp.Parameters.Add("t", t);
                        foreach (KeyValuePair<string,double> param in parameters)
                        {
                            exp.Parameters.Add(param.Key, param.Value);
                        }
                    }
                    List<Double> result = new List<double>();
                    foreach (NCalc.Expression exp in expressions)
                        result.Add(Convert.ToDouble(exp.Evaluate()));
                    foreach (NCalc.Expression exp in expressions)
                    {
                        exp.Parameters.Remove("Pi");
                        exp.Parameters.Remove("u");
                        exp.Parameters.Remove("t");
                        foreach (KeyValuePair<string, double> param in parameters)
                        {
                            exp.Parameters.Remove(param.Key);
                        }
                    }
                    return result.ToArray();
                };
                double[] urange = new double[] { Convert.ToDouble((new NCalc.Expression(form.urange[0])).Evaluate()), Convert.ToDouble((new NCalc.Expression(form.urange[1])).Evaluate()) };
                double[] trange = new double[] { Convert.ToDouble((new NCalc.Expression(form.trange[0])).Evaluate()), Convert.ToDouble((new NCalc.Expression(form.trange[1])).Evaluate()) };
                shape = new ParametricSurface(form.Name, form.dimension, cordFunc, urange, trange, expParams, false, false);
                
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
                CloseableTabItem newtab = new CloseableTabItem();
                newtab.SetHeader(new TextBlock { Text = shape.Name });
                Frame frame = new Frame();
                switch(form.dimension){
                    case 3:
                        frame.Content = new Page3D(shape);
                        break;
                    case 4:
                        frame.Content = new Page4D(shape.triangulate(5, 5), shape.triangulate(30, 30));
                        break;
                    default:
                        frame.Content = new PageND(shape.triangulate(5, 5), shape.triangulate(30, 30), shape.dimension);
                        break;
                }
                newtab.Content = frame;
                tabControl1.Items.Add(newtab);
                newtab.IsSelected = true;   

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

        private void Save_Parametric_Surface_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Save_Transformation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Load_Transformation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Save_Tab_State_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Load_Tab_State_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}