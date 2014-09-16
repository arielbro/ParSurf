using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
namespace ParSurf
{
    public class CloseableTabItem : TabItem
    {
        public string Value;
        public CloseableTabItem() : base()
        {
            this.SetResourceReference(StyleProperty, typeof(TabItem));
        }
        public void SetHeader(UIElement header)
        {
            // Container for header controls
            Value = ((TextBlock)header).Text;
            Console.WriteLine(Value);
            var dockPanel = new DockPanel();
            dockPanel.Children.Add(header);
            
            // Close button to remove the tab
            var closeButton = new TabCloseButton();
            closeButton.Click +=
                (sender, e) =>
                {
                    var tabControl = Parent as ItemsControl;
                    tabControl.Items.Remove(this);
                };
            dockPanel.Children.Add(closeButton);

            // Set the header
            Header = dockPanel;
        }
    }
}
