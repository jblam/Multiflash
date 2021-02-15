using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Serial
{
    class SerialConnection
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
        internal static async Task<int?> TryReadAsync(StreamReader reader, Memory<char> buffer, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return null;
            try
            {
                return await reader.ReadAsync(buffer, token);
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }
        internal static async IAsyncEnumerable<string> ConsumeAsync(StreamReader reader, IList<string> fullLines, [EnumeratorCancellation]CancellationToken token)
        {
            var buffer = new char[1024].AsMemory();
            // TODO: how to detect end of stream then?
            while (true)
            {
                await Task.Yield();
                var length = await TryReadAsync(reader, buffer, token).ConfigureAwait(false);
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
            }
        }

        readonly SerialPort port;

        public IAsyncEnumerable<string> Lines { get; }

        /// <summary>
        /// Gets an observable collection of the output in lines, including incomplete
        /// lines.
        /// </summary>
        public ObservableCollection<string> Output { get; } = new();
        public CancellationTokenSource EnumeratorCancellationSource { get; } = new();

    }
}
