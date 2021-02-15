using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JBlam.Multiflash.Tests
{
    [TestClass]
    public class SerialRedirectionBehaviour
    {
        class DebugReader : TextReader
        {
            public void Add(string s)
            {
                if (tcs != null) {
                    tcs.SetResult(s);
                    tcs = null;
                        }
                else
                {
                    strings.Enqueue(s);
                }
            }
            TaskCompletionSource<string> tcs = null;
            readonly Queue<string> strings = new();
            public override async ValueTask<int> ReadAsync(Memory<char> memory, CancellationToken token)
            {
                if (tcs != null)
                    throw new InvalidOperationException("Concurrent reads are not supported by the debug stream");
                if (!strings.TryDequeue(out string output))
                {
                    token.Register(() => tcs.TrySetCanceled(token));
                    tcs = new TaskCompletionSource<string>();
                    output = await tcs.Task.ConfigureAwait(false);
                }
                output.AsMemory().CopyTo(memory);
                return output.Length;
            }
        }

        [TestMethod]
        public async Task CanYieldReturnLines()
        {
            var reader = new DebugReader();
            var list = new List<string>();

            CancellationTokenSource cancellationTokenSource = new();

            var e = Serial.SerialConnection.ConsumeAsync(reader, list, cancellationTokenSource.Token);
            await Task.Yield();
            reader.Add("Line one\n");
            reader.Add("Line ");
            reader.Add("two\n");
            cancellationTokenSource.CancelAfter(10);
            var result = await e.ToListAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            var expected = new[] { "Line one\n", "Line two\n" };
            CollectionAssert.AreEqual(expected, list);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public async Task IncompleteLineIsObservable()
        {
            var reader = new DebugReader();
            var list = new List<string>();
            _ = Serial.SerialConnection.ConsumeAsync(reader, list, default).LastAsync().AsTask();
            await Task.Yield();
            reader.Add("Line one\n");
            reader.Add("Line ");
            await Task.Delay(10);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Line ", list[1]);
        }

        [TestMethod, Timeout(8000)]
        public async Task ListensOnSerial()
        {
            using var c = Serial.SerialConnection.Open("COM4", 115200);
            c.EnumeratorCancellationSource.CancelAfter(TimeSpan.FromSeconds(1));
            var mac = await c.Lines.FirstAsync(c => c.StartsWith("Hub"), c.EnumeratorCancellationSource.Token);
            const string expected = "24-62-AB-E4-0F-48";
            Assert.AreEqual(expected, mac.Split(new[] { ' ', '\r', '\n' })[1]);
            
        }
    }
}
