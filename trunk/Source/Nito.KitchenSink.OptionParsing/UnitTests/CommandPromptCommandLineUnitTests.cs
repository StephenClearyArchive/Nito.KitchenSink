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
        [TestMethod]
        public void BlogExampleA()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            Assert.AreEqual("^^ \"^\"", CommandPromptCommandLine.Escape("^ \"^\""));
        }

        [TestMethod]
        public void BlogExampleB()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            Assert.AreEqual("\"\"", CommandPromptCommandLine.Escape("\"\""));
        }

        [TestMethod]
        public void BlogExampleC()
        {
            // http://nitoprograms.blogspot.com/2011/06/option-parsing-lexing.html
            Assert.AreEqual("\"^\"^^\"", CommandPromptCommandLine.Escape("\"^\"^\""));
        }
    }
}
