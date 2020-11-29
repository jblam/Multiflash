﻿using System;
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
            var toolset = new ArduinoToolset();

            var set = System.Text.Json.JsonSerializer.Deserialize<BinarySet>(@"{""Name"":""Jeff"",""Binaries"":[{""Path"":""C:\\Path\\to\\binary.hex""}]}");
            foreach (var binary in set?.Binaries ?? throw new InvalidOperationException(nameof(set)))
            {
                var tool = toolset.GetToolForBinary(binary) ?? throw new InvalidOperationException("Couldn't get a tool");
                var s = tool.GetStartInfo(binary, "COM5");
                s.RedirectStandardOutput = true;
                s.RedirectStandardError = true;
                s.RedirectStandardInput = true;
                s.CreateNoWindow = true;
                var p = System.Diagnostics.Process.Start(s) ?? throw new InvalidOperationException("Couldn't get a process");
                await p.WaitForExitAsync();
                label1.Content = $@"Finished with exit code: {p.ExitCode}

Output:
{await p.StandardOutput.ReadToEndAsync()}

Error:
{await p.StandardError.ReadToEndAsync()}";
            }
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
            var set = await BinarySet.Deserialise(paths[0]);
            ;
        }
    }
}
