using JBlam.Multiflash.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

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
            var actual = StringComposer.ToLines(original, withBackspace);
            Assert.AreEqual("0123", actual.Single());
        }

        [TestMethod]
        public void CanComposeMultipleBackspaces()
        {
            var original = "0xx";
            var withBackspace = "\b\b123";
            var actual = StringComposer.ToLines(original, withBackspace);
            Assert.AreEqual("0123", actual.Single());
        }

        [TestMethod]
        public void CanComposeNoBackspaces()
        {
            var original = "0";
            var withBackspace = "123";
            var actual = StringComposer.ToLines(original, withBackspace);
            Assert.AreEqual("0123", actual.Single());
        }

        [TestMethod]
        public void CanComposeTooManyBackspaces()
        {
            var original = "x";
            var withBackspace = "\b\b123";
            var actual = StringComposer.ToLines(original, withBackspace);
            Assert.AreEqual("123", actual.Single());
        }

        [TestMethod]
        public void CanComposeEmbeddedBackspaces()
        {
            var original = "0";
            var withEmbedded = "1xx\b\b23";
            var actual = StringComposer.ToLines(original, withEmbedded);
            Assert.AreEqual("0123", actual.Single());
        }

        [TestMethod]
        public void ErasesFirstWhenLeadingCr()
        {
            var original = "x";
            var withCr = "\r123";
            var actual = StringComposer.ToLines(original, withCr);
            Assert.AreEqual("123", actual.Single());
        }
        [TestMethod]
        public void ErasesLeadingBeforeEmbeddedCr()
        {
            var original = "x";
            var withCr = "xxx\r123";
            var actual = StringComposer.ToLines(original, withCr);
            Assert.AreEqual("123", actual.Single());
        }

        [TestMethod]
        public void ErasesWithBackspaceAfterCr()
        {
            var first = "x";
            var second = "xx\rx\b\b123";
            var actual = StringComposer.ToLines(first, second);
            Assert.AreEqual("123", actual.Single());
        }

        [TestMethod]
        public void CannotBackspaceOverNewline()
        {
            var first = "a";
            var second = "bc\nde\b\b\b\bf";
            var actual = StringComposer.ToLines(first, second);
            CollectionAssert.AreEqual(new[] { "abc", "f" }, actual.ToList());
        }

        [TestMethod]
        public void CannotCarriageReturnOverNewline()
        {
            var first = "ab";
            var second = "c\nd\re";
            var actual = StringComposer.ToLines(first, second);
            CollectionAssert.AreEqual(new[] { "abc", "e" }, actual.ToList());
        }

        [TestMethod]
        public void DoesComposeMultipleCarriageReturns()
        {
            var first = "ab";
            var second = "c\nde\rf\rg";
            var actual = StringComposer.ToLines(first, second);
            CollectionAssert.AreEqual(new[] { "abc", "g" }, actual.ToList());
        }

        [TestMethod]
        public void DoesNotEraseForCrLf()
        {
            var first = "abc";
            var second = "def\r\nghi";
            var actual = StringComposer.ToLines(first, second);
            CollectionAssert.AreEqual(new[] { "abcdef", "ghi" }, actual.ToList());
        }

        [TestMethod]
        public void EmitsEmptyStringTrailingNewline()
        {
            var first = "";
            var second = "\n";
            CollectionAssert.AreEqual(new[] { "", "" }, StringComposer.ToLines(first, second).ToList());
        }
    }
}
