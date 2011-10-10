using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink.OptionParsing;
using System.Diagnostics;

namespace UnitTests
{
    [TestClass]
    public class ConsoleCommandLineLexerUnitTests
    {
        [DebuggerStepThrough]
        private void AssertSequenceEqual<T>(IEnumerable<T> actual, IEnumerable<T> expected)
        {
            Assert.IsTrue(actual.SequenceEqual(expected));
        }

        [TestMethod]
        public void RegularParameter()
        {
            AssertSequenceEqual(new[] { "a" }, ConsoleCommandLineLexer.Lex("a"));
        }
    }
}
