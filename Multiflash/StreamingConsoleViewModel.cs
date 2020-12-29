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

        public StreamingConsoleViewModel(Binary b, ProcessStartInfo startInfo, int expectedExitCode = 0)
        {
            Binary = b;
            StartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));
            this.expectedExitCode = expectedExitCode;
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

        public int? ExitCode => Process is not null && Process.HasExited ? Process.ExitCode : null;
        public bool? IsSuccess => ExitCode.HasValue ? ExitCode == expectedExitCode : null;
        public bool IsRunning => !Process?.HasExited ?? false;

        public Binary Binary { get; }

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
