using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class GuidExtensionsUnitTests
    {
        [TestMethod]
        public void Test_KnownGUID_A()
        {
            // This GUID was generated on 2010-02-24 on my machine using "uuidgen -x"
            Guid test = new Guid("6478c9bd-2157-11df-8f70-001c234418a3");
            Assert.AreEqual(GuidVariant.RFC4122, test.GetVariant());
            Assert.AreEqual(GuidVersion.TimeBased, test.GetVersion());
            Assert.IsTrue(test.NodeIsMAC());
            Assert.IsTrue(test.GetNode().SequenceEqual(new byte[] { 0x00, 0x1C, 0x23, 0x44, 0x18, 0xA3 })); // This MAC address was retrieved using "ipconfig /all"
            Assert.IsTrue(test.GetCreateTime().Date == new DateTime(2010, 02, 24));
        }

        [TestMethod]
        public void Test_KnownGUID_B()
        {
            // This GUID was generated on 2010-02-24 on my machine using "uuidgen -x"
            Guid test = new Guid("661a0750-2157-11df-8f70-001c234418a3");
            Assert.AreEqual(GuidVariant.RFC4122, test.GetVariant());
            Assert.AreEqual(GuidVersion.TimeBased, test.GetVersion());
            Assert.IsTrue(test.NodeIsMAC());
            Assert.IsTrue(test.GetNode().SequenceEqual(new byte[] { 0x00, 0x1C, 0x23, 0x44, 0x18, 0xA3 })); // This MAC address was retrieved using "ipconfig /all"
            Assert.IsTrue(test.GetCreateTime().Date == new DateTime(2010, 02, 24));
        }

        [TestMethod]
        public void Test_KnownGUID_C()
        {
            // This GUID was generated on 2010-02-24 on my machine using "uuidgen"
            Guid test = new Guid("87ce17d1-7d63-42e5-aafb-4e105942fbf7");
            Assert.AreEqual(GuidVariant.RFC4122, test.GetVariant());
            Assert.AreEqual(GuidVersion.Random, test.GetVersion());
        }
    }
}
