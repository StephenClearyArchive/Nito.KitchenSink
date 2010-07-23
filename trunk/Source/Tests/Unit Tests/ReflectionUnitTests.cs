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
            Assert.IsTrue(typeof(object).IsReferenceEquatable());

            Assert.IsFalse(typeof(Enum).IsReferenceEquatable());
            Assert.IsFalse(typeof(ValueType).IsReferenceEquatable());
            Assert.IsFalse(typeof(Delegate).IsReferenceEquatable());
            Assert.IsFalse(typeof(MulticastDelegate).IsReferenceEquatable());
        }

        struct ValueTypeWithoutExplicitEquals { }
        
        [TestMethod]
        public void ValueType_DoesNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(int).IsReferenceEquatable());
            Assert.IsFalse(typeof(Tuple<int>).IsReferenceEquatable());
            Assert.IsFalse(typeof(Tuple<>).IsReferenceEquatable());
            Assert.IsFalse(typeof(ValueTypeWithoutExplicitEquals).IsReferenceEquatable());
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
            Assert.IsFalse(typeof(string).IsReferenceEquatable());
            Assert.IsFalse(typeof(DerivedClassNotDefiningEquality).IsReferenceEquatable());
        }

        [TestMethod]
        public void ReferenceType_WithoutValueComparision_UsesReferenceEquality()
        {
            Assert.IsTrue(typeof(Exception).IsReferenceEquatable());
            Assert.IsTrue(typeof(List<int>).IsReferenceEquatable());
            Assert.IsTrue(typeof(List<>).IsReferenceEquatable());
            Assert.IsTrue(typeof(ReflectionUnitTests).IsReferenceEquatable());
        }
        
        [TestMethod]
        public void Arrays_UseReferenceEquality()
        {
            Assert.IsTrue(typeof(Array).IsReferenceEquatable());
        }

        [TestMethod]
        public void Interfaces_DoNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(IEquatable<int>).IsReferenceEquatable());
        }

        [TestMethod]
        public void Pointers_DoNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(int).MakePointerType().IsReferenceEquatable());
        }

        [TestMethod]
        public void Enumerations_DoNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(AttributeTargets).IsReferenceEquatable());
        }

        [TestMethod]
        public void Delegates_DoNotUseReferenceEquality()
        {
            Assert.IsFalse(typeof(Action).IsReferenceEquatable());
        }
    }
}
