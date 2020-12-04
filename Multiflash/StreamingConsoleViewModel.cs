using JBlam.Multiflash.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace JBlam.Multiflash
{
    public class StreamingConsoleViewModel : INotifyPropertyChanged
    {
        private readonly int expectedExitCode;

        public StreamingConsoleViewModel(Process process, int expectedExitCode = 0)
        {
            Process = process ?? throw new ArgumentNullException(nameof(process));
            process.Exited += (sender, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            process.ErrorDataReceived += (sender, e) => Process_DataReceived(OutputKind.StdErr, e.Data);
            process.OutputDataReceived += (sender, e) => Process_DataReceived(OutputKind.StdOut, e.Data);
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            this.expectedExitCode = expectedExitCode;
        }

        private void Process_DataReceived(OutputKind kind, string? data)
        {
            if (data is not null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!Output.Any() || !data.StartsWith('\b'))
                    {
                        Output.Add(new(kind, data));
                    }
                    else
                    {
                        var (lastkind, lastdata) = Output[^1];
                        if (lastkind != kind)
                        {
                            Output.Add(new(kind, data));
                        }
                        else
                        {
                            Output[^1] = new(kind, StringComposer.Compose(lastdata, data));
                        }
                    }
                });
            }
        }

        public Process Process { get; }
        public ObservableCollection<ConsoleOutput> Output { get; } = new();

        public int? ExitCode => Process.HasExited ? null : Process.ExitCode;
        public bool? IsSuccess => ExitCode == expectedExitCode;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public enum OutputKind
    {
        StdOut,
        StdErr
    }
    public struct ConsoleOutput
    {
        public ConsoleOutput(OutputKind kind, string data)
        {
            Kind = kind;
            Data = data;
        }

        public OutputKind Kind { get; }
        public string Data { get; }

        public void Deconstruct(out OutputKind kind, out string data)
        {
            kind = Kind;
            data = Data;
        }

        public override string ToString() => Data;
    }
}
