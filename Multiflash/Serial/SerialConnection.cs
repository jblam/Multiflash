using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Serial
{
    class SerialConnection : IDisposable
    {
        public static SerialConnection Open(string comPort, int baud, CancellationToken token = default)
        {
            var port = new SerialPort(comPort)
            {
                BaudRate = baud
            };
            port.Open();
            return new SerialConnection(port, token);
        }
        SerialConnection(SerialPort port, CancellationToken token)
        {
            this.port = port;
            this.token = token;
            var reader = new StreamReader(port.BaseStream, Encoding.UTF8);
            completion = ConsumeAsync(reader, Output, token);
        }

        static async ValueTask<int?> SafeReadAsync(TextReader reader, Memory<char> memory, CancellationToken token)
        {
            try
            {
                return await reader.ReadAsync(memory, token);
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }
        internal static async Task ConsumeAsync(TextReader reader, IList<string> fullLines, CancellationToken token)
        {
            var buffer = new char[1024].AsMemory();
            // on serial ports, reader.EndOfStream will never be true
            while (!token.IsCancellationRequested)
            {
                var length = await SafeReadAsync(reader, buffer, token).ConfigureAwait(false);
                if (!length.HasValue)
                    break;
                var section = buffer[..length.Value];
                while (section.Length > 0)
                {
                    var nextNewline = section.Span.IndexOf('\n');
                    var hasNewline = nextNewline >= 0;
                    var end = hasNewline ? nextNewline + 1 : section.Length;
                    var substring = section[0..end];
                    section = section[end..];
                    string yieldLine;
                    if (fullLines.Count == 0 || fullLines[^1].EndsWith('\n'))
                    {
                        yieldLine = substring.ToString();
                        fullLines.Add(yieldLine);
                    }
                    else
                    {
                        yieldLine = fullLines[^1] + substring;
                        fullLines[^1] = yieldLine;
                    }
                }
                await Task.Yield();
            }
        }

        readonly Task completion;
        readonly CancellationToken token;
        readonly SerialPort port;
        private bool disposedValue;

        /// <summary>
        /// Gets an observable collection of the output in lines, including incomplete
        /// lines.
        /// </summary>
        public ObservableCollection<string> Output { get; } = new();

        public Task<string> Prompt(string input, bool waitForCompleteLine = false)
        {
            var output = new TaskCompletionSource<string>();
            void h(object? sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewStartingIndex == Output.Count - 1)
                        {
                            if (Output[^1].EndsWith('\n') || !waitForCompleteLine)
                            {
                                output.SetResult(Output[^1]);
                                Output.CollectionChanged -= h;
                            }
                        }
                        else
                        {
                            Output.CollectionChanged -= h;
                            output.SetException(new InvalidOperationException("Serial response collection was mutated in an unexpected way"));
                        }
                        break;
                    default:
                        Output.CollectionChanged -= h;
                        output.SetException(new InvalidOperationException("Serial response collection was mutated in an unexpected way"));
                        break;
                }
            }
            Output.CollectionChanged += h;
            port.Write(input);
            return output.Task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    port.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
