using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class BooleanExtensionsUnitTests
    {
        [TestMethod]
        public void FalseBoolean_ConvertedToInt_Is0()
        {
            bool booleanValue = false;
            int intValue = booleanValue.ToInt32();
            Assert.AreEqual(0, intValue, "False boolean values should convert to 0");
        }

        [TestMethod]
        public void TrueBoolean_ConvertedToInt_Is1()
        {
            bool booleanValue = true;
            int intValue = booleanValue.ToInt32();
            Assert.AreEqual(1, intValue, "True boolean values should convert to 1");
        }

        [TestMethod]
        public void FalseNullableBoolean_ConvertedToInt_Is0()
        {
            bool? booleanValue = false;
            int? intValue = booleanValue.ToInt32();
            Assert.AreEqual(0, intValue.Value, "False boolean values should convert to 0");
        }

        [TestMethod]
        public void TrueNullableBoolean_ConvertedToInt_Is1()
        {
            bool? booleanValue = true;
            int? intValue = booleanValue.ToInt32();
            Assert.AreEqual(1, intValue.Value, "True boolean values should convert to 1");
        }

        [TestMethod]
        public void NullBoolean_ConvertedToInt_IsNull()
        {
            bool? booleanValue = null;
            int? intValue = booleanValue.ToInt32();
            Assert.IsFalse(intValue.HasValue, "Null boolean values should convert to null");
        }
    }
}
