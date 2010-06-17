using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class CommaSeparatedValueReaderUnitTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CSV_WithUndefinedHeaders_RejectsEmptyStream()
        {
            var csv = new StringReader(string.Empty);
            var result = new CommaSeparatedValueParser(csv, false).ReadDynamic();
            result.Any();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CSV_WithUserDefinedHeaders_RejectsEmptyStream()
        {
            var csv = new StringReader(string.Empty);
            var result = new CommaSeparatedValueParser(csv, false, new [] { "test" }).ReadDynamic();
            result.Any();
        }

        [TestMethod]
        public void CSV_WithUndefinedHeaders_WithSingleField_ReadsData()
        {
            var csv = new StringReader("data");
            var result = new CommaSeparatedValueParser(csv, false).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].Field0);
        }

        [TestMethod]
        public void CSV_WithUserDefinedHeaders_WithSingleField_ReadsData()
        {
            var csv = new StringReader("data");
            var result = new CommaSeparatedValueParser(csv, false, new [] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithCSVDefinedHeaders_WithSingleField_ReadsData()
        {
            var csv = new StringReader("test\r\ndata");
            var result = new CommaSeparatedValueParser(csv).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithCSVAndUserDefinedHeaders_WithSingleField_ReadsData()
        {
            var csv = new StringReader("test\r\ndata");
            var result = new CommaSeparatedValueParser(csv, headers:new [] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithUndefinedHeaders_WithSingleEscapedField_ReadsData()
        {
            var csv = new StringReader("\"data\"");
            var result = new CommaSeparatedValueParser(csv, false).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].Field0);
        }

        [TestMethod]
        public void CSV_WithUserDefinedHeaders_WithSingleEscapedField_ReadsData()
        {
            var csv = new StringReader("\"data\"");
            var result = new CommaSeparatedValueParser(csv, false, new[] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithCSVDefinedHeaders_WithSingleEscapedField_ReadsData()
        {
            var csv = new StringReader("test\r\n\"data\"");
            var result = new CommaSeparatedValueParser(csv).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithCSVAndUserDefinedHeaders_WithSingleEscapedField_ReadsData()
        {
            var csv = new StringReader("test\r\n\"data\"");
            var result = new CommaSeparatedValueParser(csv, headers: new[] { "test" }).ReadDynamic().ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("data", result[0].test);
        }

        [TestMethod]
        public void CSV_WithMultipleRecords_ReadsData()
        {
            var csv = new StringReader("test,Value\r\n\"data\",13\r\nother,15");
            var result = new CommaSeparatedValueParser(csv).ReadDynamic().ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("data", result[0].test);
            Assert.AreEqual("13", result[0].Value);
            Assert.AreEqual("other", result[1].test);
            Assert.AreEqual("15", result[1].Value);
        }

        [TestMethod]
        public void CSV_EndingWithCrLf_ReadsData()
        {
            var csv = new StringReader("test,Value\r\n\"data\",13\r\nother,15\r\n");
            var result = new CommaSeparatedValueParser(csv).ReadDynamic().ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("data", result[0].test);
            Assert.AreEqual("13", result[0].Value);
            Assert.AreEqual("other", result[1].test);
            Assert.AreEqual("15", result[1].Value);
        }

        [TestMethod]
        public void CSV_WithEscapedValue_ReadsData()
        {
            var csv = new StringReader("test,Value\r\n\"da\"\"t,\r\na\",13\r\nother,15\r\n");
            var result = new CommaSeparatedValueParser(csv).ReadDynamic().ToArray();
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
            var csv = new StringReader("test,Value\r\n\"data\",13\r\not\rher,15\r\n");
            var result = new CommaSeparatedValueParser(csv).ReadDynamic().ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CSV_RejectsUnsquareData()
        {
            var csv = new StringReader("test,Value\r\n\"data\",13\r\nother\r\n");
            var result = new CommaSeparatedValueParser(csv).ReadDynamic().ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CSV_RejectsInvalidEnding()
        {
            var csv = new StringReader("test,Value\r\n\"data\",13\r\nother,15\r");
            var result = new CommaSeparatedValueParser(csv).ReadDynamic().ToArray();
        }
    }
}
