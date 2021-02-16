using JBlam.Multiflash.Serial;
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
        [TestMethod, Timeout(8000), Ignore("Requires an actual device")]
        public async Task ListensOnSerial()
        {
            using var c = Serial.SerialConnection.Open("COM4", 115200);
            await Task.Delay(5000);
            Assert.AreNotEqual(0, c.Output.Count);
        }
        [TestMethod, Timeout(8000), Ignore("Requires an actual device")]
        public async Task SendsCommandsOnSerial()
        {
            using var c = Serial.SerialConnection.Open("COM4", 115200);
            var output = await c.Prompt("REGISTER", true);
            await Task.Delay(100);
            Assert.AreEqual("", output);
        }

        [TestMethod]
        public void GetsFullMessage()
        {
            var completeSerialData = "asdf\r\n".AsSpan();
            var message = Message.ConsumeIncoming(ref completeSerialData, default);
            Assert.AreEqual(0, completeSerialData.Length);
            Assert.AreEqual("asdf", message.Content);
            Assert.IsTrue(message.IsTerminated);
        }

        [TestMethod]
        public void GetsNonterminatedMessage()
        {
            var completeSerialData = "asdf".AsSpan();
            var message = Message.ConsumeIncoming(ref completeSerialData, default);
            Assert.AreEqual(0, completeSerialData.Length);
            Assert.AreEqual("asdf", message.Content);
            Assert.IsFalse(message.IsTerminated);
        }

        [TestMethod]
        public void GetsPartMessage()
        {
            var completeSerialData = "asdf\r\n1234".AsSpan();
            var message = Message.ConsumeIncoming(ref completeSerialData, default);
            Assert.AreEqual("1234", completeSerialData.ToString());
            Assert.AreEqual("asdf", message.Content);
            Assert.IsTrue(message.IsTerminated);
        }

        [TestMethod]
        public void ConcatenatesIncompleteMessage()
        {
            var first = new Message
            {
                Content = "1234",
                IsTerminated = false,
                Direction = MessageDirection.FromRemote,
                StartedAt = default
            };
            var combined = first.TryCombine(new Message { Content = "5678", Direction = MessageDirection.FromRemote });
            Assert.IsNotNull(combined);
            Assert.AreEqual(combined.Value.Content, "12345678");
            Assert.IsFalse(combined.Value.IsTerminated);
        }

        [TestMethod]
        public void DoesNotConcatenateCompleteMessage()
        {
            var first = new Message
            {
                Content = "1234",
                IsTerminated = true,
                Direction = MessageDirection.FromRemote
            };
            var combined = first.TryCombine(new Message { Content = "5678", Direction = MessageDirection.FromRemote });
            Assert.IsNull(combined);
        }
    }
}
