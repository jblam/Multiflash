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

            // Task.Run is necessary because awaiting still blocks the UI thread.
            _ = Task.Run(() => Consume(process.StandardOutput, OutputKind.StdOut));
            _ = Task.Run(() => Consume(process.StandardError, OutputKind.StdErr));
            this.expectedExitCode = expectedExitCode;
        }

        async Task Consume(System.IO.StreamReader s, OutputKind kind)
        {
            var buffer = new char[1024];
            while (!s.EndOfStream)
            {
                // Note that this does block the active thread; we must Task.Run.
                var count = await s.ReadAsync(buffer).ConfigureAwait(false);
                AppendData(kind, new string(buffer.AsSpan(0, count)));
            }

            void AppendData(OutputKind kind, string data)
            {
                var splitData = StringComposer.Split(data);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // TODO: does/should data trail a newline char?
                    foreach (var line in splitData)
                    {
                        if (!Output.Any())
                        {
                            Output.Add(new(kind, line));
                            Debug.WriteLine($"Adding {kind}: {line} ({line.Length})");
                        }
                        else
                        {
                            var (lastkind, lastdata) = Output[^1];
                            if (lastkind != kind)
                            {
                                Output.Add(new(kind, line));
                                Debug.WriteLine($"Adding {kind}: {line} ({line.Length})");
                            }
                            else
                            {
                                Output[^1] = new(kind, StringComposer.Compose(lastdata, line));
                                Debug.WriteLine($"Composing {kind}: {line} ({line.Length})");
                            }
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
