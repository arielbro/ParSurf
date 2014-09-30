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

        public int parallelResolution = 30;
        public int renderResolution = 150;
        private List<Point3D[]> renderTriangles;
        private List<Point3D[]> parallelTriangles;
        double pointSize = 1;
        ParametricSurface shape;
        string[] previousFormulae;
        string previousFormulaeName;
        string[] previousFormulaeURanges;
        string[] previousFormulaevranges;
        private CanvasGraphics canvasGraphics;
        private ViewPortGraphics viewPortGraphics;
        private double xCoordinateRange;
        private double yCoordinateRange;
        private List<Surface> surfaces;
        public MainWindow()
        {
            InitializeComponent();
            using (Stream stream = File.Open("Surfaces.bin", FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                surfaces = (List<Surface>)bin.Deserialize(stream);
            }
            foreach (ParametricSurface surf in surfaces)
            {
                surf.parameters.Remove("pi");
                surf.parameters.Remove("Pi");
                surf.parameters.Remove("E");
                surf.parameters.Remove("e");
            }
            surfaces = surfaces.OrderBy(surface => surface.name).ToList();
            using (Stream stream = File.Open("Surfaces.bin", FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, surfaces);
            }
            //List<ParametricSurface> surfaces = new List<ParametricSurface>();
            //ParametricSurface surface = new ParametricSurface("Flat Torus", 3, ParametricSurface.spherePoint, new double[] { 0, 1 }, new double[] { 0, 1 }, new Dictionary<string, double>() { { "radius", 1 } });
            //surfaces.Add(surface);
            //using (Stream stream = File.Open("Surfaces.bin", FileMode.Create))
            //{
            //    BinaryFormatter bin = new BinaryFormatter();
            //    bin.Serialize(stream, surfaces);
            //}
            if (File.Exists("Surfaces.bin"))
            {
                using (Stream stream = File.Open("Surfaces.bin", FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    surfaces = (List<Surface>)bin.Deserialize(stream);
                }
            }
            else
            {
                using (Stream stream = File.Open("Surfaces.bin", FileMode.Create))
                {
                    surfaces = new List<Surface>();
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, surfaces);
                }

            }
            for (int i = 0; i < surfaces.Count; i++)
            {
                MenuItem item = new MenuItem();
                item.Header = surfaces[i].name;
                item.Name = "MenuItem_" + i.ToString();
                item.Click += parametric_select_item_checked;
                MenuItem_savedParametricSurface.Items.Add(item);
            }
            Cursor = Cursors.Arrow;
        }
        private void parametric_select_item_checked(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CloseableTabItem newtab = new CloseableTabItem();
            Surface surface = surfaces[int.Parse(((MenuItem)sender).Name.Replace("MenuItem_", ""))];
            newtab.SetHeader(new TextBlock { Text = surface.name });
            Frame frame = new Frame();
            switch (surface.dimension)
            {
                case 3:
                    frame.Content = new Page3D(surface);
                    break;
                case 4:
                    frame.Content = new Page4D(surface);
                    break;
                default:
                    frame.Content = new PageND(surface, surface.dimension);
                    break;
            }
            newtab.Content = frame;
            tabControl1.Items.Add(newtab);
            newtab.IsSelected = true;
            //force rendering of canvas before releasing mouse
            Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.ApplicationIdle, null);
        }
        private void graphicSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, double> dict = new Dictionary<string, double>();
            switch (((MenuItem)sender).Name)
            {
                case ("pointSizeMenuItem"):
                    dict.Add("Point Size", 0);
                    break;
                case ("renderResolutionMenuItem"):
                    dict.Add("Render Resolution", 0);
                    break;
                case ("parallelResoltuionMenuItem"):
                    dict.Add("Parallel Resolution", 0);
                    break;
            }
            TabItem tab = new TabItem();
            foreach (TabItem temp in this.tabControl1.Items)
            {
                if (temp.IsSelected) { tab = temp; break; }
            }
            if (((MenuItem)sender).Name == "parametersMenuItem") dict = ((ParametricSurface)((GraphicsPage)((Frame)tab.Content).Content).surface).parameters;
            InputNumberForm form = new InputNumberForm(dict);
            System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
            if (formStatus == System.Windows.Forms.DialogResult.OK)
            {
                if (sender == pointSizeMenuItem)
                {
                    if (Double.IsNaN(form.result["Point Size"]) || form.result["Point Size"] <= 0)
                    {
                        MessageBox.Show("Input for parameter must be a positive double");
                        return;
                    }
                    ((GraphicsPage)((Frame)tab.Content).Content).renderResolution = Convert.ToInt32(form.result["Point Size"]);
                    Properties.Settings.Default.pointSize = form.result["Point Size"];
                    ((GraphicsPage)((Frame)tab.Content).Content).reRender(1);
                    //canvasGraphics.reDraw(); 
                    return;
                }
                //    else if (Double.IsNaN(form.result) || form.result <= 0 ||
                //           Convert.ToInt32(form.result) != form.result)
                //    {
                //        MessageBox.Show("Input for parameter must be a positive integer");
                //        return;
                //    }
                else if (sender == renderResolutionMenuItem)
                {
                    ((GraphicsPage)((Frame)tab.Content).Content).renderResolution = Convert.ToInt32(form.result["Render Resolution"]);
                    Properties.Settings.Default.renderResolution = Convert.ToInt32(form.result["Render Resolution"]);
                    ((GraphicsPage)((Frame)tab.Content).Content).reRender(0);
                }
                else if (sender == parallelResoltuionMenuItem)
                {
                    ((GraphicsPage)((Frame)tab.Content).Content).renderResolution = Convert.ToInt32(form.result["Parallel Resolution"]);
                    Properties.Settings.Default.parallelResolution = Convert.ToInt32(form.result["Parallel Resolution"]);
                    ((GraphicsPage)((Frame)tab.Content).Content).reRender(1);
                }
                else if (sender == parametersMenuItem)
                {
                    ((ParametricSurface)((GraphicsPage)((Frame)tab.Content).Content).surface).parameters = form.result;
                    ((GraphicsPage)((Frame)tab.Content).Content).reRender(2);
                }
            }
            Properties.Settings.Default.Save();
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
        private void newParametricSurface_Click(object sender, RoutedEventArgs e)
        {
            FormulaInputForm form = new FormulaInputForm();
            if (previousFormulae != null)
            {
                form.setFormulas(previousFormulae,previousFormulaeName,previousFormulaeURanges,previousFormulaevranges);
            }
            
            System.Windows.Forms.DialogResult formStatus;// = form.ShowDialog();
            while ((formStatus = form.ShowDialog()) == System.Windows.Forms.DialogResult.OK)
            {
                previousFormulae = (new List<String>(form.formulas)).ToArray();
                previousFormulaeName = form.name;
                previousFormulaeURanges = form.urange;
                previousFormulaevranges = form.vrange;
                List<NCalc.Expression> expressions = new List<NCalc.Expression>();
                Dictionary<string, double> expParams = new Dictionary<string, double>();
                
                //find parameters in the formulae (including u/v ranges)
                foreach (string exp in form.formulas.Union(form.urange).Union(form.vrange)) 
                {
                    MatchCollection mc = Regex.Matches(exp, @"\$([A-Za-z0-9_]+)");
                    foreach (Match m in mc)
                    {
                        string temp = m.ToString().Replace("$", "");
                        if (!expParams.ContainsKey(temp))
                            expParams.Add(temp, Double.NaN);
                    }
                    string cleanExp = exp.Replace("$", "");
                    try
                    {
                        expressions.Add(new NCalc.Expression(cleanExp));
                    }
                    catch
                    {
                        MessageBox.Show("The formulae entered are not valid");
                        continue;
                    }

                }
                //prepare the strings representing the formulae in the shape to be created
                List<string> expressionStrings = new List<string>();
                List<string> variableRangesStrings = new List<string>();
                foreach (string exp in form.formulas)
                {
                    expressionStrings.Add(exp.Replace("$", ""));
                }
                foreach (string exp in form.urange.Concat(form.vrange))
                {
                    variableRangesStrings.Add(exp.Replace("$", ""));
                }
               
                //test the input expressions for legality using made up parameter values
                foreach (NCalc.Expression exp in expressions)
                {
                    exp.Parameters.Add("u", 1);
                    exp.Parameters.Add("v", 1);
                    foreach (KeyValuePair<string, double> param in expParams)
                    {
                        exp.Parameters.Add(param.Key, 1);//try to avoid devision by zero (which only *might* cause Exception)
                    }
                    foreach (KeyValuePair<string, double> param in Surface.mathConsts)
                    {
                        exp.Parameters.Add(param.Key,param.Value);
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
                    MessageBox.Show("The formulae entered are not valid");
                    continue;
                }
                //clear parameters after test
                foreach (NCalc.Expression exp in expressions)
                {
                    exp.Parameters.Remove("u");
                    exp.Parameters.Remove("v");
                    foreach (KeyValuePair<string, double> param in expParams)
                    {
                        exp.Parameters.Remove(param.Key);
                    }
                    foreach (KeyValuePair<string, double> param in Surface.mathConsts)
                    {
                        exp.Parameters.Remove(param.Key);
                    }
                }
                //create shape (note parameters will be queried again, if $ style parameters exist in formulae, replaced on GraphicPage creation)
                shape = new ParametricSurface(form.name, form.dimension, expressionStrings, variableRangesStrings, expParams);

                //uncheck all shapes items (including new, no point in it checked)
                //foreach (Control item in MenuItem_parametric_shape.Items)
                //{
                //    if (!(item is MenuItem))
                //        continue;
                //    MenuItem menuItem = item as MenuItem;
                //    if (menuItem.IsCheckable)
                //        menuItem.IsChecked = false;
                //}

                Mouse.OverrideCursor = Cursors.Wait;
                CloseableTabItem newtab = new CloseableTabItem();
                newtab.SetHeader(new TextBlock { Text = shape.name });
                Frame frame = new Frame();
                switch (form.dimension)
                {
                    case 3:
                        frame.Content = new Page3D(shape);
                        break;
                    case 4:
                        frame.Content = new Page4D(shape);
                        break;
                    default:
                        frame.Content = new PageND(shape, shape.dimension);
                        break;
                }
                newtab.Content = frame;
                tabControl1.Items.Add(newtab);
                newtab.IsSelected = true;
                break;
            }
            //force rendering of canvas before releasing mouse
            Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.ApplicationIdle, null);
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
            TabItem tab = new TabItem();
            foreach (TabItem temp in this.tabControl1.Items)
            {
                if (temp.IsSelected) { tab = temp; break; }
            }
            Surface newSurface = ((GraphicsPage)((Frame)tab.Content).Content).surface;
            surfaces.Add(newSurface);
            using (Stream stream = File.Open("Surfaces.bin", FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, surfaces);
            }
            MenuItem item = new MenuItem();
            item.Header = newSurface.name;
            item.Name = "MenuItem_" + MenuItem_savedParametricSurface.Items.Count.ToString();
            item.Click += parametric_select_item_checked;
            MenuItem_savedParametricSurface.Items.Add(item);
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