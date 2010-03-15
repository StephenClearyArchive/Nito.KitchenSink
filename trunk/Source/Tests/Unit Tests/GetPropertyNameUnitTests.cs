using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class GetPropertyNameUnitTests
    {
        [TestMethod]
        public void GetPropertyName_ForValueProperty_ReturnsPropertyName()
        {
            TestObject test = new TestObject();
            string propertyName = test.GetPropertyName(x => x.ValueProperty);
            Assert.AreEqual("ValueProperty", propertyName, "GetPropertyName should return property name for value properties");
        }

        [TestMethod]
        public void GetPropertyName_ForReferenceProperty_ReturnsPropertyName()
        {
            TestObject test = new TestObject();
            string propertyName = test.GetPropertyName(x => x.ObjectProperty);
            Assert.AreEqual("ObjectProperty", propertyName, "GetPropertyName should return property name for reference properties");
        }

        private sealed class TestObject
        {
            public int ValueProperty { get; set; }
            public object ObjectProperty { get; set; }
        }
    }
}

