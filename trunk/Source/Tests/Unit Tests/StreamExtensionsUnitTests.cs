using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;
using System.IO;
using System.IO.Compression;

namespace UnitTests
{
    [TestClass]
    public class StreamExtensionsUnitTests
    {
        [TestMethod]
        public void TryGetLength_OnStreamWithKnownLength_ReturnsLength()
        {
            var files = Directory.GetFiles(".");
            using (var stream = File.OpenRead(files[0]))
            {
                Assert.AreEqual(stream.Length, stream.TryGetLength(), "TryGetLength should return Length if possible");
            }
        }

        [TestMethod]
        public void TryGetLength_OnStreamWithoutKnownLength_ReturnsLength()
        {
            using (var memoryStream = new MemoryStream())
            using (var stream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                Assert.AreEqual(-1, stream.TryGetLength(), "TryGetLength should return -1 if the length cannot be determined");
            }
        }

        [TestMethod]
        public void StreamCopy_CopiesStream()
        {
            byte[] originalData = Guid.NewGuid().ToByteArray();
            using (var sourceStream = new MemoryStream(originalData))
            using (var destinationStream = new MemoryStream())
            {
                byte[] buffer = new byte[2];
                sourceStream.CopyTo(destinationStream, buffer, null);
                Assert.IsTrue(originalData.SequenceEqual(destinationStream.ToArray()), "CopyTo should copy the stream");
            }
        }

        [TestMethod]
        public void StreamCopy_InvokesProgress()
        {
            bool progressInvoked = false;
            byte[] originalData = Guid.NewGuid().ToByteArray();
            using (var sourceStream = new MemoryStream(originalData))
            using (var destinationStream = new MemoryStream())
            {
                byte[] buffer = new byte[32];
                sourceStream.CopyTo(destinationStream, buffer, _ => progressInvoked = true);
                Assert.IsTrue(progressInvoked, "CopyTo should invoke its progress callback");
            }
        }
    }
}
