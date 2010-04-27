using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink.Dynamic;
using System.Security.Cryptography;

namespace Tests.Unit_Tests
{
    [TestClass]
    public class DynamicUnitTests
    {
        [TestMethod]
        public void Dynamic_InvokeStaticMethod_InvokesNormalMethod()
        {
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            dynamicClass.Dynamic_InvokeStaticMethod_InvokesNormalMethod_Method();
            Assert.IsTrue(Dynamic_InvokeStaticMethod_InvokesNormalMethod_MethidInvoked);
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesNormalMethod_MethidInvoked;
        public static void Dynamic_InvokeStaticMethod_InvokesNormalMethod_Method()
        {
            Dynamic_InvokeStaticMethod_InvokesNormalMethod_MethidInvoked = true;
        }


        [TestMethod]
        public void Dynamic_InvokeStaticMethod_InvokesMethodWithNullParameter()
        {
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            dynamicClass.Dynamic_InvokeStaticMethod_InvokesMethodWithNullParameter_Method(null);
            Assert.IsTrue(Dynamic_InvokeStaticMethod_InvokesMethodWithNullParameter_MethidInvoked);
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesMethodWithNullParameter_MethidInvoked;
        public static void Dynamic_InvokeStaticMethod_InvokesMethodWithNullParameter_Method(object parameter)
        {
            Dynamic_InvokeStaticMethod_InvokesMethodWithNullParameter_MethidInvoked = true;
        }


        [TestMethod]
        public void Dynamic_InvokeStaticMethod_InvokesOverloadedMethod()
        {
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            dynamicClass.Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_Method((long)13);
            Assert.IsFalse(Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_MethidInvokedWithInt);
            Assert.IsTrue(Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_MethidInvokedWithLong);
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_MethidInvokedWithInt;
        public static void Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_Method(int parameter)
        {
            Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_MethidInvokedWithInt = true;
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_MethidInvokedWithLong;
        public static void Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_Method(long parameter)
        {
            Dynamic_InvokeStaticMethod_InvokesOverloadedMethod_MethidInvokedWithLong = true;
        }


        [TestMethod]
        public void Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod()
        {
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            dynamicClass.Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_Method(string.Empty);
            Assert.IsFalse(Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_MethidInvokedWithTestMethodAttribute);
            Assert.IsTrue(Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_MethidInvokedWithString);
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_MethidInvokedWithString;
        public static void Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_Method(string parameter)
        {
            Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_MethidInvokedWithString = true;
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_MethidInvokedWithTestMethodAttribute;
        public static void Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_Method(TestMethodAttribute parameter)
        {
            Dynamic_InvokeStaticMethod_InvokesOverloadedObjectMethod_MethidInvokedWithTestMethodAttribute = true;
        }


        [TestMethod]
        public void Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull()
        {
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            dynamicClass.Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_Method(13, null);
            Assert.IsFalse(Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_MethidInvokedWithLong);
            Assert.IsTrue(Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_MethidInvokedWithInt);
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_MethidInvokedWithInt;
        public static void Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_Method(int parameter, object obj)
        {
            Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_MethidInvokedWithInt = true;
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_MethidInvokedWithLong;
        public static void Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_Method(long parameter, object obj)
        {
            Dynamic_InvokeStaticMethod_InvokesOverloadedMethodWithNull_MethidInvokedWithLong = true;
        }


        [TestMethod]
        public void Dynamic_InvokeStaticMethod_InvokesMethodWithOutParameter()
        {
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            int result = 17;
            var arg = RefOutArg.Create(result);
            dynamicClass.Dynamic_InvokeStaticMethod_InvokesMethodWithOutParameter_Method(arg);
            result = arg.Value;
            Assert.IsTrue(Dynamic_InvokeStaticMethod_InvokesMethodWithOutParameter_MethidInvoked);
            Assert.AreEqual(13, result);
        }
        private static bool Dynamic_InvokeStaticMethod_InvokesMethodWithOutParameter_MethidInvoked;
        public static void Dynamic_InvokeStaticMethod_InvokesMethodWithOutParameter_Method(out int parameter)
        {
            parameter = 13;
            Dynamic_InvokeStaticMethod_InvokesMethodWithOutParameter_MethidInvoked = true;
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Dynamic_InvokeStaticMethodWithOutParameter_WithoutRefOutArg_ThrowsArgumentException()
        {
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            int result = 17;
            dynamicClass.Dynamic_InvokeStaticMethodWithOutParameter_WithoutRefOutArg_ThrowsArgumentException_Method(out result);
        }
        public static void Dynamic_InvokeStaticMethodWithOutParameter_WithoutRefOutArg_ThrowsArgumentException_Method(out int parameter)
        {
            parameter = 13;
        }


        [TestMethod]
        public void Dynamic_ReadStaticProperty_ReadsProperty()
        {
            Dynamic_ReadStaticProperty_ReadsProperty_Property = 13;
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            int result = dynamicClass.Dynamic_ReadStaticProperty_ReadsProperty_Property;
            Assert.AreEqual(13, result);
        }
        public static int Dynamic_ReadStaticProperty_ReadsProperty_Property { get; set; }


        [TestMethod]
        public void Dynamic_WriteStaticProperty_WritesProperty()
        {
            var dynamicClass = DynamicStaticTypeMembers.Create(this.GetType());
            dynamicClass.Dynamic_WriteStaticProperty_WritesProperty_Property = 31;
            Assert.AreEqual(31, Dynamic_WriteStaticProperty_WritesProperty_Property);
        }
        public static int Dynamic_WriteStaticProperty_WritesProperty_Property { get; set; }
    }
}
