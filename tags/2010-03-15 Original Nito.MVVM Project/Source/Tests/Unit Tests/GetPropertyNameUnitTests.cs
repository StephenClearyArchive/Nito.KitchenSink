using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.Utility;

namespace UnitTests
{
    [TestClass]
    public class GetPropertyNameUnitTests
    {
        [TestMethod]
        public void TestValueProperty()
        {
            TestObject test = new TestObject();

            Assert.AreEqual("ValueProperty", test.GetPropertyName(x => x.ValueProperty));
        }

        [TestMethod]
        public void TestObjectProperty()
        {
            TestObject test = new TestObject();

            Assert.AreEqual("ObjectProperty", test.GetPropertyName(x => x.ObjectProperty));
        }

        private sealed class TestObject
        {
            public int ValueProperty { get; set; }
            public object ObjectProperty { get; set; }
        }
    }
}
