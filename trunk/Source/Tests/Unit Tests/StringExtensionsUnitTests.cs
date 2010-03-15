using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;
using Nito.Linq;

namespace UnitTests
{
    [TestClass]
    public class StringExtensionsUnitTests
    {
        [TestMethod]
        public void Join_OnEmptySequence_ReturnsEmptyString()
        {
            string result = new List<string>().Join(",");
            Assert.AreEqual(string.Empty, result, "Join on an empty string sequence should result in an empty string");
        }

        [TestMethod]
        public void Join_OnSingleSequence_DoesNotUseSeparator()
        {
            string result = new List<string> { "test" }.Join(",");
            Assert.AreEqual("test", result, "Join on a string sequence of a single string should result in just that string");
        }

        [TestMethod]
        public void Join_OnSequence_UsesSeparator()
        {
            string result = new List<string> { "test1", "test2" }.Join(", ");
            Assert.AreEqual("test1, test2", result, "Join on a string sequence should use the separator string correctly");
        }

        [TestMethod]
        public void Join_WithourSeparator_JustConcatenates()
        {
            string result = new List<string> { "test1", "test2" }.Join();
            Assert.AreEqual("test1test2", result, "Join on a string sequence should concatenate in the absence of a separator string");
        }

        [TestMethod]
        public void PrettyDump_WithASCIIChars_ResultsInString()
        {
            string result = new byte[] { 72, 101, 108, 108, 111 }.PrettyDump();
            Assert.AreEqual("\"Hello\"", result, "Simple ASCII byte array should pretty dump to a string");
        }

        [TestMethod]
        public void PrettyDump_AsString_AllowsCRLFTab()
        {
            string result = new byte[] { 72, 9, 101, 108, 108, 111, 13, 10 }.PrettyDump();
            Assert.AreEqual("\"H\\tello\\r\\n\"", result, "ASCII byte array with CR, LF, or TAB should pretty dump to a string");
        }

        [TestMethod]
        public void PrettyDump_WithNonASCIIChars_RevertsToBinary()
        {
            string result = new byte[] { 72, 9, 101, 108, 108, 111, 13, 11 }.PrettyDump();
            Assert.AreEqual("[48 09 65 6C 6C 6F 0D 0B]", result, "Binary byte array should pretty dump to a hex string");
        }

        [TestMethod]
        public void PrettyDump_SupportsEmptySequences()
        {
            string result = new byte[] { 72, 9, 101, 11, 108, 108, 111, 13, 10 }.Slice(0, 0).PrettyDump();
            Assert.AreEqual("\"\"", result, "Empty array range should pretty dump to an empty string");
        }

        [TestMethod]
        public void PrintableEscape_WithNormalText_DoesNotEscape()
        {
            string result = "Hello".PrintableEscape();
            Assert.AreEqual("Hello", result, "PrintableEscape should not mangle simple strings");
        }

        [TestMethod]
        public void PrintableEscape_WithEscapableText_WillEscape()
        {
            string result = "Hello\'\"\a\0\\\b\f\n\r\t\v".PrintableEscape();
            Assert.AreEqual("Hello\\'\\\"\\a\\0\\\\\\b\\f\\n\\r\\t\\v", result, "PrintableEscape should escape if necessary");
        }

        [TestMethod]
        public void PrintableEscape_WithSurrogatePair_WillUnicodeEscape()
        {
            string result = "\U0001D11E".PrintableEscape();
            Assert.AreEqual("\\uD834\\uDD1E", result, "PrintableEscape should Unicode-escape surrogate characters");
        }

        [TestMethod]
        public void PrintableEscape_WithCombiningCharacters_WillUnicodeEscape()
        {
            string result = "a\u0304\u0308".PrintableEscape();
            Assert.AreEqual("\\u0061\\u0304\\u0308", result, "PrintableEscape should Unicode-escape combining characters");
        }

        [TestMethod]
        public void Printable_WithNormalText_WillNotEscape()
        {
            string result = "Hello".Printable();
            Assert.AreEqual("Hello", result, "Printable should not mangle simple strings");
        }

        [TestMethod]
        public void Printable_WithEscapableText_WillNotEscape()
        {
            string result = "Hello, \"John\"!".Printable();
            Assert.AreEqual("Hello, \"John\"!", result, "Printable should not mangle simple strings with escapable characters");
        }

        [TestMethod]
        public void Printable_WithUnprintableText_WillEscape()
        {
            string result = "Hello, \"John\"!\n".Printable();
            Assert.AreEqual("Hello, \\\"John\\\"!\\n", result, "Printable should escape if necessary");
        }

        [TestMethod]
        public void Flatten_WithNormalText_WillNotModify()
        {
            string result = "Hello".Flatten();
            Assert.AreEqual("Hello", result, "Flatten should not mangle simple strings");
        }

        [TestMethod]
        public void Flatten_WithCRLFText_WillReplaceCRLF()
        {
            string result = "Hello\r\nDave".Flatten();
            Assert.AreEqual("Hello  Dave", result, "Flatten should flatten CRLF");
        }
    }
}
