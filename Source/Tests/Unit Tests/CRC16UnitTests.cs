using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;
using System.Security.Cryptography;

namespace Tests.Unit_Tests
{
    [TestClass]
    public class CRC16UnitTests
    {
        [TestMethod]
        public void CRC16_Default_ChecksumVerification()
        {
            var calculator = new CRC16();
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.AreEqual(0xBB3D, BitConverter.ToUInt16(result, 0));
        }

        [TestMethod]
        public void CRC16_CcittFalse_ChecksumVerification()
        {
            var calculator = new CRC16(CRC16.Definition.CcittFalse);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.AreEqual((ushort)0x29B1, BitConverter.ToUInt16(result, 0));
        }

        [TestMethod]
        public void CRC16_Ccitt_ChecksumVerification()
        {
            var calculator = new CRC16(CRC16.Definition.Ccitt);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.AreEqual((ushort)0x2189, BitConverter.ToUInt16(result, 0));
        }

        [TestMethod]
        public void CRC16_XModem_ChecksumVerification()
        {
            var calculator = new CRC16(CRC16.Definition.XModem);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.AreEqual((ushort)0x31C3, BitConverter.ToUInt16(result, 0));
        }

        [TestMethod]
        public void CRC16_X25_ChecksumVerification()
        {
            var calculator = new CRC16(CRC16.Definition.X25);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.AreEqual((ushort)0x906E, BitConverter.ToUInt16(result, 0));
        }
    }
}
