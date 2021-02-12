using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JBlam.Multiflash.App
{
    /// <summary>
    /// Interaction logic for StreamingConsole.xaml
    /// </summary>
    public partial class StreamingConsole : UserControl
    {
        public StreamingConsole()
        {
            InitializeComponent();
        }

        private void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            // JB 2020-12-28: the default presentation template for an expander's header
            // does not stretch to fit, for whatever reason. There's a variety of workarounds
            // described here: https://stackoverflow.com/q/31161591
            // and here: https://joshsmithonwpf.wordpress.com/2007/02/24/stretching-content-in-an-expander-header/
            ((FrameworkElement)((FrameworkElement)sender).TemplatedParent).HorizontalAlignment = HorizontalAlignment.Stretch;
        }
    }
}
