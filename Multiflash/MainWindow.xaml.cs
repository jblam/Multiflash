using System;
using System.Collections.Generic;
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

namespace JBlam.Multiflash
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var s = new EspUploaderPyTool().GetStartInfo(new Binary(BinaryFormat.Bin, "C:\\Path\\to\\binary.hex", 0x300000), "COM5");
            s.RedirectStandardOutput = true;
            s.RedirectStandardError = true;
            s.RedirectStandardInput = true;
            s.CreateNoWindow = true;
            var p = System.Diagnostics.Process.Start(s);
            await p.WaitForExitAsync();
            label1.Content = $@"Finished with exit code: {p.ExitCode}
Output:
{await p.StandardOutput.ReadToEndAsync()}

Error:
{await p.StandardError.ReadToEndAsync()}";
        }
    }
}
