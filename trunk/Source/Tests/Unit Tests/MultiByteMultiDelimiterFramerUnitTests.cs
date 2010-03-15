using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink.Communication;

namespace UnitTests
{
    [TestClass]
    public class MultiByteMultiDelimiterFramerUnitTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Delimiter should reject invalid delimiter sequences in constructor")]
        public void BeginAndEndDelimiterSequencesOfDifferentLengths_IsRejected()
        {
            var framer = new MultipleByteMultipleDelimiterFramer(0, new[] { Encoding.ASCII.GetBytes("<beg>"), Encoding.ASCII.GetBytes("<start>") }, new[] { Encoding.ASCII.GetBytes("<end>") });
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.ProtocolViolationException), "Delimiter should reject messages that are too large")]
        public void OversizeMessage_IsRejected()
        {
            var framer = new MultipleByteMultipleDelimiterFramer(2, new[] { Encoding.ASCII.GetBytes("<beg>") }, new[] { Encoding.ASCII.GetBytes("<end>") });
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>hid<end>"));
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.ProtocolViolationException), "Delimiter should reject all out-of-band communications")]
        public void FirstMessage_NotStartingWithStartDelimiter_IsRejected()
        {
            var framer = CreateFramer();
            framer.DataReceived(Encoding.ASCII.GetBytes("bogus67"));
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.ProtocolViolationException), "Delimiter should reject all out-of-band communications")]
        public void FirstMessage_StartingWithPartialIncorrectStartDelimiter_IsRejected()
        {
            var framer = CreateFramer();
            framer.DataReceived(Encoding.ASCII.GetBytes("<start"));
            framer.DataReceived(Encoding.ASCII.GetBytes("?"));
        }

        [TestMethod]
        public void CompleteMessage_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept simple messages");
        }

        [TestMethod]
        public void PartialEnd_IsAcceptedAsPartOfMessage()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<en<end>"));
            Assert.AreEqual("test<en", message1, "Delimiter should accept simple messages with partial ends");
        }

        [TestMethod]
        public void Reset_AfterSplitMessage_ResetsState()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>te"));
            framer.Reset();
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end>"));
            Assert.AreEqual("test", message1, "Delimiter should allow resetting");
        }

        [TestMethod]
        public void SplitMessage_AtStartOfBeginDelimiter_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("beg>test<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessage_InMiddleOfBeginDelimiter_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("eg>test<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessage_AtEndOfBeginDelimiter_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>"));
            framer.DataReceived(Encoding.ASCII.GetBytes("test<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessage_InMiddleOfMessage_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>te"));
            framer.DataReceived(Encoding.ASCII.GetBytes("st<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessage_AtEndOfMessage_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessage_AtStartOfEndDelimiter_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessage_InMiddleOfEndDelimiter_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("nd>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void MultiSplitMessage_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("g"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("s"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("n"));
            framer.DataReceived(Encoding.ASCII.GetBytes("d"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void MultiSplitMessage_WithPartialEnd_IsAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            framer.MessageArrived += (i, x) => message1 = Encoding.ASCII.GetString(x.ToArray());
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("g"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("s"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("n"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("n"));
            framer.DataReceived(Encoding.ASCII.GetBytes("d"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            Assert.AreEqual("test<en", message1, "Delimiter should accept split messages");
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.ProtocolViolationException), "Delimiter should reject all out-of-band communications")]
        public void SecondMessage_NotStartingWithStartDelimiter_IsRejected()
        {
            var framer = CreateFramer();
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end>bogus67"));
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.ProtocolViolationException), "Delimiter should reject all out-of-band communications")]
        public void SecondMessage_StartingWithPartialIncorrectStartDelimiter_IsRejected()
        {
            var framer = CreateFramer();
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><beg567"));
            framer.DataReceived(Encoding.ASCII.GetBytes("?"));
        }

        [TestMethod]
        public void CompleteMessages_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept multiple messages");
            Assert.AreEqual("other", message2, "Delimiter should accept multiple messages");
        }

        [TestMethod]
        public void PartialEnds_AreAcceptedAsPartOfMessage()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<en<end><beg>other<end<end>"));
            Assert.AreEqual("test<en", message1, "Delimiter should accept multiple messages with partial ends");
            Assert.AreEqual("other<end", message2, "Delimiter should accept multiple messages with partial ends");
        }

        [TestMethod]
        public void SplitMessages_AtStartOfFirstBeginDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("beg>test<end><beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_AtStartOfSecondBeginDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><"));
            framer.DataReceived(Encoding.ASCII.GetBytes("beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_InMiddleOfFirstBeginDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("eg>test<end><beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_InMiddleOfSecondBeginDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("eg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_AtEndOfFirstBeginDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>"));
            framer.DataReceived(Encoding.ASCII.GetBytes("test<end><beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_AtEndOfSecondBeginDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><beg>"));
            framer.DataReceived(Encoding.ASCII.GetBytes("other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_InMiddleOfFirstMessage_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>te"));
            framer.DataReceived(Encoding.ASCII.GetBytes("st<end><beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_InMiddleOfSecondMessage_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><beg>oth"));
            framer.DataReceived(Encoding.ASCII.GetBytes("er<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_AtEndOfFirstMessage_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<end><beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_AtEndOfSecondMessage_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><beg>other"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_AtStartOfFirstEndDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("end><beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_AtStartOfSecondEndDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><beg>other<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_InMiddleOfFirstEndDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("nd><beg>other<end>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void SplitMessages_InMiddleOfSecondEndDelimiter_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<beg>test<end><beg>other<en"));
            framer.DataReceived(Encoding.ASCII.GetBytes("d>"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void MultiSplitMessages_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("g"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("s"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("n"));
            framer.DataReceived(Encoding.ASCII.GetBytes("d"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("g"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            framer.DataReceived(Encoding.ASCII.GetBytes("o"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("h"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("r"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("n"));
            framer.DataReceived(Encoding.ASCII.GetBytes("d"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            Assert.AreEqual("test", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        [TestMethod]
        public void MultiSplitMessages_WithPartialEnd_AreAccepted()
        {
            var framer = CreateFramer();
            string message1 = null;
            string message2 = null;
            framer.MessageArrived += (i, x) => { if (message1 == null) message1 = Encoding.ASCII.GetString(x.ToArray()); else message2 = Encoding.ASCII.GetString(x.ToArray()); };
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("g"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("s"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("n"));
            framer.DataReceived(Encoding.ASCII.GetBytes("d"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("b"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("g"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            framer.DataReceived(Encoding.ASCII.GetBytes("o"));
            framer.DataReceived(Encoding.ASCII.GetBytes("t"));
            framer.DataReceived(Encoding.ASCII.GetBytes("h"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("r"));
            framer.DataReceived(Encoding.ASCII.GetBytes("<"));
            framer.DataReceived(Encoding.ASCII.GetBytes("e"));
            framer.DataReceived(Encoding.ASCII.GetBytes("n"));
            framer.DataReceived(Encoding.ASCII.GetBytes("d"));
            framer.DataReceived(Encoding.ASCII.GetBytes(">"));
            Assert.AreEqual("test<e", message1, "Delimiter should accept split messages");
            Assert.AreEqual("other", message2, "Delimiter should accept split messages");
        }

        private MultipleByteMultipleDelimiterFramer CreateFramer()
        {
            return new MultipleByteMultipleDelimiterFramer(0, new[] { Encoding.ASCII.GetBytes("<beg>"), Encoding.ASCII.GetBytes("<start>") }, new[] { Encoding.ASCII.GetBytes("<end>"), Encoding.ASCII.GetBytes("<end>") });
        }
    }
}
