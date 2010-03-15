using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class ExceptionExtensionsUnitTests
    {
        [TestMethod]
        public void InnerExceptionsAndSelf_WithoutInnerExceptions_EnumeratesSelf()
        {
            Exception a = new Exception("a");
            Assert.IsTrue(a.InnerExceptionsAndSelf().SequenceEqual(new[] { a }), "InnerExceptionsAndSelf should enumerate itself even without inner exceptions");
        }

        [TestMethod]
        public void InnerExceptionsAndSelf_WithInnerExceptions_EnumeratesSelfAndInnerExceptions()
        {
            Exception a = new Exception("a");
            Exception b = new Exception("b", a);
            Assert.IsTrue(b.InnerExceptionsAndSelf().SequenceEqual(new[] { b, a }), "InnerExceptionsAndSelf should enumerate itself and its inner exceptions");
        }

        [TestMethod]
        public void ErrorMessage_WithSimpleMessage_JustReturnsSimpleMessage()
        {
            Exception a = new Exception("a");
            Assert.IsTrue(a.ErrorMessage(false).Contains("a"), "ErrorMessage should just return the simple error message");
        }

        [TestMethod]
        public void ErrorMessage_WithCRLFMessage_ReturnsFlattenedMessage()
        {
            Exception a = new Exception("a\r\nb");
            Assert.IsTrue(IsFlattened(a.ErrorMessage(false)), "ErrorMessage should return a flattened error message");
        }

        [TestMethod]
        public void ErrorMessage_WithEscapableMessage_ReturnsEscapedMessage()
        {
            Exception a = new Exception("a\tb");
            Assert.IsTrue(a.ErrorMessage(false).Contains("a\\tb"), "ErrorMessage should return an escaped error message");
        }

        [TestMethod]
        public void ErrorMessage_WithData_ReturnsData()
        {
            Exception a = new Exception("a");
            a.Data.Add("hi", "test");
            Assert.IsTrue(a.ErrorMessage(false).Contains("test"), "ErrorMessage should include Data information if present");
        }

        [TestMethod]
        public void ErrorMessage_WithoutStackTrace_DoesNotReturnStackTrace()
        {
            Exception a = new Exception("a");
            Assert.IsFalse(a.ErrorMessage(true).Contains("at"), "ErrorMessage should not include stack trace information if not present");
        }

        [TestMethod]
        public void ErrorMessage_WithoutStackTrace_DoesReturnStackTrace()
        {
            Exception a = null;
            try
            {
                throw new Exception("a");
            }
            catch (Exception ex)
            {
                a = ex;
            }

            Assert.IsTrue(a.ErrorMessage(true).Contains("at"), "ErrorMessage should include stack trace information if requested");
        }

        [TestMethod]
        public void Dump_WithInnerException_ReturnsBothMessages()
        {
            Exception a = new Exception("a");
            Exception b = new Exception("b", a);
            Assert.IsTrue(b.Dump(false).Contains(a.Message), "ErrorMessage should return the error message for outer exceptions");
            Assert.IsTrue(b.Dump(false).Contains(b.Message), "ErrorMessage should return all error messages for inner exceptions");
        }

        [TestMethod]
        public void Dump_WithEverything_IsFlattened()
        {
            Exception c = null;
            try
            {
                try
                {
                    try
                    {
                        throw new Exception("a");
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Add("adata", "testa");
                        throw new Exception("b", ex);
                    }
                }
                catch (Exception ex)
                {
                    ex.Data.Add("bdata", "testb");
                    throw new Exception("c", ex);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("cdata", "testc");
                c = ex;
            }

            string test = c.Dump(true);
            Assert.IsTrue(IsFlattened(test), "All dump messages should be flattened");
        }

        /// <summary>
        /// Whether a string is flattened.
        /// </summary>
        /// <param name="test">The string to test.</param>
        private static bool IsFlattened(string test)
        {
            if (test.Contains('\n') || test.Contains('\r'))
                return false;

            return true;
        }
    }
}
