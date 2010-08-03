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
            // The characters to encode.
            var chars = new[]
            {
                new[] { '\u0023' }, // #
                new[] { '\u0025' }, // %
                new[] { '\u03a0' }, // Pi
                new[] { '\u03a3' } // Sigma
            };

            var result =
                chars.ToObservable(Scheduler.ThreadPool).Encode(Encoding.UTF7).ToEnumerable().SelectMany(x => x).ToArray
                    ();

            Assert.IsTrue(result.SequenceEqual(new byte[] { 43, 65, 67, 77, 65, 74, 81, 79, 103, 65, 54, 77, 45 }));
        }

        [TestMethod]
        public void MSDNEncoderGetBytesSample()
        {
            // The characters to encode.
            var chars = new[]
            {
                new[] { '\u0023' }, // #
                new[] { '\u0025' }, // %
                new[] { '\u03a0' }, // Pi
                new[] { '\u03a3' } // Sigma
            };

            var result = chars.ToObservable(Scheduler.ThreadPool)
                .Encode(Encoding.Unicode).ToEnumerable().SelectMany(x => x).ToArray();

            Assert.IsTrue(result.SequenceEqual(new byte[] { 35, 0, 37, 0, 160, 3, 163, 3 }));
        }

        [TestMethod]
        public void MSDNDecoderSample()
        {
            var bytes = new[]
            {
                new byte[] { 0x20, 0x23, 0xe2 },
                new byte[] { 0x98, 0xa3 },
            };

            var result = bytes.ToObservable(Scheduler.ThreadPool)
                .Decode(Encoding.UTF8).ToEnumerable().SelectMany(x => x).ToArray();

            Assert.IsTrue(result.SequenceEqual(new[] { '\u0020', '\u0023', '\u2623' }));
        }
    }
}
