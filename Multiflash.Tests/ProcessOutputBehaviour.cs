using JBlam.Multiflash.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JBlam.Multiflash.Tests
{
    [TestClass]
    public class ProcessOutputBehaviour
    {
        [TestMethod]
        public void CanComposeBackspaceStrings()
        {
            var original = "0x";
            var withBackspace = "\b123";
            var actual = StringComposer.Compose(original, withBackspace);
            Assert.AreEqual("0123", actual);
        }

        [TestMethod]
        public void CanComposeMultipleBackspaces()
        {
            var original = "0xx";
            var withBackspace = "\b\b123";
            var actual = StringComposer.Compose(original, withBackspace);
            Assert.AreEqual("0123", actual);
        }

        [TestMethod]
        public void CanComposeNoBackspaces()
        {
            var original = "0";
            var withBackspace = "123";
            var actual = StringComposer.Compose(original, withBackspace);
            Assert.AreEqual("0123", actual);
        }

        [TestMethod]
        public void CanComposeTooManyBackspaces()
        {
            var original = "x";
            var withBackspace = "\b\b123";
            var actual = StringComposer.Compose(original, withBackspace);
            Assert.AreEqual("\b123", actual);
        }
    }
}
