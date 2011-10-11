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
    public class CommandPromptCommandLineUnitTests
    {
        [DebuggerStepThrough]
        private void AssertSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            Assert.IsTrue(actual.SequenceEqual(expected), "Expected: [" + string.Join(", ", expected) + "]; actual: [" + string.Join(", ", actual) + "]");
        }

        private const string Quote = "\"";

        [TestMethod]
        public void BlogExampleA()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "^", Quote + "^" + Quote }, ConsoleCommandLineLexer.Lex("^^ " + Quote + "^" + Quote));
        }

        [TestMethod]
        public void BlogExampleB()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { Quote + Quote }, ConsoleCommandLineLexer.Lex("^" + Quote + "^" + Quote));
        }

        [TestMethod]
        public void BlogExampleC()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { Quote + Quote }, ConsoleCommandLineLexer.Lex("^" + Quote + Quote));
        }

        [TestMethod]
        public void BlogExampleD()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { Quote + "^" + Quote + Quote }, ConsoleCommandLineLexer.Lex(Quote + "^" + Quote + "^" + Quote));
        }

        [TestMethod]
        public void BlogExampleE()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { Quote + "^" + Quote + "^" + Quote }, ConsoleCommandLineLexer.Lex(Quote + "^" + Quote + "^^" + Quote));
        }

        [TestMethod]
        public void BlogExampleF()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { Quote + "^^" }, ConsoleCommandLineLexer.Lex(Quote + "^^"));
        }
    }
}
