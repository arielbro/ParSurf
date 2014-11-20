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
using ParSurf.Properties;
namespace ParSurf
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public int parallelResolution;
        public int renderResolution;
        string[] previousFormulae;
        string previousFormulaeName;
        string[] previousFormulaeURanges;
        string[] previousFormulaeVRanges;

        public MainWindow()
        {
            InitializeComponent();

            object[][] toLoad;
            try
            {
                using (Stream stream = File.Open("LastTabs", FileMode.OpenOrCreate))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    toLoad = (object[][])bin.Deserialize(stream);
                }
                for (int i = 0; i < toLoad.Length; i++)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    CloseableTabItem newtab = new CloseableTabItem();
                    newtab.SetHeader(new TextBlock { Text = ((Surface)toLoad[i][0]).name });
                    Frame frame = new Frame();
                    ((TabSettings)toLoad[i][2]).loadColors();
                    switch (((Surface)toLoad[i][0]).dimension)
                    {
                        case 3:
                            frame.Content = new Page3D(((Surface)toLoad[i][0]), ((double[][])toLoad[i][1]), ((TabSettings)toLoad[i][2]));
                            break;
                        case 4:
                            frame.Content = new Page4D(((Surface)toLoad[i][0]), ((double[][])toLoad[i][1]), ((TabSettings)toLoad[i][2]));
                            break;
                        default:
                            frame.Content = new PageND(((Surface)toLoad[i][0]), ((double[][])toLoad[i][1]), ((TabSettings)toLoad[i][2]));
                            break;
                    }
                    newtab.Content = frame;
                    tabControl1.Items.Add(newtab);
                    newtab.IsSelected = true;

                }
            }
            catch { }
            parallelPointsShownMenuItem.IsEnabled = false;

            //Cursor = Cursors.Arrow;
        }
        private void graphicSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, double> dict = new Dictionary<string, double>();
            switch (((MenuItem)sender).Name)
            {
                case ("pointSizeMenuItem"):
                    dict.Add("Point Size", Settings.Default.pointSize);
                    break;
                case ("renderResolutionMenuItem"):
                    dict.Add("Render Resolution", Settings.Default.renderResolution);
                    break;
                case ("parallelResoltuionMenuItem"):
                    dict.Add("Parallel Resolution", Settings.Default.parallelResolution);
                    break;
            }
            GraphicsPage currentPage = null;
            foreach (TabItem temp in this.tabControl1.Items)
            {
                if (temp.IsSelected) { currentPage = ((Frame)(temp.Content)).Content as GraphicsPage; break; }
            }
            if (((MenuItem)sender).Name == "parametersMenuItem") dict = ((ParametricSurface)(currentPage.surface)).parameters;
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
                    Properties.Settings.Default.pointSize = form.result["Point Size"];
                    if (currentPage != null)
                    {
                        currentPage.settings.pointSize = Convert.ToInt32(form.result["Point Size"]);
                        currentPage.reRender(ReRenderingModes.Canvas);
                    }

                }
                else if (sender == renderResolutionMenuItem)
                {
                    Properties.Settings.Default.renderResolution = Convert.ToInt32(form.result["Render Resolution"]);
                    if (currentPage != null)
                    {
                        currentPage.settings.renderResolution = Convert.ToInt32(form.result["Render Resolution"]);
                        currentPage.reRender(ReRenderingModes.Viewport);
                    }
                }
                else if (sender == parallelResoltuionMenuItem)
                {
                    if (currentPage != null)
                    {
                        currentPage.settings.parallelResolution = Convert.ToInt32(form.result["Parallel Resolution"]);
                        currentPage.reRender(ReRenderingModes.Canvas);
                    }
                    Properties.Settings.Default.parallelResolution = Convert.ToInt32(form.result["Parallel Resolution"]);
                }
                else if (sender == parametersMenuItem)
                {
                    ((ParametricSurface)currentPage.surface).parameters = form.result;
                    currentPage.reRender(ReRenderingModes.Both);
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
            //first try to see if current page hosts a parametric shape. If so, use its parameterization as default.
            GraphicsPage currentPage = null;
            foreach (TabItem temp in this.tabControl1.Items)
            {
                if (temp.IsSelected) { currentPage = ((Frame)(temp.Content)).Content as GraphicsPage; break; }
            }
            if (currentPage != null && currentPage.surface.GetType() == typeof(ParametricSurface))
            {
                previousFormulae = ((ParametricSurface)currentPage.surface).formulaeStrings.ToArray();
                foreach (string paramName in ((ParametricSurface)currentPage.surface).parameters.Keys)
                    for (int i = 0;i<previousFormulae.Length;i++)
                        previousFormulae[i] = previousFormulae[i].Replace(paramName, "$" + paramName);
                previousFormulaeURanges = new string[] { ((ParametricSurface)currentPage.surface).variableRangesStrings[0],
                                                              ((ParametricSurface)currentPage.surface).variableRangesStrings[1]};
                previousFormulaeVRanges = new string[] { ((ParametricSurface)currentPage.surface).variableRangesStrings[2],
                                                              ((ParametricSurface)currentPage.surface).variableRangesStrings[3]};
            }
            //Else, use the last parameterization entered, if one exists
            if (previousFormulae != null)
            {
                form.setFormulas(previousFormulae, previousFormulaeName, previousFormulaeURanges, previousFormulaeVRanges);
            }

            System.Windows.Forms.DialogResult formStatus;// = form.ShowDialog();
            while ((formStatus = form.ShowDialog()) == System.Windows.Forms.DialogResult.OK)
            {
                previousFormulae = (new List<String>(form.formulas)).ToArray();
                previousFormulaeName = form.name;
                previousFormulaeURanges = form.urange;
                previousFormulaeVRanges = form.vrange;
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
                Surface shape = new ParametricSurface(form.name, form.dimension, expressionStrings, variableRangesStrings, expParams);

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
            Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.SystemIdle, null);
        }
        private void newPointCloudSurface_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog load = new System.Windows.Forms.OpenFileDialog();
            load.DefaultExt = ".csv";
            load.Filter = "CSV files (*.csv)|*.csv|All Files (*.*)|*.*";
            load.InitialDirectory = System.IO.Path.GetFullPath("Surfaces");
            if (!System.IO.Directory.Exists(load.InitialDirectory))
                System.IO.Directory.CreateDirectory(load.InitialDirectory);
            load.RestoreDirectory = true;
            if (load.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //change extention to make sure it's a CSV file (stackoverflow recommendation)
                string[] lines = File.ReadAllLines(System.IO.Path.ChangeExtension(load.FileName, ".csv"));
                List<double[]> points = new List<double[]>();
                foreach (string line in lines)
                {
                    string[] splitted = line.Split(new char[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        if (splitted.Length != 3)
                            throw new Exception();
                        double[] point = new double[]{Double.Parse(splitted[0]), 
                                                    Double.Parse(splitted[1]), Double.Parse(splitted[2])};
                        points.Add(point);
                    }
                    catch
                    {
                        MessageBox.Show("Error parsing input file. Check the file is appropriately formatted (csv, each line defines a 3D point)");
                    }
                }
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    PointCloudSurface surface = new PointCloudSurface(load.SafeFileName.Split('.')[0], points);
                    CloseableTabItem newtab = new CloseableTabItem();
                    newtab.SetHeader(new TextBlock { Text = surface.name });
                    Frame frame = new Frame();
                    frame.Content = new Page3D(surface);
                    newtab.Content = frame;
                    tabControl1.Items.Add(newtab);
                    newtab.IsSelected = true;
                }
                catch
                {
                    MessageBox.Show("Unfortunately, triangulation of the point cloud failed.\n For aiding in the software's improvement, we would appriciate if you send the csv file used to the software's authors.");
                }

            }
            //force rendering of canvas before releasing mouse
            Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.SystemIdle, null);
        }
        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ParSurf - parallel coordinates surface visualization\n" +
                            "By:\n" + "Ron Kirsch (ronkirsc@mail.tau.ac.il)\n" + "Kirill Savchenko (kirillsa@mail.tau.ac.il)\n" +
                            "Ariel Brunner (arielbro@mail.tau.ac.il)");
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
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.DefaultExt = ".surf";
            save.InitialDirectory = System.IO.Path.GetFullPath("Surfaces");
            if (!System.IO.Directory.Exists(save.InitialDirectory))
                System.IO.Directory.CreateDirectory(save.InitialDirectory);
            save.RestoreDirectory = true;
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TabItem tab = new TabItem();
                foreach (TabItem temp in this.tabControl1.Items)
                {
                    if (temp.IsSelected) { tab = temp; break; }
                }
                using (Stream stream = File.Open(save.FileName, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, ((GraphicsPage)((ContentControl)tab.Content).Content).surface);
                }
            }
            //TabItem tab = new TabItem();
            //foreach (TabItem temp in this.tabControl1.Items)
            //{
            //    if (temp.IsSelected) { tab = temp; break; }
            //}
            //Surface newSurface = ((GraphicsPage)((Frame)tab.Content).Content).surface;
            //surfaces.Add(newSurface);
            //using (Stream stream = File.Open("Surfaces.bin", FileMode.Create))
            //{
            //    BinaryFormatter bin = new BinaryFormatter();
            //    bin.Serialize(stream, surfaces);
            //}
            //MenuItem item = new MenuItem();
            //item.Header = newSurface.name;
            //item.Name = "MenuItem_" + MenuItem_savedParametricSurface.Items.Count.ToString();
            //item.Click += parametric_select_item_checked;
            //MenuItem_savedParametricSurface.Items.Add(item);
        }

        private void Save_Tab_State_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.DefaultExt = ".tab";
            save.InitialDirectory = System.IO.Path.GetFullPath("Tabs");
            if (!System.IO.Directory.Exists(save.InitialDirectory))
                System.IO.Directory.CreateDirectory(save.InitialDirectory);
            save.RestoreDirectory = true;
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TabItem tab = new TabItem();
                foreach (TabItem temp in this.tabControl1.Items)
                {
                    if (temp.IsSelected) { tab = temp; break; }
                }
                object[] toSave = new object[3];
                toSave[0] = ((GraphicsPage)((ContentControl)tab.Content).Content).surface;
                toSave[1] = ((GraphicsPage)((ContentControl)tab.Content).Content).currentTransform;
                ((GraphicsPage)((ContentControl)tab.Content).Content).settings.saveColors();
                toSave[2] = ((GraphicsPage)((ContentControl)tab.Content).Content).settings;
                using (Stream stream = File.Open(save.FileName, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, toSave);
                }

            }
            Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.SystemIdle);

        }
        private void Load_Tab_State_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog load = new System.Windows.Forms.OpenFileDialog();
            load.DefaultExt = ".tab";
            load.Filter = "Tab Files (*.tab)|*.tab|All Files (*.*)|*.*";
            load.InitialDirectory = System.IO.Path.GetFullPath("Tabs");
            if (!System.IO.Directory.Exists(load.InitialDirectory))
                System.IO.Directory.CreateDirectory(load.InitialDirectory);
            load.RestoreDirectory = true;
            if (load.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                object[] toLoad = new object[3];
                using (Stream stream = File.Open(load.FileName, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    toLoad = (object[])bin.Deserialize(stream);
                }

                Mouse.OverrideCursor = Cursors.Wait;
                CloseableTabItem newtab = new CloseableTabItem();
                newtab.SetHeader(new TextBlock { Text = ((Surface)toLoad[0]).name });
                Frame frame = new Frame();
                ((TabSettings)toLoad[2]).loadColors();
                switch (((Surface)toLoad[0]).dimension)
                {
                    case 3:
                        frame.Content = new Page3D(((Surface)toLoad[0]), ((double[][])toLoad[1]), ((TabSettings)toLoad[2]));
                        break;
                    case 4:
                        frame.Content = new Page4D(((Surface)toLoad[0]), ((double[][])toLoad[1]), ((TabSettings)toLoad[2]));
                        break;
                    default:
                        frame.Content = new PageND(((Surface)toLoad[0]), ((double[][])toLoad[1]), ((TabSettings)toLoad[2]));
                        break;
                }
                newtab.Content = frame;
                tabControl1.Items.Add(newtab);
                newtab.IsSelected = true;
                Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.SystemIdle);
            }
        }
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //gray-out and undo graying of the parametric-surface specific "parameters"
            Debug.Assert(e.AddedItems.Count <= 1);//could be 0 on last tab deletion
            Debug.Assert(e.RemovedItems.Count <= 1);//could be 0 on first tab addition
            setUpParallelPointsShownMenuItems();
            if (e.AddedItems.Count > 0)
            {
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.SystemIdle);
                resetTransformationMenuItem.IsEnabled = true;
                applyTransformationMenuItem.IsEnabled = true;
                parallelPointsShownMenuItem.IsEnabled = true;
                Surface surface = (Surface)((GraphicsPage)((Frame)((TabItem)e.AddedItems[0]).Content).Content).surface;
                if (surface.GetType() == typeof(ParametricSurface) && ((ParametricSurface)surface).parameters.Count > 0)
                {
                    parametersMenuItem.IsEnabled = true;
                }
                else
                    parametersMenuItem.IsEnabled = false;
            }
            else if (tabControl1.Items.Count == 0) //deleted last tab
            {
                parametersMenuItem.IsEnabled = false;
                resetTransformationMenuItem.IsEnabled = false;
                applyTransformationMenuItem.IsEnabled = false;
                parallelPointsShownMenuItem.IsEnabled = false;
            }
        }
        private void resetTransformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem tab in tabControl1.Items)
            {
                if (tab.IsSelected)
                {
                    GraphicsPage currentPage = (GraphicsPage)(((Frame)tab.Content).Content);
                    currentPage.resetTransformation();
                    break;
                }
            }
        }
        private void applyTransformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem tab in tabControl1.Items)
            {
                if (tab.IsSelected)
                {
                    GraphicsPage currentPage = (GraphicsPage)(((Frame)tab.Content).Content);

                    InputMatrixForm form = new InputMatrixForm(currentPage.surface.dimension + 1, currentPage.currentTransform);
                    System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
                    if (formStatus != System.Windows.Forms.DialogResult.OK)
                        return;
                    double[][] transformation = form.result.Item1;
                    bool isReset = form.result.Item2;
                    if (isReset)
                        currentPage.resetTransformation();
                    currentPage.applyTransformation(transformation);
                    return;
                }
            }
        }
        private void colorSchemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GraphicsPage currentPage = null;
            foreach (TabItem tab in tabControl1.Items)
                if (tab.IsSelected)
                    currentPage = (GraphicsPage)(((Frame)tab.Content).Content);

            System.Windows.Media.Color currentFrontColor = currentPage != null ? currentPage.settings.renderingFrontColor :
                                                                                Settings.Default.frontColor;
            System.Windows.Media.Color currentBackColor = currentPage != null ? currentPage.settings.renderingBackColor :
                                                                            Settings.Default.backColor;
            double currentOpacity = currentPage != null ? currentPage.settings.renderingFrontColor.A : Settings.Default.renderingOpacity;
            if (sender == colorSchemeChooseColorsMenuItem)
            {
                InputColorForm form = new InputColorForm(currentFrontColor, currentBackColor, "Front color", "Back color");
                System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
                if (formStatus != System.Windows.Forms.DialogResult.OK)
                    return;

                currentFrontColor = Settings.Default.frontColor = form.firstColor;
                currentBackColor = Settings.Default.backColor = form.secondColor;
            }
            else if (sender == colorSchemeAllBlueMenuItem)
            {
                currentFrontColor = Settings.Default.frontColor = Colors.RoyalBlue;
                currentBackColor = Settings.Default.backColor = Colors.RoyalBlue;
            }
            else if (sender == colorSchemeBlueRedMenuItem)
            {
                currentFrontColor = Settings.Default.frontColor = Colors.RoyalBlue;
                currentBackColor = Settings.Default.backColor = Colors.Firebrick;
            }
            else if (sender == colorSchemeTransperencyMenuItem)
            {
                Dictionary<string, double> transparancyParameterDict = new Dictionary<string, double>();
                transparancyParameterDict["opacity"] = Settings.Default.frontColor.ScA;
                InputNumberForm form = new InputNumberForm(transparancyParameterDict);
                System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
                if (formStatus != System.Windows.Forms.DialogResult.OK)
                    return;
                Settings.Default.renderingOpacity = currentOpacity = form.result["opacity"];
            }
            if (currentPage != null)
            {
                currentFrontColor.ScA = (float)currentOpacity;
                currentBackColor.ScA = (float)currentOpacity;
                currentPage.applyRenderingColorScheme(currentFrontColor, currentBackColor);
            }
        }

        private void parallelPointsShownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GraphicsPage currentPage = null;

            foreach (TabItem tab in tabControl1.Items)
                if (tab.IsSelected)
                {
                    MenuItem selectedMenuItem = sender as MenuItem;
                    currentPage = (GraphicsPage)(((Frame)tab.Content).Content);
                    if (selectedMenuItem.Name.Contains("manual"))
                    {
                        //manual selection - Pi_ijk included in name, plus transposed or not as '
                        string[] splittedName = ((MenuItem)sender).Name.Split('_');
                        int i = int.Parse(splittedName[1]);
                        int j = int.Parse(splittedName[2]);
                        int k = int.Parse(splittedName[3]);
                        bool isTransposed = bool.Parse(splittedName[4]);

                        //Notice that the change in IsChecked happens before the 
                        //event handler is called (so when IsChecked == true, the user just checked it)
                        List<Tuple<int, int, int>> relevantList = isTransposed ? currentPage.settings.transposedPLanePointsShown :
                                                       currentPage.settings.originalPLanePointsShown;
                        if (selectedMenuItem.IsChecked)
                            relevantList.Add(new Tuple<int, int, int>(i, j, k));
                        else
                            relevantList.Remove(new Tuple<int, int, int>(i, j, k));

                    }
                    else
                    {
                        List<Tuple<int, int, int>> originalPoints = currentPage.settings.originalPLanePointsShown;
                        List<Tuple<int, int, int>> transposedPoints = currentPage.settings.transposedPLanePointsShown;
                        originalPoints.Clear();
                        transposedPoints.Clear();

                        if (sender == parallelPointsShownMenuItemStandard || sender == parallelPointsShownMenuItemNoTransposed)
                            for (int i = 0; i < currentPage.dimension - 2; i++)
                                originalPoints.Add(new Tuple<int, int, int>(i, i + 1, i + 2));
                        if (sender == parallelPointsShownMenuItemStandard || sender == parallelPointsShownMenuItemOnlyTransposed)
                            for (int i = 0; i < currentPage.dimension - 2; i++)
                                transposedPoints.Add(new Tuple<int, int, int>(i, i + 1, i + 2));
                        if (sender == parallelPointsShownMenuItemAll)
                        {
                            for (int i = 0; i < currentPage.dimension - 2; i++)
                                for (int j = i + 1; j < currentPage.dimension - 1; j++)
                                    for (int k = j + 1; k < currentPage.dimension; k++)
                                    {
                                        originalPoints.Add(new Tuple<int, int, int>(i, j, k));
                                        transposedPoints.Add(new Tuple<int, int, int>(i, j, k));
                                    }

                        }

                        setUpParallelPointsShownMenuItems();
                    }
                    currentPage.applyParallelColorScheme(currentPage.settings.originalPlanePointsColor,
                                                         currentPage.settings.transposedPlanePointsColor,
                                                         currentPage.settings.isPlanePointsColoringGradient,
                                                         currentPage.settings.isPlanePointsColoringArbitrary);
                    currentPage.reRender(ReRenderingModes.Canvas);
                    break;
                }
        }
        private void setUpParallelPointsShownMenuItems()
        {
            foreach (TabItem tab in tabControl1.Items)
            {
                if (tab.IsSelected)
                {
                    Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.SystemIdle);
                    GraphicsPage currentPage = (GraphicsPage)(((Frame)tab.Content).Content);
                    //set up Pi_ijk and Pi_ijk' for each non-decreasing ijk trio 
                    parallelPointsShownMenuItemChooseManually.Items.Clear();
                    for (int i = 0; i < currentPage.dimension; i++)
                        for (int j = i + 1; j < currentPage.dimension; j++)
                            for (int k = j + 1; k < currentPage.dimension; k++)
                            {
                                //note that "normal" people use notations starting with 1 for axes.
                                foreach (bool isTransposed in new bool[] { false, true })
                                {
                                    MenuItem pi_ijk_MenuItem = new MenuItem();
                                    pi_ijk_MenuItem.Header = string.Format("Pi_{0}{1}{2}{3}", i + 1, j + 1, k + 1, isTransposed ? "'" : "");
                                    pi_ijk_MenuItem.Name = string.Format("manual_{0}_{1}_{2}_{3}", i, j, k, isTransposed ? "true" : "false");
                                    pi_ijk_MenuItem.IsCheckable = true;

                                    List<Tuple<int, int, int>> relevantList = isTransposed ?
                                                                                        currentPage.settings.transposedPLanePointsShown :
                                                                                        currentPage.settings.originalPLanePointsShown;
                                    pi_ijk_MenuItem.IsChecked = relevantList.Contains(new Tuple<int, int, int>(i, j, k));
                                    pi_ijk_MenuItem.Click += parallelPointsShownMenuItem_Click;

                                    parallelPointsShownMenuItemChooseManually.Items.Add(pi_ijk_MenuItem);
                                }
                            }
                    break;
                }
            }
        }
        private void parallelColorSchemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GraphicsPage currentPage = null;
            foreach (TabItem tab in tabControl1.Items)
                if (tab.IsSelected)
                    currentPage = (GraphicsPage)(((Frame)tab.Content).Content);

            System.Windows.Media.Color currentOriginalPointsColor = currentPage != null ? currentPage.settings.originalPlanePointsColor :
                                                                                Settings.Default.originalPlanePointsColor;
            System.Windows.Media.Color currentTransposedPointsColor = currentPage != null ? currentPage.settings.transposedPlanePointsColor :
                                                                            Settings.Default.transposedPlanePointsColor;
            bool currentIsGradient = currentPage != null ? currentPage.settings.isPlanePointsColoringGradient :
                                                                            Settings.Default.isPlanePointsColoringGradient;
            bool currentIsArbitrary = currentPage != null ? currentPage.settings.isPlanePointsColoringArbitrary :
                                                                            Settings.Default.isPlanePointsColoringArbitrary;

            if (sender == parallelColorSchemeMenuItemChooseTwoColors)
            {
                InputColorForm form = new InputColorForm(currentOriginalPointsColor, currentTransposedPointsColor,
                                                        "Original points", "Transposed points");
                System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
                if (formStatus != System.Windows.Forms.DialogResult.OK)
                    return;

                currentOriginalPointsColor = Settings.Default.originalPlanePointsColor = form.firstColor;
                currentTransposedPointsColor = Settings.Default.transposedPlanePointsColor = form.secondColor;
                currentIsGradient = Settings.Default.isPlanePointsColoringGradient = false;
                currentIsArbitrary = Settings.Default.isPlanePointsColoringArbitrary = false;
            }
            else if (sender == parallelColorSchemeMenuItemChooseTwoColorsGradient)
            {
                InputColorForm form = new InputColorForm(currentOriginalPointsColor, currentTransposedPointsColor,
                                                        "Original points", "Transposed points");
                System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
                if (formStatus != System.Windows.Forms.DialogResult.OK)
                    return;

                currentOriginalPointsColor = Settings.Default.originalPlanePointsColor = form.firstColor;
                currentTransposedPointsColor = Settings.Default.transposedPlanePointsColor = form.secondColor;
                currentIsGradient = Settings.Default.isPlanePointsColoringGradient = true;
                currentIsArbitrary = Settings.Default.isPlanePointsColoringArbitrary = false;
            }
            else if (sender == parallelColorSchemeMenuItemGreenAndRedGradient)
            {
                currentOriginalPointsColor = Settings.Default.originalPlanePointsColor = Colors.Red;
                currentTransposedPointsColor = Settings.Default.transposedPlanePointsColor = Colors.Green;
                currentIsGradient = Settings.Default.isPlanePointsColoringGradient = true;
                currentIsArbitrary = Settings.Default.isPlanePointsColoringArbitrary = false;
            }
            else if (sender == parallelColorSchemeMenuItemArbitrary)
            {
                currentOriginalPointsColor = Settings.Default.originalPlanePointsColor = Colors.Red;
                currentTransposedPointsColor = Settings.Default.transposedPlanePointsColor = Colors.Green;
                currentIsGradient = Settings.Default.isPlanePointsColoringGradient = false;
                currentIsArbitrary = Settings.Default.isPlanePointsColoringArbitrary = true;
            }
            Settings.Default.Save();
            if (currentPage != null)
            {
                currentPage.applyParallelColorScheme(currentOriginalPointsColor, currentTransposedPointsColor,
                                                currentIsGradient, currentIsArbitrary);
            }

        }
        private void mainWindow_Closing(object sender, EventArgs e)
        {
            object[][] toSave = new object[tabControl1.Items.Count][];
            for (int i = 0; i < tabControl1.Items.Count; i++)
            {
                TabItem tab = (TabItem)tabControl1.Items[i];
                toSave[i] = new object[3];
                toSave[i][0] = ((GraphicsPage)((ContentControl)tab.Content).Content).surface;
                toSave[i][1] = ((GraphicsPage)((ContentControl)tab.Content).Content).currentTransform;
                ((GraphicsPage)((ContentControl)tab.Content).Content).settings.saveColors();
                toSave[i][2] = ((GraphicsPage)((ContentControl)tab.Content).Content).settings;
            }
            using (Stream stream = File.Open("LastTabs", FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, toSave);
            }
        }
        private void Load_Parametric_Surface_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog load = new System.Windows.Forms.OpenFileDialog();
            load.DefaultExt = ".surf";
            load.Filter = "Surface Files (*.surf)|*.surf|All Files (*.*)|*.*";
            load.InitialDirectory = System.IO.Path.GetFullPath("Surfaces");
            if (!System.IO.Directory.Exists(load.InitialDirectory))
                System.IO.Directory.CreateDirectory(load.InitialDirectory);
            load.RestoreDirectory = true;
            if (load.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Surface toLoad;
                using (Stream stream = File.Open(load.FileName, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    toLoad = (Surface)bin.Deserialize(stream);
                }

                Mouse.OverrideCursor = Cursors.Wait;
                CloseableTabItem newtab = new CloseableTabItem();
                newtab.SetHeader(new TextBlock { Text = toLoad.name });
                Frame frame = new Frame();
                switch (toLoad.dimension)
                {
                    case 3:
                        frame.Content = new Page3D(toLoad);
                        break;
                    case 4:
                        frame.Content = new Page4D(toLoad);
                        break;
                    default:
                        frame.Content = new PageND(toLoad, toLoad.dimension);
                        break;
                }
                newtab.Content = frame;
                tabControl1.Items.Add(newtab);
                newtab.IsSelected = true;

            }
        }
        private void CreateSaveBitmap(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.DefaultExt = "";
            save.InitialDirectory = System.IO.Path.GetFullPath("Snapshots");
            if (!System.IO.Directory.Exists(save.InitialDirectory))
                System.IO.Directory.CreateDirectory(save.InitialDirectory);
            save.RestoreDirectory = true;
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GraphicsPage currentPage = null;
                foreach (TabItem tab in tabControl1.Items)
                    if (tab.IsSelected)
                        currentPage = (GraphicsPage)(((Frame)tab.Content).Content);
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
             (int)currentPage.canvas.ActualWidth * 500 / 96, (int)currentPage.canvas.ActualHeight * 500 / 96,
             500, 500, PixelFormats.Pbgra32);
                // needed otherwise the image output is black
                currentPage.canvas.Measure(new Size((int)currentPage.canvas.ActualWidth, (int)currentPage.canvas.ActualHeight));
                //currentPage.canvas.Arrange(new Rect(new Size((int)currentPage.canvas.ActualWidth, (int)currentPage.canvas.ActualHeight)));
                renderBitmap.Render(currentPage.canvas);
                //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                using (FileStream file = File.Create(save.FileName + "Canvas.png"))
                {
                    encoder.Save(file);
                }
                if (currentPage.dimension == 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        RenderTargetBitmap bmp = new RenderTargetBitmap((int)currentPage.viewports[0].ActualWidth * 500 / 96, (int)currentPage.viewports[0].ActualHeight * 500 / 96, 500, 500, PixelFormats.Pbgra32);
                        bmp.Render(currentPage.viewports[i]);
                        Clipboard.SetImage(bmp);
                        PngBitmapEncoder png = new PngBitmapEncoder();
                        png.Frames.Add(BitmapFrame.Create(bmp));
                        using (Stream stm = File.Create(save.FileName + "3DView" + i + ".png"))
                        {
                            png.Save(stm);
                        }
                    }
                }
                else
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)currentPage.viewports[0].ActualWidth * 500 / 96, (int)currentPage.viewports[0].ActualHeight * 500 / 96, 500, 500, PixelFormats.Pbgra32);
                    bmp.Render(currentPage.viewports[0]);
                    Clipboard.SetImage(bmp);
                    PngBitmapEncoder png = new PngBitmapEncoder();
                    png.Frames.Add(BitmapFrame.Create(bmp));
                    using (Stream stm = File.Create(save.FileName + "3DView.png"))
                    {
                        png.Save(stm);
                    }
                }
            }
        }
    }
}