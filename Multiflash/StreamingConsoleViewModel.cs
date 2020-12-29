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
            var allSpans = new List<string>();
            while (!s.EndOfStream)
            {
                // Note that this does block the active thread; we must Task.Run.
                var count = await s.ReadAsync(buffer).ConfigureAwait(false);
                AppendData(kind, new string(buffer.AsSpan(0, count)));
            }

            void AppendData(OutputKind kind, string data)
            {
                allSpans.Add(data);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var shouldReplace = Output.Any() && Output[^1].Kind == kind;
                    var lines = StringComposer.ToLines(shouldReplace ? Output[^1].Data : "", data);
                    foreach (var line in lines)
                    {
                        if (shouldReplace)
                        {
                            Output[^1] = new(kind, line);
                            shouldReplace = false;
                        }
                        else
                        {
                            Output.Add(new(kind, line));
                        }
                    }
                });
            }
        }

        public Process Process { get; }
        public ObservableCollection<ConsoleOutput> Output { get; } = new();

        public int? ExitCode => Process.HasExited ? Process.ExitCode : null;
        public bool? IsSuccess => ExitCode.HasValue ? ExitCode == expectedExitCode : null;
        public bool IsRunning => !Process.HasExited;

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
