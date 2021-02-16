using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace JBlam.Multiflash.Serial
{
    class SerialConnection : IDisposable
    {
        public static SerialConnection Open(string comPort, int baud, CancellationToken token = default) =>
            Open(comPort, baud, Encoding.UTF8, token);
        public static SerialConnection Open(string comPort, int baud, Encoding encoding, CancellationToken token = default)
        {
            var port = new SerialPort(comPort, baud)
            {
                Encoding = encoding,
            };
            port.Open();
            return new SerialConnection(port, token);
        }
        SerialConnection(SerialPort port, CancellationToken token)
        {
            this.port = port;
            this.token = token;
            observationDispatcher = Dispatcher.CurrentDispatcher;
            port.DataReceived += Port_DataReceived;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                var portString = port.ReadExisting();
                observationDispatcher.Invoke(() =>
                {
                    var data = portString.AsSpan();
                    while (data.Length > 0)
                    {
                        var message = Message.ConsumeIncoming(ref data, sw.Elapsed);
                        var previous = Output.Count > 0 ? Output[^1] : new Message { IsTerminated = true };
                        var combined = previous.TryCombine(message);
                        if (combined.HasValue)
                        {
                            Output[^1] = combined.Value;
                        }
                        else
                        {
                            Output.Add(message);
                        }
                    }
                });
            }
        }

        readonly Dispatcher observationDispatcher;
        readonly Stopwatch sw = Stopwatch.StartNew();
        readonly CancellationToken token;
        readonly SerialPort port;
        private bool disposedValue;

        /// <summary>
        /// Gets an observable collection of the output in lines, including incomplete
        /// lines.
        /// </summary>
        public ObservableCollection<Message> Output { get; } = new();

        public Task<string> Prompt(string input, bool waitForCompleteLine = false)
        {
            Output.Add(Message.CreateOutgoing(input, sw.Elapsed));
            var output = new TaskCompletionSource<string>();
            void h(object? sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewStartingIndex == Output.Count - 1)
                        {
                            var message = Output[^1];
                            if (message.Direction != MessageDirection.FromRemote)
                                return;
                            if (waitForCompleteLine && !message.IsTerminated)
                                return;
                            output.SetResult(Output[^1].Content);
                            Output.CollectionChanged -= h;
                        }
                        else
                        {
                            Output.CollectionChanged -= h;
                            output.SetException(new InvalidOperationException("Serial response collection was mutated at an unexpected index"));
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
