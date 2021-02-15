using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static SerialConnection Open(string comPort, int baud)
        {
            var port = new SerialPort(comPort)
            {
                BaudRate = baud
            };
            port.Open();
            return new SerialConnection(port);
        }
        SerialConnection(SerialPort port)
        {
            this.port = port;
            var reader = new StreamReader(port.BaseStream, Encoding.UTF8);
            Lines = ConsumeAsync(reader, Output, EnumeratorCancellationSource.Token);
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
        internal static async IAsyncEnumerable<string> ConsumeAsync(TextReader reader, IList<string> fullLines, [EnumeratorCancellation]CancellationToken token)
        {
            var buffer = new char[1024].AsMemory();
            // on serial ports, reader.EndOfStream will never be true
            while (!token.IsCancellationRequested)
            {
                var length = await SafeReadAsync(reader, buffer, token).ConfigureAwait(false);
                if (!length.HasValue)
                    yield break;
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
                    if (hasNewline)
                    {
                        yield return yieldLine;
                    }
                }
                await Task.Yield();
            }
        }

        readonly SerialPort port;
        private bool disposedValue;

        public IAsyncEnumerable<string> Lines { get; }

        /// <summary>
        /// Gets an observable collection of the output in lines, including incomplete
        /// lines.
        /// </summary>
        public ObservableCollection<string> Output { get; } = new();
        public CancellationTokenSource EnumeratorCancellationSource { get; } = new();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    port.Dispose();
                    EnumeratorCancellationSource.Cancel();
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
