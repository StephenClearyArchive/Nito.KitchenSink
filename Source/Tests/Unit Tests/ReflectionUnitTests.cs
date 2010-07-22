using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nito.KitchenSink.Reflection;

namespace Tests.Unit_Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ReflectionUnitTests
    {
        [TestMethod]
        public void UsesReferenceEquality_ReturnsExpectedValueForBCLTypes()
        {
            Assert.IsTrue(typeof(object).UsesReferenceEquality());

            Assert.IsFalse(typeof(Enum).UsesReferenceEquality());
            Assert.IsFalse(typeof(ValueType).UsesReferenceEquality());
            Assert.IsFalse(typeof(Delegate).UsesReferenceEquality());
            Assert.IsFalse(typeof(MulticastDelegate).UsesReferenceEquality());
        }

        struct ValueTypeWithoutExplicitEquals { }
        
        [TestMethod]
        public void ValueType_DoesNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(int).UsesReferenceEquality());
            Assert.IsFalse(typeof(Tuple<int>).UsesReferenceEquality());
            Assert.IsFalse(typeof(Tuple<>).UsesReferenceEquality());
            Assert.IsFalse(typeof(ValueTypeWithoutExplicitEquals).UsesReferenceEquality());
        }

        class BaseClassDefiningEquality
        {
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        class DerivedClassNotDefiningEquality : BaseClassDefiningEquality { }

        [TestMethod]
        public void ReferenceType_WithValueComparision_DoesNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(string).UsesReferenceEquality());
            Assert.IsFalse(typeof(DerivedClassNotDefiningEquality).UsesReferenceEquality());
        }

        [TestMethod]
        public void ReferenceType_WithoutValueComparision_UsesReferenceEquality()
        {
            Assert.IsTrue(typeof(Exception).UsesReferenceEquality());
            Assert.IsTrue(typeof(List<int>).UsesReferenceEquality());
            Assert.IsTrue(typeof(List<>).UsesReferenceEquality());
            Assert.IsTrue(typeof(ReflectionUnitTests).UsesReferenceEquality());
        }
        
        [TestMethod]
        public void Arrays_UseReferenceEquality()
        {
            Assert.IsTrue(typeof(Array).UsesReferenceEquality());
        }

        [TestMethod]
        public void Interfaces_DoNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(IEquatable<int>).UsesReferenceEquality());
        }

        [TestMethod]
        public void Pointers_DoNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(int).MakePointerType().UsesReferenceEquality());
        }

        [TestMethod]
        public void Enumerations_DoNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(AttributeTargets).UsesReferenceEquality());
        }

        [TestMethod]
        public void Delegates_DoNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(Action).UsesReferenceEquality());
        }
    }
}
