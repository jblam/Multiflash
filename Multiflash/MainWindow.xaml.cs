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

        readonly IToolset toolset = new DummyToolset();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var set = System.Text.Json.JsonSerializer.Deserialize<BinarySet>(@"{""Name"":""Jeff"",""Binaries"":[{""Path"":""C:\\Path\\to\\binary.hex""}]}");
            foreach (var binary in set?.Binaries ?? throw new InvalidOperationException(nameof(set)))
            {
                await RunTool(binary);
            }
        }

        private async Task RunTool(Binary binary, string? workingDir = null)
        {
            var tool = toolset.GetToolForBinary(binary) ?? throw new InvalidOperationException("Couldn't get a tool");
            var s = tool.GetStartInfo(binary, ((MultiflashViewModel)DataContext).ComPortSelector.SelectedPort ?? throw new InvalidOperationException("Couldn't get the port"));
            s.RedirectStandardOutput = true;
            s.RedirectStandardError = true;
            s.RedirectStandardInput = true;
            s.CreateNoWindow = true;
            s.WorkingDirectory = workingDir ?? s.WorkingDirectory;
            var p = System.Diagnostics.Process.Start(s) ?? throw new InvalidOperationException("Couldn't get a process");
            var vm = new StreamingConsoleViewModel(p);
            console.DataContext = vm;
            await p.WaitForExitAsync();
            label1.Content = $@"Finished with exit code: {p.ExitCode}";
        }

        private void DockPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private async void DockPanel_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            if (data is not string[] paths)
            {
                throw new InvalidOperationException("Allowed drop which did not contain any files");
            }
            if (paths.Length != 1)
            {
                throw new NotSupportedException("Multiple files not supported");
            }
            var (location, contents) = await BinarySet.Extract(paths[0]);
            foreach (var item in contents?.Binaries ?? throw new InvalidOperationException("Unable to parse any binaries"))
            {
                await RunTool(item, location);
            }
            ;
        }
    }
}
