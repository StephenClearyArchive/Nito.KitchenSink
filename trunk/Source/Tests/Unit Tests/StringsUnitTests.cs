using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink.Text;

namespace Tests.Unit_Tests
{
    [TestClass]
    public class StringsUnitTests
    {
        [TestMethod]
        public void DamerauLevenshteinKnownValues()
        {
            Assert.AreEqual(0, Strings.DamerauLevenshteinDistance("test", "test"));
            Assert.AreEqual(1, Strings.DamerauLevenshteinDistance("to", "ot"));
            Assert.AreEqual(1, Strings.DamerauLevenshteinDistance("ot", "ost"));
            Assert.AreEqual(3, Strings.DamerauLevenshteinDistance("to", "ost"));
        }
    }
}
