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
using System.Windows.Input;
using System.Windows.Threading;

namespace JBlam.Multiflash.App
{
    public class StreamingConsoleViewModel : INotifyPropertyChanged
    {
        // TODO: determine if we have any tool that returns a nonzero exit code on success.
        private readonly int expectedExitCode;

        // TODO: verbose tools cause the console window to overflow.
        // Even though the console view is presented inside a scroll viewer, we need to convince
        // WPF to lock its size at least to the height of the display.

        public StreamingConsoleViewModel(string name, ProcessStartInfo startInfo, int expectedExitCode = 0)
        {
            Name = name;
            StartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));
            this.expectedExitCode = expectedExitCode;
            CopyText = Command.Create(() => Clipboard.SetText(string.Join(Environment.NewLine, Output.Select(o => o.Data))));
        }

        public Process Start()
        {
            Process = Process.Start(StartInfo);
            if (Process is not null)
            {
                Process.Exited += (sender, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

                // Task.Run is necessary because awaiting still blocks the UI thread.
                _ = Task.Run(() => Consume(Process.StandardOutput, OutputKind.StdOut));
                _ = Task.Run(() => Consume(Process.StandardError, OutputKind.StdErr));
            }
            else
            {
                throw new InvalidOperationException("Process failed to start");
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            return Process;
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

        public ProcessStartInfo StartInfo { get; }
        Process? Process { get; set; }
        public ObservableCollection<ConsoleOutput> Output { get; } = new();
        public ICommand CopyText { get; }

        public int? ExitCode => Process is not null && Process.HasExited ? Process.ExitCode : null;
        public bool? IsSuccess => ExitCode.HasValue ? ExitCode == expectedExitCode : null;
        public bool IsRunning => !Process?.HasExited ?? false;
        public bool IsStarted => Process is not null;

        public string Name { get; }

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
