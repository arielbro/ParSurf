﻿using System;
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

        public int parallelResolution = 30;
        public int renderResolution = 150;
        ParametricSurface shape;
        string[] previousFormulae;
        string previousFormulaeName;
        string[] previousFormulaeURanges;
        string[] previousFormulaevranges;
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
            //not implemented yet
            MenuItem_PointCloud.IsEnabled = false;
            MenuItem_ImplicitEquation.IsEnabled = false;

            //Cursor = Cursors.Arrow;
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
            //force rendering of canvas before releasing mouse.
            Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Arrow; }), DispatcherPriority.ApplicationIdle, null);
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
                    ((GraphicsPage)((Frame)tab.Content).Content).reRender(ReRenderingModes.Canvas);
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
                    ((GraphicsPage)((Frame)tab.Content).Content).renderResolution = Properties.Settings.Default.renderResolution;
                    ((GraphicsPage)((Frame)tab.Content).Content).reRender(ReRenderingModes.Viewport);
                }
                else if (sender == parallelResoltuionMenuItem)
                {
                    ((GraphicsPage)((Frame)tab.Content).Content).renderResolution = Convert.ToInt32(form.result["Parallel Resolution"]);
                    Properties.Settings.Default.parallelResolution = Convert.ToInt32(form.result["Parallel Resolution"]);
                    ((GraphicsPage)((Frame)tab.Content).Content).parallelResolution = Properties.Settings.Default.parallelResolution;
                    ((GraphicsPage)((Frame)tab.Content).Content).reRender(ReRenderingModes.Canvas);
                }
                else if (sender == parametersMenuItem)
                {
                    ((ParametricSurface)((GraphicsPage)((Frame)tab.Content).Content).surface).parameters = form.result;
                    ((GraphicsPage)((Frame)tab.Content).Content).reRender(ReRenderingModes.Both);
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
                form.setFormulas(previousFormulae, previousFormulaeName, previousFormulaeURanges, previousFormulaevranges);
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
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TabItem tab = new TabItem();
                foreach (TabItem temp in this.tabControl1.Items)
                {
                    if (temp.IsSelected) { tab = temp; break; }
                }
                object[] toSave = new object[5];
                toSave[0] = ((GraphicsPage)((ContentControl)tab.Content).Content).surface;
                toSave[1] = ((GraphicsPage)((ContentControl)tab.Content).Content).currentTransform;
                toSave[2] = ((GraphicsPage)((ContentControl)tab.Content).Content).renderResolution;
                toSave[3] = ((GraphicsPage)((ContentControl)tab.Content).Content).parallelResolution;
                toSave[4] = ((GraphicsPage)((ContentControl)tab.Content).Content).pointSize;
                using (Stream stream = File.Open(save.FileName, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, toSave);
                }
            }
        }
        private void Load_Tab_State_Click(object sender, RoutedEventArgs e)
        {

        }
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //gray-out and undo graying of the parametric-surface specific "parameters"
            Debug.Assert(e.AddedItems.Count <= 1);//could be 0 on last tab deletion
            Debug.Assert(e.RemovedItems.Count <= 1);//could be 0 on first tab addition
            setUpParallelPointsShownMenuItems();
            if (e.AddedItems.Count > 0)
            {
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ApplicationIdle);
                resetTransformationMenuItem.IsEnabled = true;
                applyTransformationMenuItem.IsEnabled = true;
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
                    double[][] transformation = form.result;

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

            System.Windows.Media.SolidColorBrush currentFrontColor = currentPage != null ? currentPage.renderingFrontColor :
                                                                                Settings.Default.frontColor;
            System.Windows.Media.SolidColorBrush currentBackColor = currentPage != null ? currentPage.renderingBackColor :
                                                                            Settings.Default.backColor;
            double currentOpacity = currentPage != null ? currentPage.renderingFrontColor.Opacity : Settings.Default.renderingOpacity;
            if (sender == colorSchemeChooseColorsMenuItem)
            {
                InputColorForm form = new InputColorForm(currentFrontColor, currentBackColor);
                System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
                if (formStatus != System.Windows.Forms.DialogResult.OK)
                    return;

                currentFrontColor = Settings.Default.frontColor = form.frontColor;
                currentBackColor = Settings.Default.backColor = form.backColor;
            }
            else if (sender == colorSchemeAllBlueMenuItem)
            {
                currentFrontColor = Settings.Default.frontColor = new SolidColorBrush(Colors.RoyalBlue);
                currentBackColor = Settings.Default.backColor = new SolidColorBrush(Colors.RoyalBlue);
            }
            else if (sender == colorSchemeBlueRedMenuItem)
            {
                currentFrontColor = Settings.Default.frontColor = new SolidColorBrush(Colors.RoyalBlue);
                currentBackColor = Settings.Default.backColor = new SolidColorBrush(Colors.Firebrick);
            }
            else if (sender == colorSchemeTransperencyMenuItem)
            {
                Dictionary<string, double> transparancyParameterDict = new Dictionary<string, double>();
                transparancyParameterDict["opacity"] = Settings.Default.frontColor.Opacity;
                InputNumberForm form = new InputNumberForm(transparancyParameterDict);
                System.Windows.Forms.DialogResult formStatus = form.ShowDialog();
                if (formStatus != System.Windows.Forms.DialogResult.OK)
                    return;
                Settings.Default.renderingOpacity = currentOpacity = form.result["opacity"];
            }
            if (currentPage != null)
            {
                currentFrontColor = currentFrontColor.Clone();
                currentFrontColor.Opacity = currentOpacity;
                currentBackColor = currentBackColor.Clone();
                currentBackColor.Opacity = currentOpacity;
                currentPage.applyRenderingColorScheme(currentFrontColor, currentBackColor);
            }
        }
        private void Save_Snapshot_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog load = new System.Windows.Forms.OpenFileDialog();
            if (load.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                object[] page = new object[5];
                using (Stream stream = File.Open(load.FileName, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    page = (object[])bin.Deserialize(stream);
                }
                CloseableTabItem newtab = new CloseableTabItem();
                newtab.SetHeader(new TextBlock { Text = ((Surface)page[0]).name });
                Frame frame = new Frame();
                switch (((Surface)page[0]).dimension)
                {
                    case 3:
                        frame.Content = new Page3D(((Surface)page[0]));
                        break;
                    case 4:
                        frame.Content = new Page4D(((Surface)page[0]), (double[][])page[1], (int)page[2], (int)page[3], (double)page[4]);
                        break;
                    default:
                        frame.Content = new PageND(((Surface)page[0]), ((Surface)page[0]).dimension);
                        break;
                }

                newtab.Content = frame;
                tabControl1.Items.Add(newtab);
                newtab.IsSelected = true;

            }
        }
        private void parallelPointsShownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender == parallelPointsShownMenuItemStandard)
            {
            }
            else if (sender == parallelPointsShownMenuItemNoTransposed)
            {
            }
            else if (sender == parallelPointsShownMenuItemOnlyTransposed)
            {
            }
            else if (sender == parallelPointsShownMenuItemNone)
            {
            }
            else //manual selection - Pi_ijk included in name, plus transposed or not as '
            {
                MenuItem selectedMenuItem = sender as MenuItem;
                if (selectedMenuItem.IsChecked)//dechecking
                {

                }
                else//checking
                {

                }
            }
            setUpParallelPointsShownMenuItems();
        }
        private void setUpParallelPointsShownMenuItems()
        {
            foreach (TabItem tab in tabControl1.Items)
            {
                if (tab.IsSelected)
                {
                    GraphicsPage currentPage = (GraphicsPage)(((Frame)tab.Content).Content);
                    //set up Pi_ijk and Pi_ijk' for each non-decreasing ijk trio 
                    for (int i = 0; i < currentPage.dimension; i++)
                        for (int j = i + 1; j < currentPage.dimension; j++)
                            for (int k = j + 1; k < currentPage.dimension; j++)
                            {
                                //note that "normal" people use notations starting with 1 for axes.
                                foreach (bool isTransposed in new bool[]{ false, true })
                                {
                                    MenuItem pi_ijk_MenuItem = new MenuItem();
                                    pi_ijk_MenuItem.Header = string.Format("Pi{3}_{0}{1}{2}", i, j, k,isTransposed ? "'" : "");
                                    pi_ijk_MenuItem.Name = pi_ijk_MenuItem.Header as string;
                                    pi_ijk_MenuItem.IsCheckable = true;
                                    pi_ijk_MenuItem.IsChecked = ??????;
                                    pi_ijk_MenuItem.Click += parallelPointsShownMenuItem_Click;
                                }
                            }
                    break;
                }
            }
        }
    }
}