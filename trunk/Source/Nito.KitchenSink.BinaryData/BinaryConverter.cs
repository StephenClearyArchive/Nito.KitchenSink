// <copyright file="BinaryConverter.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.BinaryData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides binary converters similar to <see cref="BitConverter"/> but with more generic signatures and endian conversions when necessary.
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
        public static EndianBinaryConverter LittleEndian
        {
            get
            {
                Contract.Ensures(Contract.Result<EndianBinaryConverter>() != null);
                return littleEndian;
            }
        }

        /// <summary>
        /// Gets the converter that uses big-endian semantics.
        /// </summary>
        public static EndianBinaryConverter BigEndian
        {
            get
            {
                Contract.Ensures(Contract.Result<EndianBinaryConverter>() != null);
                return bigEndian;
            }
        }

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

            private byte[] ReverseIfNecessary(byte[] array)
            {
                Contract.Requires(array != null);
                Contract.Ensures(Contract.Result<byte[]>() == array);

                if (!this.reversesBytes)
                {
                    return array;
                }

                for (int i = 0; i < array.Length / 2; ++i)
                {
                    byte tmp = array[array.Length - i - 1];
                    array[array.Length - i - 1] = array[i];
                    array[i] = tmp;
                }

                return array;
            }

            private static void CopyTo(byte[] source, IList<byte> destination, int offset)
            {
                Contract.Requires(source != null);
                Contract.Requires(destination != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(source.Length <= destination.Count);
                Contract.Requires(destination.Count - source.Length >= offset);

                for (int i = 0; i != source.Length; ++i)
                {
                    destination[offset + i] = source[i];
                }
            }

            private static byte[] Slice(IList<byte> source, int offset, int count)
            {
                Contract.Requires(source != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(count >= 0);
                Contract.Requires(count <= source.Count);
                Contract.Requires(source.Count - count >= offset);
                Contract.Ensures(Contract.Result<byte[]>() != null);
                Contract.Ensures(Contract.Result<byte[]>().Length == count);

                var ret = new byte[count];
                for (int i = 0; i != count; ++i)
                {
                    ret[i] = source[offset + i];
                }

                return ret;
            }

            /// <summary>
            /// Converts a boolean value to a 1-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromBoolean(IList<byte> buffer, int offset, bool value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(bool) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Converts a signed 16-bit integer value to a 2-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromInt16(IList<byte> buffer, int offset, short value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(short) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Converts an unsigned 16-bit integer value to a 2-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            [CLSCompliant(false)]
            public void FromUInt16(IList<byte> buffer, int offset, ushort value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(ushort) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Converts a signed 32-bit integer value to a 4-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromInt32(IList<byte> buffer, int offset, int value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(int) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Converts an unsigned 32-bit integer value to a 4-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            [CLSCompliant(false)]
            public void FromUInt32(IList<byte> buffer, int offset, uint value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(uint) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Converts a signed 64-bit integer value to an 8-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromInt64(IList<byte> buffer, int offset, long value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(long) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Converts an unsigned 64-bit integer value to an 8-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            [CLSCompliant(false)]
            public void FromUInt64(IList<byte> buffer, int offset, ulong value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(ulong) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Converts a single-precision floating-point value to a 4-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromSingle(IList<byte> buffer, int offset, float value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(float) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Converts a double-precision floating-point value to an 8-byte sequence and stores it in the specified buffer at the specified offset.
            /// </summary>
            /// <param name="buffer">The buffer in which to store the binary value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to store the binary value.</param>
            /// <param name="value">The value to store in the buffer.</param>
            public void FromDouble(IList<byte> buffer, int offset, double value)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(double) >= offset);
                CopyTo(this.ReverseIfNecessary(BitConverter.GetBytes(value)), buffer, offset);
            }

            /// <summary>
            /// Reads a boolean value from one byte in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public bool ToBoolean(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(bool) >= offset);
                return BitConverter.ToBoolean(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(bool))), 0);
            }

            /// <summary>
            /// Reads a signed 16-bit integer value from two bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public short ToInt16(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(short) >= offset);
                return BitConverter.ToInt16(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(short))), 0);
            }

            /// <summary>
            /// Reads an unsigned 16-bit integer value from two bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            [CLSCompliant(false)]
            public ushort ToUInt16(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(ushort) >= offset);
                return BitConverter.ToUInt16(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(ushort))), 0);
            }

            /// <summary>
            /// Reads a signed 32-bit integer value from four bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public int ToInt32(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(int) >= offset);
                return BitConverter.ToInt32(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(int))), 0);
            }

            /// <summary>
            /// Reads an unsigned 32-bit integer value from four bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            [CLSCompliant(false)]
            public uint ToUInt32(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(uint) >= offset);
                return BitConverter.ToUInt32(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(uint))), 0);
            }

            /// <summary>
            /// Reads a signed 64-bit integer value from eight bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public long ToInt64(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(long) >= offset);
                return BitConverter.ToInt64(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(long))), 0);
            }

            /// <summary>
            /// Reads an unsigned 64-bit integer value from eight bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            [CLSCompliant(false)]
            public ulong ToUInt64(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(ulong) >= offset);
                return BitConverter.ToUInt64(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(ulong))), 0);
            }

            /// <summary>
            /// Reads a single-precision floating-point value from four bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public float ToSingle(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(float) >= offset);
                return BitConverter.ToSingle(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(float))), 0);
            }

            /// <summary>
            /// Reads a double-precision floating-point value from eight bytes in the specified buffer at the specified position.
            /// </summary>
            /// <param name="buffer">The buffer from which to read the value. May not be <c>null</c>.</param>
            /// <param name="offset">The offset in the buffer at which to read the value.</param>
            /// <returns>The value read from the buffer.</returns>
            public double ToDouble(IList<byte> buffer, int offset)
            {
                Contract.Requires(buffer != null);
                Contract.Requires(offset >= 0);
                Contract.Requires(buffer.Count - sizeof(double) >= offset);
                return BitConverter.ToDouble(this.ReverseIfNecessary(Slice(buffer, offset, sizeof(double))), 0);
            }
        }
    }
}
