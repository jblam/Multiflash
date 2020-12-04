using JBlam.Multiflash.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBlam.Multiflash
{
    class StreamingDummyViewModel : INotifyPropertyChanged
    {
        public StreamingDummyViewModel()
        {
            Start();
        }
        const int Length = 4;
        static string Print(int i) => string.Join(
            null,
            "[".Concat(Enumerable.Repeat('\b', Length - i))
                .Concat(Enumerable.Repeat('#', i))
                .Concat(Enumerable.Repeat(' ', Length - i)
                .Concat("]")));
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<ConsoleOutput> Output { get; } = new();
        public async void Start()
        {
            Output.Add(new(OutputKind.StdOut, "Started"));
            Output.Add(new(default, "[" + string.Join(null, Enumerable.Repeat(' ', Length)) + "]"));
            for (int i = 0; i < Length; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                Output[^1] = new(default, Print(i));
            }
            Output[^1] = new(default, Print(Length));
            Output.Add(new(OutputKind.StdOut, "Finished"));
            ExitCode = 1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExitCode)));
        }

        public int? ExitCode { get; set; } = null;
        public int ExpectedExitCode { get; } = 7;
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
