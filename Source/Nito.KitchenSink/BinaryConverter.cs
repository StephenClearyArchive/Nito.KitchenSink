using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nito.Linq;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Provides binary converters similar to <see cref="BitConverter"/> but with more generic signatures and byte reversal when necessary.
    /// </summary>
    public static class BinaryConverter
    {
        /// <summary>
        /// The converter that uses little-endian semantics.
        /// </summary>
        private static EndianBinaryConverter littleEndian = new EndianBinaryConverter(true);

        /// <summary>
        /// The converter that uses big-endian semantics.
        /// </summary>
        private static EndianBinaryConverter bigEndian = new EndianBinaryConverter(false);

        /// <summary>
        /// Gets the converter that uses little-endian semantics.
        /// </summary>
        public static EndianBinaryConverter LittleEndian { get { return littleEndian; } }

        /// <summary>
        /// Gets the converter that uses big-endian semantics.
        /// </summary>
        public static EndianBinaryConverter BigEndian { get { return bigEndian; } }

        /// <summary>
        /// Provides binary conversions similar to those in <see cref="BitConverter"/> but with more generic signatures and byte reversal when necessary.
        /// </summary>
        public sealed class EndianBinaryConverter
        {
            /// <summary>
            /// Whether or not this instance needs to reverse its byte sequences (i.e., it is the opposite endianness of the architecture we're running on).
            /// </summary>
            private readonly bool reversesBytes;

            /// <summary>
            /// Initializes a new instance of the <see cref="EndianBinaryConverter"/> class with the specified endianness.
            /// </summary>
            /// <param name="littleEndian">If <c>true</c>, this binary converter is little-endian; if <c>false</c>, this binary converter is big-endian.</param>
            public EndianBinaryConverter(bool littleEndian)
            {
                this.reversesBytes = littleEndian != BitConverter.IsLittleEndian;
            }

            /// <summary>
            /// Converts a boolean value to a 1-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromBoolean(IList<byte> buffer, int offset, bool value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Converts a signed 16-bit integer value to a 2-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromInt16(IList<byte> buffer, int offset, short value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Converts an unsigned 16-bit integer value to a 2-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            [CLSCompliant(false)]
            public void FromUInt16(IList<byte> buffer, int offset, ushort value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Converts a signed 32-bit integer value to a 4-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromInt32(IList<byte> buffer, int offset, int value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Converts an unsigned 32-bit integer value to a 4-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            [CLSCompliant(false)]
            public void FromUInt32(IList<byte> buffer, int offset, uint value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Converts a signed 64-bit integer value to an 8-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromInt64(IList<byte> buffer, int offset, long value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Converts an unsigned 64-bit integer value to an 8-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            [CLSCompliant(false)]
            public void FromUInt64(IList<byte> buffer, int offset, ulong value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Converts a single-precision floating-point value to a 4-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromSingle(IList<byte> buffer, int offset, float value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Converts a double-precision floating-point value to an 8-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromDouble(IList<byte> buffer, int offset, double value)
            {
                if (this.reversesBytes)
                {
                    BitConverter.GetBytes(value).Reverse().CopyTo(buffer, offset);
                }
                else
                {
                    BitConverter.GetBytes(value).CopyTo(buffer, offset);
                }
            }

            /// <summary>
            /// Reads a boolean value from one byte in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public bool ToBoolean(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToBoolean(buffer.Slice(offset, sizeof(bool)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToBoolean(buffer.Slice(offset, sizeof(bool)).ToArray(), 0);
            }

            /// <summary>
            /// Reads a signed 16-bit integer value from two bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public short ToInt16(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToInt16(buffer.Slice(offset, sizeof(short)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToInt16(buffer.Slice(offset, sizeof(short)).ToArray(), 0);
            }

            /// <summary>
            /// Reads an unsigned 16-bit integer value from two bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            [CLSCompliant(false)]
            public ushort ToUInt16(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToUInt16(buffer.Slice(offset, sizeof(ushort)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToUInt16(buffer.Slice(offset, sizeof(ushort)).ToArray(), 0);
            }

            /// <summary>
            /// Reads a signed 32-bit integer value from four bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public int ToInt32(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToInt32(buffer.Slice(offset, sizeof(int)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToInt32(buffer.Slice(offset, sizeof(int)).ToArray(), 0);
            }

            /// <summary>
            /// Reads an unsigned 32-bit integer value from four bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            [CLSCompliant(false)]
            public uint ToUInt32(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToUInt32(buffer.Slice(offset, sizeof(uint)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToUInt32(buffer.Slice(offset, sizeof(uint)).ToArray(), 0);
            }

            /// <summary>
            /// Reads a signed 64-bit integer value from eight bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public long ToInt64(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToInt64(buffer.Slice(offset, sizeof(long)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToInt64(buffer.Slice(offset, sizeof(long)).ToArray(), 0);
            }

            /// <summary>
            /// Reads an unsigned 64-bit integer value from eight bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            [CLSCompliant(false)]
            public ulong ToUInt64(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToUInt64(buffer.Slice(offset, sizeof(ulong)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToUInt64(buffer.Slice(offset, sizeof(ulong)).ToArray(), 0);
            }

            /// <summary>
            /// Reads a single-precision floating-point value from four bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public float ToSingle(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToSingle(buffer.Slice(offset, sizeof(float)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToSingle(buffer.Slice(offset, sizeof(float)).ToArray(), 0);
            }

            /// <summary>
            /// Reads a double-precision floating-point value from eight bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public double ToDouble(IList<byte> buffer, int offset)
            {
                if (this.reversesBytes)
                {
                    return BitConverter.ToDouble(buffer.Slice(offset, sizeof(double)).Reverse().ToArray(), 0);
                }

                return BitConverter.ToDouble(buffer.Slice(offset, sizeof(double)).ToArray(), 0);
            }
        }
    }
}
