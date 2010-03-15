using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink.Communication;

namespace UnitTests
{
    [TestClass]
    public class SingleByteDelimiterFramerUnitTests
    {
        [TestMethod]
        [ExpectedException(typeof(System.Net.ProtocolViolationException), "Delimiter should reject all out-of-band communications")]
        public void FirstMessage_NotStartingWithStartDelimiter_IsRejected()
        {
            var framer = CreateFramer();
            framer.DataReceived(new byte[] { 0x7, 0x13 });
        }

        [TestMethod]
        public void CompleteMessage_IsAccepted()
        {
            var framer = CreateFramer();
            bool invokedMessageReceived = false;
            framer.MessageArrived += (x) => invokedMessageReceived = true;
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', 0x7 });
            Assert.IsTrue(invokedMessageReceived, "Delimiter should accept simple messages");
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.ProtocolViolationException), "Delimiter should reject all out-of-band communications")]
        public void SecondMessage_NotStartingWithStartDelimiter_IsRejected()
        {
            var framer = CreateFramer();
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', 0x7, 0x7 });
        }

        [TestMethod]
        public void MultipleMessages_AreAllAccepted()
        {
            var framer = CreateFramer();
            int invokedMessageReceived = 0;
            framer.MessageArrived += (x) => ++invokedMessageReceived;
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', 0x7, 0x13, (byte)'h', (byte)'o', 0x7 });
            Assert.AreEqual(2, invokedMessageReceived, "Delimiter should accept multiple messages");
        }

        [TestMethod]
        public void Reset_AfterPartialMessage_ResetsState()
        {
            var framer = CreateFramer();
            bool invokedMessageReceived = false;
            framer.MessageArrived += (x) => invokedMessageReceived = (Encoding.ASCII.GetString(x.ToArray()) == "hi");
            framer.DataReceived(new byte[] { 0x13, (byte)'h' });
            framer.Reset();
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', 0x7 });
            Assert.IsTrue(invokedMessageReceived, "Delimiter should allow resetting");
        }

        [TestMethod]
        public void PartialMessage_IsBuffered()
        {
            var framer = CreateFramer();
            bool invokedMessageReceived = false;
            framer.MessageArrived += (x) => invokedMessageReceived = (Encoding.ASCII.GetString(x.ToArray()) == "hi");
            framer.DataReceived(new byte[] { 0x13, (byte)'h' });
            framer.DataReceived(new byte[] { (byte)'i', 0x7 });
            Assert.IsTrue(invokedMessageReceived, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void PartialMessage_AfterStartDelimiter_IsBuffered()
        {
            var framer = CreateFramer();
            bool invokedMessageReceived = false;
            framer.MessageArrived += (x) => invokedMessageReceived = (Encoding.ASCII.GetString(x.ToArray()) == "hi");
            framer.DataReceived(new byte[] { 0x13 });
            framer.DataReceived(new byte[] { (byte)'h', (byte)'i', 0x7 });
            Assert.IsTrue(invokedMessageReceived, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void PartialMessage_BeforeEndDelimiter_IsBuffered()
        {
            var framer = CreateFramer();
            bool invokedMessageReceived = false;
            framer.MessageArrived += (x) => invokedMessageReceived = (Encoding.ASCII.GetString(x.ToArray()) == "hi");
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i' });
            framer.DataReceived(new byte[] { 0x7 });
            Assert.IsTrue(invokedMessageReceived, "Delimiter should accept split messages");
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.ProtocolViolationException), "Delimiter should reject messages that are too large")]
        public void OversizeMessage_IsRejected()
        {
            var framer = CreateSmallFramer();
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', (byte)'d', 0x7 });
        }

        [TestMethod]
        public void MultipleMessages_SplitAfterStartDelimiter_AreAllAccepted()
        {
            var framer = CreateFramer();
            int invokedMessageReceived = 0;
            framer.MessageArrived += (x) => ++invokedMessageReceived;
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', 0x7, 0x13 });
            framer.DataReceived(new byte[] { (byte)'h', (byte)'o', 0x7 });
            Assert.AreEqual(2, invokedMessageReceived, "Delimiter should accept multiple messages when split");
        }

        [TestMethod]
        public void MultipleMessages_SplitMidMessage_AreAllAccepted()
        {
            var framer = CreateFramer();
            int invokedMessageReceived = 0;
            framer.MessageArrived += (x) => ++invokedMessageReceived;
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', 0x7, 0x13, (byte)'h' });
            framer.DataReceived(new byte[] { (byte)'o', 0x7 });
            Assert.AreEqual(2, invokedMessageReceived, "Delimiter should accept multiple messages when split");
        }

        [TestMethod]
        public void MultipleMessages_SplitBeforeEndDelimiter_AreAllAccepted()
        {
            var framer = CreateFramer();
            int invokedMessageReceived = 0;
            framer.MessageArrived += (x) => ++invokedMessageReceived;
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', 0x7, 0x13, (byte)'h', (byte)'o' });
            framer.DataReceived(new byte[] { 0x7 });
            Assert.AreEqual(2, invokedMessageReceived, "Delimiter should accept multiple messages when split");
        }

        [TestMethod]
        public void MultipleMessages_ArePreserved()
        {
            var framer = CreateFramer();
            IList<byte> message1 = null;
            IList<byte> message2 = null;
            framer.MessageArrived += (x) => { if (message1 == null) message1 = x; else message2 = x; };
            framer.DataReceived(new byte[] { 0x13, (byte)'h', (byte)'i', 0x7, 0x13, (byte)'h', (byte)'o', 0x7 });
            Assert.IsTrue(message1.SequenceEqual(new byte[] { (byte)'h', (byte)'i' }), "Delimiter should preserve first message");
            Assert.IsTrue(message2.SequenceEqual(new byte[] { (byte)'h', (byte)'o' }), "Delimiter should preserve second message");
        }

        /// <summary>
        /// Returns a framer that uses 0x13 as its begin delimiter and 0x7 as its end delimiter.
        /// </summary>
        private static SingleByteDelimiterFramer CreateFramer()
        {
            return new SingleByteDelimiterFramer(0, 0x13, 0x7);
        }

        /// <summary>
        /// Returns a framer that uses 0x13 as its begin delimiter and 0x7 as its end delimiter, with a maximum message size of 2 bytes.
        /// </summary>
        private static SingleByteDelimiterFramer CreateSmallFramer()
        {
            return new SingleByteDelimiterFramer(2, 0x13, 0x7);
        }
    }
}
