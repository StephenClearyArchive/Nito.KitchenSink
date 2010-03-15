using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class RolloverLogTraceListenerUnitTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RolloverLog_RejectsBadConstructorParameters()
        {
            using (var listener = new RolloverLogTraceListener("blah"))
            {
            }
        }

        [TestMethod]
        public void RolloverLog_CreatedWithCurrentLogPresent_RollsOver()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            Directory.CreateDirectory("log" + uniqueTestID);
            byte[] data = Guid.NewGuid().ToByteArray();
            File.WriteAllBytes("log" + uniqueTestID + @"\current.txt", data);

            using (var listener = SimpleListener(uniqueTestID))
            {
                var fileNames = Directory.GetFiles("log" + uniqueTestID).Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
                Assert.AreEqual(2, fileNames.Length, "After recovering from a crash without archive files, there should be two files present.");
                Assert.IsTrue(data.SequenceEqual(File.ReadAllBytes(Path.Combine("log" + uniqueTestID, fileNames[0]))), "The archived file should just be renamed.");
            }
        }

        [TestMethod]
        public void RolloverLog_CreatedWithExtraLogs_TrimsLogs()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            Directory.CreateDirectory("log" + uniqueTestID);
            byte[] data = Guid.NewGuid().ToByteArray();
            File.WriteAllBytes(@"log" + uniqueTestID + @"\2009-10-11 03;24;13.134.txt", data);
            File.WriteAllBytes(@"log" + uniqueTestID + @"\2009-10-11 03;25;13.134.txt", data);
            File.WriteAllBytes(@"log" + uniqueTestID + @"\2009-10-11 03;26;13.134.txt", data);

            using (var listener = SimpleListener(uniqueTestID))
            {
                var fileNames = Directory.GetFiles("log" + uniqueTestID).Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
                Assert.AreEqual(3, fileNames.Length, "Rollover log should trim old logs on startup.");
            }
        }

        [TestMethod]
        public void RolloverLog_CreatedWithNoLogs_OnlyCreatesCurrent()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            Directory.CreateDirectory("log" + uniqueTestID);

            using (var listener = SimpleListener(uniqueTestID))
            {
                var fileNames = Directory.GetFiles("log" + uniqueTestID).Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
                Assert.AreEqual(1, fileNames.Length, "Rollover log should only create a current log if no archive logs are present.");
                Assert.AreEqual("current.txt", Path.GetFileName(fileNames[0]), "Rollover log's current log should be named 'current.txt'.");
            }
        }

        [TestMethod]
        public void RolloverLog_CreatedWithoutLogDirectory_CreatesLogDirectory()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            using (var listener = SimpleListener(uniqueTestID))
            {
                Assert.IsTrue(Directory.Exists("log" + uniqueTestID), "Rollover log should create log directory if it doesn't exist.");
            }
        }

        [TestMethod]
        public void RolloverLog_ShutdownWithoutLogs_DoesNotRolloverEmptyLog()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            using (var listener = SimpleListener(uniqueTestID))
            {
            }

            var fileNames = Directory.GetFiles("log" + uniqueTestID).Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
            Assert.AreEqual(0, fileNames.Length, "Rollover log should delete an empty 'current.txt' instead of rolling it over.");
        }

        [TestMethod]
        public void RolloverLog_ShutdownWithLogs_DoesRolloverLogWithMessage()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            using (var listener = SimpleListener(uniqueTestID))
            {
                listener.Write("Test");
            }

            var fileNames = Directory.GetFiles("log" + uniqueTestID).Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
            Assert.AreEqual(1, fileNames.Length, "Rollover log should roll over a 'current.txt' that has a message.");
            Assert.AreNotEqual("current.txt", fileNames[0], "Rollover log should roll the 'current.txt' file when disposed.");
        }

        [TestMethod]
        public void RolloverLog_GivenSmallMessage_DoesNotRollOver()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            using (var listener = SimpleListener(uniqueTestID))
            {
                listener.WriteLine("test");

                var fileNames = Directory.GetFiles("log" + uniqueTestID).Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
                Assert.AreEqual(1, fileNames.Length, "Rollover log should not rollover for a small message.");
            }
        }

        [TestMethod]
        public void RolloverLog_GivenLargeMessage_DoesRollOver()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            using (var listener = SmallListener(uniqueTestID))
            {
                listener.WriteLine("This is a test string more than 10 bytes long.");

                var fileNames = Directory.GetFiles("log" + uniqueTestID).Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
                Assert.AreEqual(2, fileNames.Length, "Rollover log should not rollover for a large message.");
            }
        }

        [TestMethod]
        public void RolloverLog_GivenMultipleLargeMessages_TrimsOldLogs()
        {
            string uniqueTestID = Guid.NewGuid().ToString("N");
            using (var listener = SmallListener(uniqueTestID))
            {
                listener.WriteLine("This is a test string more than 10 bytes long.");
                listener.WriteLine("This is a test string more than 10 bytes long.");
                listener.WriteLine("This is a test string more than 10 bytes long.");

                var fileNames = Directory.GetFiles("log" + uniqueTestID).Select(x => Path.GetFileName(x)).OrderBy(x => x).ToArray();
                Assert.AreEqual(3, fileNames.Length, "Rollover log should trim old logs as necessary.");
            }
        }

        /// <summary>
        /// Creates a simple listener with directory "log", max file size of 2k bytes, and only 2 archive logs.
        /// </summary>
        /// <param name="testUniqueID">The unique string ID for the test.</param>
        private static RolloverLogTraceListener SimpleListener(string testUniqueID)
        {
            return new RolloverLogTraceListener("log" + testUniqueID + ";2048;2");
        }

        /// <summary>
        /// Creates a simple listener with directory "log", max file size of 10 bytes, and only 2 archive logs.
        /// </summary>
        /// <param name="testUniqueID">The unique string ID for the test.</param>
        private static RolloverLogTraceListener SmallListener(string testUniqueID)
        {
            return new RolloverLogTraceListener("log" + testUniqueID + ";10;2");
        }
    }
}
