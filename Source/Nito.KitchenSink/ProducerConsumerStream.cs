using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    using System.Collections.Concurrent;
    using System.IO;

    /// <summary>
    /// A stream that can be written to and read from by different threads. Multiple producer (writer) threads are supported, but only one consumer (reader) thread is supported.
    /// </summary>
    public sealed class ProducerConsumerStream : Stream
    {
        /// <summary>
        /// The underlying buffer that provides the producer/consumer behavior.
        /// </summary>
        private readonly BlockingCollection<byte[]> buffer;

        /// <summary>
        /// The remaining buffer if a read only gets part of a write.
        /// </summary>
        private byte[] remainingBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerConsumerStream"/> class.
        /// </summary>
        public ProducerConsumerStream()
        {
            this.buffer = new BlockingCollection<byte[]>();
        }

        /// <summary>
        /// Disposes the underlying buffer.
        /// </summary>
        /// <param name="disposing"><c>true</c> if the method is called from <see cref="Dispose"/>; <c>false</c> if the method is called from the finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.buffer.Dispose();
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("ProducerConsumerStream does not support seeking.");
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("ProducerConsumerStream does not support seeking.");
        }

        /// <summary>
        /// Returns the next buffer in the stream. May return <c>null</c> if the stream has been closed for writing.
        /// </summary>
        /// <returns>The next buffer in the stream.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The object has been disposed. </exception>
        private byte[] TryTake()
        {
            if (this.remainingBuffer != null)
            {
                return this.remainingBuffer;
            }

            try
            {
                return this.buffer.Take();
            }
            catch (InvalidOperationException)
            {
                // InvalidOperationException is thrown either when the underlying collection is modified (which it never is)
                //  or when the buffer has been marked as complete for adding.
                return null;
            }
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var internalBuffer = this.TryTake();
            if (internalBuffer == null)
            {
                // End of stream.
                return 0;
            }

            if (internalBuffer.Length <= count)
            {
                Array.Copy(internalBuffer, 0, buffer, offset, internalBuffer.Length);
                this.remainingBuffer = null;
                return internalBuffer.Length;
            }
            else
            {
                Array.Copy(internalBuffer, 0, buffer, offset, count);
                this.remainingBuffer = new byte[internalBuffer.Length - count];
                Array.Copy(internalBuffer, count, this.remainingBuffer, 0, this.remainingBuffer.Length);
                return count;
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. </param>
        /// <param name="count">The number of bytes to be written to the current stream. </param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return;
            }

            var internalBuffer = new byte[count];
            Array.Copy(buffer, offset, internalBuffer, 0, count);
            this.buffer.Add(internalBuffer);
        }

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        /// <returns>
        /// <c>true</c>.
        /// </returns>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        /// <returns>
        /// <c>false</c>.
        /// </returns>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        /// <returns>
        /// <c>true</c>.
        /// </returns>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>
        /// </returns>
        public override long Length
        {
            get
            {
                throw new NotSupportedException("ProducerConsumerStream does not support seeking.");
            }
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>
        /// </returns>
        public override long Position
        {
            get
            {
                throw new NotSupportedException("ProducerConsumerStream does not support seeking.");
            }

            set
            {
                throw new NotSupportedException("ProducerConsumerStream does not support seeking.");
            }
        }
    }
}
