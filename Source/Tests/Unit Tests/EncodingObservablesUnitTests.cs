using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Concurrency;
using Nito.KitchenSink;

namespace Tests.Unit_Tests
{
    [TestClass]
    public class EncodingObservablesUnitTests
    {
        [TestMethod]
        public void MSDNEncoderSample()
        {
            var chars = new[]
            {
                new[] { '\u0023' }, // #
                new[] { '\u0025' }, // %
                new[] { '\u03a0' }, // Pi
                new[] { '\u03a3' } // Sigma
            };

            var result = chars.ToObservable(Scheduler.ThreadPool)
                              .Encode(Encoding.UTF7)
                              .ToEnumerable()
                              .SelectMany(x => x)
                              .ToArray();

            Assert.IsTrue(result.SequenceEqual(new byte[] { 43, 65, 67, 77, 65, 74, 81, 79, 103, 65, 54, 77, 45 }));
        }

        [TestMethod]
        public void MSDNEncoderGetBytesSample()
        {
            var chars = new[]
            {
                new[] { '\u0023' }, // #
                new[] { '\u0025' }, // %
                new[] { '\u03a0' }, // Pi
                new[] { '\u03a3' } // Sigma
            };

            var result = chars.ToObservable(Scheduler.ThreadPool)
                              .Encode(Encoding.Unicode)
                              .ToEnumerable()
                              .SelectMany(x => x)
                              .ToArray();

            Assert.IsTrue(result.SequenceEqual(new byte[] { 35, 0, 37, 0, 160, 3, 163, 3 }));
        }

        [TestMethod]
        public void MSDNDecoderSample()
        {
            var bytes = new[]
            {
                new byte[] { 0x20 },
                new byte[] { 0x23 },
                new byte[] { 0xe2 },
                new byte[] { 0x98 },
                new byte[] { 0xa3 },
            };

            var result = bytes.ToObservable(Scheduler.ThreadPool)
                              .Decode(Encoding.UTF8)
                              .ToEnumerable()
                              .SelectMany(x => x)
                              .ToArray();

            Assert.IsTrue(result.SequenceEqual(new[] { '\u0020', '\u0023', '\u2623' }));
        }

        [TestMethod]
        public void MSDNEncoderSampleDecoded()
        {
            var bytes = new[]
            {
                new byte[] { 43 },
                new byte[] { 65 },
                new byte[] { 67 },
                new byte[] { 77 },
                new byte[] { 65 },
                new byte[] { 74 },
                new byte[] { 81 },
                new byte[] { 79 },
                new byte[] { 103 },
                new byte[] { 65 },
                new byte[] { 54 },
                new byte[] { 77 },
                new byte[] { 45 },
            };

            var result = bytes.ToObservable(Scheduler.ThreadPool)
                              .Decode(Encoding.UTF7)
                              .ToEnumerable()
                              .SelectMany(x => x)
                              .ToArray();

            Assert.IsTrue(result.SequenceEqual(new[] { '\u0023', '\u0025', '\u03a0', '\u03a3' }));
        }

        [TestMethod]
        public void MSDNEncoderGetBytesSampleDecoded()
        {
            var bytes = new[]
            {
                new byte[] { 35 }, 
                new byte[] { 0 },
                new byte[] { 37 },
                new byte[] { 0 },
                new byte[] { 160 },
                new byte[] { 3 },
                new byte[] { 163 },
                new byte[] { 3 },
            };

            var result = bytes.ToObservable(Scheduler.ThreadPool)
                              .Decode(Encoding.Unicode)
                              .ToEnumerable()
                              .SelectMany(x => x)
                              .ToArray();

            Assert.IsTrue(result.SequenceEqual(new [] { '\u0023', '\u0025', '\u03a0', '\u03a3' }));
        }

        [TestMethod]
        public void MSDNDecoderSampleEncoded()
        {
            var chars = new[]
            {
                new[] { '\u0020' },
                new[] { '\u0023' },
                new[] { '\u2623' },
            };

            var result = chars.ToObservable(Scheduler.ThreadPool)
                              .Encode(Encoding.UTF8)
                              .ToEnumerable()
                              .SelectMany(x => x)
                              .ToArray();

            Assert.IsTrue(result.SequenceEqual(new byte[] { 0x20, 0x23, 0xe2, 0x98, 0xa3 }));
        }

        [TestMethod]
        public void MSDNDecoderSampleEncodedWithPreamble()
        {
            var chars = new[]
            {
                new[] { '\u0020' },
                new[] { '\u0023' },
                new[] { '\u2623' },
            };

            var result = chars.ToObservable(Scheduler.ThreadPool)
                              .Encode(new UTF8Encoding(true, true), true)
                              .ToEnumerable()
                              .SelectMany(x => x)
                              .ToArray();

            Assert.IsTrue(result.SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF, 0x20, 0x23, 0xe2, 0x98, 0xa3 }));
        }

        [TestMethod]
        public void MSDNDecoderSampleWithPreamble()
        {
            var bytes = new[]
            {
                new byte[] { 0xEF },
                new byte[] { 0xBB },
                new byte[] { 0xBF },
                new byte[] { 0x20 },
                new byte[] { 0x23 },
                new byte[] { 0xe2 },
                new byte[] { 0x98 },
                new byte[] { 0xa3 },
            };

            var result = bytes.ToObservable(Scheduler.ThreadPool)
                              .Decode(new UTF8Encoding(true, true))
                              .ToEnumerable()
                              .SelectMany(x => x)
                              .ToArray();

            Assert.IsTrue(result.SequenceEqual(new[] { '\u0020', '\u0023', '\u2623' }));
        }
    }
}
