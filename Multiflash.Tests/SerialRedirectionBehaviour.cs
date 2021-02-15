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
        [TestMethod]
        public async Task CanYieldReturnLines()
        {
            var stream = new MemoryStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);
            var list = new List<string>();
            void Append(string s)
            {
                var oldPosition = stream.Position;
                writer.Write(s);
                writer.Flush();
                stream.Position = oldPosition;
            }

            CancellationTokenSource cancellationTokenSource = new();

            var e = Serial.SerialConnection.ConsumeAsync(reader, list, cancellationTokenSource.Token);
            var outputTask = e.ToListAsync();
            Append("Line one\n");
            Append("Line ");
            Append("two\n");
            await Task.Yield();
            cancellationTokenSource.Cancel();
            writer.Close();
            var result = await outputTask;
#pragma warning disable CA2012 // Use ValueTasks correctly
            // Justification: I am *literally* doing as the hint-tip says, and asserting `IsCompleted`.
#pragma warning restore CA2012 // Use ValueTasks correctly
            //Assert.IsTrue(outputTask.IsCompleted);
            //CollectionAssert.AreEqual(new[] { "Line one\n", "Line two\n" }, outputTask.Result);
        }
    }
}
