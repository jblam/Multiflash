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
            Assert.AreEqual("123", actual);
        }

        [TestMethod]
        public void CanComposeEmbeddedBackspaces()
        {
            var original = "0";
            var withEmbedded = "1xx\b\b23";
            var actual = StringComposer.Compose(original, withEmbedded);
            Assert.AreEqual("0123", actual);
        }

        [TestMethod]
        public void ErasesFirstWhenLeadingCr()
        {
            var original = "x";
            var withCr = "\r123";
            var actual = StringComposer.Compose(original, withCr);
            Assert.AreEqual("123", actual);
        }
        [TestMethod]
        public void ErasesLeadingBeforeEmbeddedCr()
        {
            var original = "x";
            var withCr = "xxx\r123";
            var actual = StringComposer.Compose(original, withCr);
            Assert.AreEqual("123", actual);
        }

        [TestMethod]
        public void ErasesWithBackspaceAfterCr()
        {
            var first = "x";
            var second = "xx\rx\b\b123";
            var actual = StringComposer.Compose(first, second);
            Assert.AreEqual("123", actual);
        }
    }
}
