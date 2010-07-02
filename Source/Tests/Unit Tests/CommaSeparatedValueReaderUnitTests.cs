using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Nito.KitchenSink;

namespace UnitTests
{
    using System.Diagnostics;

    [TestClass]
    public class CommaSeparatedValueReaderUnitTests
    {
        [TestMethod]
        public void CSV_WithUndefinedHeaders_ReadsEmptyStream()
        {
            var result = new CommaSeparatedValueParser(string.Empty, false).ReadDynamic().ToArray();
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void CSV_WithUserDefinedHeaders_ReadsEmptyStream()
        {
            var result = new CommaSeparatedValueParser(string.Empty, false, new[] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void CSV_WithUndefinedHeaders_WithSingleField_ReadsData()
        {
            var result = new CommaSeparatedValueParser("data", false).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].Property0);
        }

        [TestMethod]
        public void CSV_WithUserDefinedHeaders_WithSingleField_ReadsData()
        {
            var result = new CommaSeparatedValueParser("data", false, new[] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithCSVDefinedHeaders_WithSingleField_ReadsData()
        {
            var result = new CommaSeparatedValueParser("test\r\ndata").ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithCSVAndUserDefinedHeaders_WithSingleField_ReadsData()
        {
            var result = new CommaSeparatedValueParser("test\r\ndata", headers: new[] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithUndefinedHeaders_WithSingleEscapedField_ReadsData()
        {
            var result = new CommaSeparatedValueParser("\"data\"", false).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].Property0);
        }

        [TestMethod]
        public void CSV_WithUserDefinedHeaders_WithSingleEscapedField_ReadsData()
        {
            var result = new CommaSeparatedValueParser("\"data\"", false, new[] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithCSVDefinedHeaders_WithSingleEscapedField_ReadsData()
        {
            var result = new CommaSeparatedValueParser("test\r\n\"data\"").ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithCSVAndUserDefinedHeaders_WithSingleEscapedField_ReadsData()
        {
            var result = new CommaSeparatedValueParser("test\r\n\"data\"", headers: new[] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithMultipleRecords_ReadsData()
        {
            var result = new CommaSeparatedValueParser("test,Value\r\n\"data\",13\r\nother,15").ReadDynamic().ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("data", result[0].test);
            Assert.AreEqual("13", result[0].Value);
            Assert.AreEqual("other", result[1].test);
            Assert.AreEqual("15", result[1].Value);
        }

        [TestMethod]
        public void CSV_EndingWithCrLf_ReadsData()
        {
            var result = new CommaSeparatedValueParser("test,Value\r\n\"data\",13\r\nother,15\r\n").ReadDynamic().ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("data", result[0].test);
            Assert.AreEqual("13", result[0].Value);
            Assert.AreEqual("other", result[1].test);
            Assert.AreEqual("15", result[1].Value);
        }

        [TestMethod]
        public void CSV_WithEscapedValue_ReadsData()
        {
            var result = new CommaSeparatedValueParser("test,Value\r\n" + "\"da\"\"t,\r\na\",13\r\n" + "other,15\r\n").ReadDynamic().ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("da\"t,\r\na", result[0].test);
            Assert.AreEqual("13", result[0].Value);
            Assert.AreEqual("other", result[1].test);
            Assert.AreEqual("15", result[1].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CSV_RejectsInvalidCharacters()
        {
            var result = new CommaSeparatedValueParser("test,Value\r\n\"data\",13\r\not\rher,15\r\n").ReadDynamic().ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CSV_RejectsUnsquareData()
        {
            var result = new CommaSeparatedValueParser("test,Value\r\n\"data\",13\r\nother\r\n").ReadDynamic().ToArray();
        }
    }
}
