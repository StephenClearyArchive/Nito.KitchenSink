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

        private const string Quote = "\"";
        private const string Backslash = "\\";

        [TestMethod]
        public void MsdnExampleA()
        {
            // http://msdn.microsoft.com/en-us/library/17w5ykft(v=VS.85).aspx
            AssertSequenceEqual(new[] { "abc", "d", "e" }, ConsoleCommandLineLexer.Lex(Quote + "abc" + Quote + " d e"));
        }

        [TestMethod]
        public void MsdnExampleB()
        {
            // http://msdn.microsoft.com/en-us/library/17w5ykft(v=VS.85).aspx
            AssertSequenceEqual(new[] { "a" + Backslash + Backslash + Backslash + "b", "de fg", "h" },
                ConsoleCommandLineLexer.Lex("a" + Backslash + Backslash + Backslash + "b d" + Quote + "e f" + Quote + "g h"));
        }

        [TestMethod]
        public void MsdnExampleC()
        {
            // http://msdn.microsoft.com/en-us/library/17w5ykft(v=VS.85).aspx
            AssertSequenceEqual(new[] { "a" + Backslash + Quote + "b", "c", "d" },
                ConsoleCommandLineLexer.Lex("a" + Backslash + Backslash + Backslash + Quote + "b c d"));
        }

        [TestMethod]
        public void MsdnExampleD()
        {
            // http://msdn.microsoft.com/en-us/library/17w5ykft(v=VS.85).aspx
            AssertSequenceEqual(new[] { "a" + Backslash + Backslash + "b c", "d", "e" },
                ConsoleCommandLineLexer.Lex("a" + Backslash + Backslash + Backslash + Backslash + Quote + "b c" + Quote + " d e"));
        }

        [TestMethod]
        public void BlogExampleA()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "a" }, ConsoleCommandLineLexer.Lex(Quote + "a" + Quote));
        }

        [TestMethod]
        public void BlogExampleB()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { Quote + "a" }, ConsoleCommandLineLexer.Lex(Backslash + Quote + "a" + Quote));
        }

        [TestMethod]
        public void BlogExampleC()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { Quote + "a" }, ConsoleCommandLineLexer.Lex(Backslash + Quote + "a"));
        }

        [TestMethod]
        public void BlogExampleD()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "a" + Quote }, ConsoleCommandLineLexer.Lex(Quote + "a" + Backslash + Quote));
        }

        [TestMethod]
        public void BlogExampleE()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "a" + Backslash }, ConsoleCommandLineLexer.Lex(Quote + "a" + Backslash + Backslash + Quote));
        }

        [TestMethod]
        public void BlogExampleF()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "a", Quote }, ConsoleCommandLineLexer.Lex("a " + Backslash + Quote));
        }

        [TestMethod]
        public void BlogExampleG()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "a " + Backslash }, ConsoleCommandLineLexer.Lex(Quote + "a " + Backslash + Backslash + Quote));
        }

        [TestMethod]
        public void BlogExampleH()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "a" + Quote + "b" }, ConsoleCommandLineLexer.Lex(Quote + "a" + Backslash + Quote + "b" + Quote));
        }

        [TestMethod]
        public void BlogExampleI()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "a" + Quote + "b" }, ConsoleCommandLineLexer.Lex(Quote + "a" + Quote + Quote + "b" + Quote));
        }

        [TestMethod]
        public void BlogExampleJ()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            AssertSequenceEqual(new[] { "a", string.Empty, Quote }, ConsoleCommandLineLexer.Lex("a " + Quote + Quote + " " + Quote + Quote + Quote + Quote));
        }
    }
}
