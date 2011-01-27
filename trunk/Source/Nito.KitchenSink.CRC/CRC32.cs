// <copyright file="CRC32.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.CRC
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// A generalized CRC-32 algorithm.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class CRC32 : HashAlgorithm
    {
        /// <summary>
        /// The lookup tables for non-reversed polynomials.
        /// </summary>
        private static readonly ConcurrentDictionary<uint, uint[]> NormalLookupTables = new ConcurrentDictionary<uint, uint[]>();
        
        /// <summary>
        /// The lookup tables for reversed polynomials.
        /// </summary>
        private static readonly ConcurrentDictionary<uint, uint[]> ReversedLookupTables = new ConcurrentDictionary<uint, uint[]>();

        /// <summary>
        /// A reference to the lookup table.
        /// </summary>
        private readonly uint[] lookupTable;

        /// <summary>
        /// The CRC-32 algorithm definition.
        /// </summary>
        private readonly Definition definition;

        /// <summary>
        /// The current value of the remainder.
        /// </summary>
        private uint remainder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC32"/> class with the specified definition and lookup table.
        /// </summary>
        /// <param name="definition">The CRC-32 algorithm definition. May not be <c>null</c>.</param>
        /// <param name="lookupTable">The lookup table. May not be <c>null</c>.</param>
        public CRC32(Definition definition, uint[] lookupTable)
        {
            Contract.Requires(definition != null);
            Contract.Requires(lookupTable != null);
            Contract.Requires(lookupTable.Length == 256);

            this.HashSizeValue = 32;
            this.definition = definition;
            this.lookupTable = lookupTable;
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC32"/> class with the specified definition.
        /// </summary>
        /// <param name="definition">The CRC-32 algorithm definition. May not be <c>null</c>.</param>
        public CRC32(Definition definition)
            : this(definition, FindOrGenerateLookupTable(definition))
        {
            Contract.Requires(definition != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC32"/> class with the default definition. Note that the "default" CRC-32 definition is an older IEEE recommendation and there are better polynomials for new protocols.
        /// </summary>
        public CRC32()
            : this(Definition.Default)
        {
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.definition != null);
            Contract.Invariant(this.lookupTable != null);
            Contract.Invariant(this.lookupTable.Length == 256);
        }

        /// <summary>
        /// Gets the result of the CRC-32 algorithm.
        /// </summary>
        public uint Result
        {
            get
            {
                if (this.definition.ReverseResultBeforeFinalXor != this.definition.ReverseDataBytes)
                {
                    return HackersDelight.Reverse(this.remainder) ^ this.definition.FinalXorValue;
                }
                else
                {
                    return this.remainder ^ this.definition.FinalXorValue;
                }
            }
        }

        /// <summary>
        /// Searches the known lookup tables for one matching the given CRC-32 definition; if none is found, a new lookup table is generated and added to the known lookup tables.
        /// </summary>
        /// <param name="definition">The CRC-32 definition.</param>
        /// <returns>The lookup table for the given CRC-32 definition.</returns>
        public static uint[] FindOrGenerateLookupTable(Definition definition)
        {
            Contract.Requires(definition != null);
            Contract.Ensures(Contract.Result<uint[]>() != null);
            Contract.Ensures(Contract.Result<uint[]>().Length == 256);

            ConcurrentDictionary<uint, uint[]> tables;
            if (definition.ReverseDataBytes)
            {
                tables = ReversedLookupTables;
            }
            else
            {
                tables = NormalLookupTables;
            }

            var ret = tables.GetOrAdd(definition.TruncatedPolynomial, _ => GenerateLookupTable(definition));
            Contract.Assume(ret != null);
            Contract.Assume(ret.Length == 256);
            return ret;
        }

        /// <summary>
        /// Generates a lookup table for a CRC-32 algorithm definition. Both <see cref="Definition.TruncatedPolynomial"/> and <see cref="Definition.ReverseDataBytes"/> are used in the calculations.
        /// </summary>
        /// <param name="definition">The CRC-32 algorithm definition.</param>
        /// <returns>The lookup table.</returns>
        public static uint[] GenerateLookupTable(Definition definition)
        {
            Contract.Requires(definition != null);
            Contract.Ensures(Contract.Result<uint[]>() != null);
            Contract.Ensures(Contract.Result<uint[]>().Length == 256);

            unchecked
            {
                uint[] ret = new uint[256];

                byte dividend = 0;
                do
                {
                    uint remainder = 0;

                    for (byte mask = 0x80; mask != 0; mask >>= 1)
                    {
                        if ((dividend & mask) != 0)
                        {
                            remainder ^= 0x80000000;
                        }

                        if ((remainder & 0x80000000) != 0)
                        {
                            remainder <<= 1;
                            remainder ^= definition.TruncatedPolynomial;
                        }
                        else
                        {
                            remainder <<= 1;
                        }
                    }

                    if (definition.ReverseDataBytes)
                    {
                        var index = HackersDelight.Reverse(dividend);
                        Contract.Assume(index >= 0);
                        Contract.Assume(index <= 255);
                        ret[index] = HackersDelight.Reverse(remainder);
                    }
                    else
                    {
                        ret[dividend] = remainder;
                    }

                    ++dividend;
                }
                while (dividend != 0);

                return ret;
            }
        }

        /// <summary>
        /// Initializes the CRC-32 calculations.
        /// </summary>
        public override void Initialize()
        {
            if (this.definition.ReverseDataBytes)
            {
                this.remainder = HackersDelight.Reverse(this.definition.Initializer);
            }
            else
            {
                this.remainder = this.definition.Initializer;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return "0x" + this.Result.ToString("X8");
        }

        /// <summary>
        /// Routes data written to the object into the hash algorithm for computing the hash.
        /// </summary>
        /// <param name="array">The input to compute the hash code for.</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the byte array to use as data.</param>
        protected override void HashCore(byte[] array, int offset, int count)
        {
            Contract.Assume(array != null);
            Contract.Assume(offset >= 0);
            Contract.Assume(count >= 0);
            Contract.Assume(count <= array.Length);
            Contract.Assume(array.Length - count >= offset);

            unchecked
            {
                uint remainder = this.remainder;
                for (int i = offset; i != offset + count; ++i)
                {
                    Contract.Assume(i < array.Length);
                    byte index = this.ReflectedIndex(remainder, array[i]);
                    remainder = this.ReflectedShift(remainder);
                    Contract.Assume(index >= 0);
                    Contract.Assume(index <= 255);
                    remainder ^= this.lookupTable[index];
                }

                this.remainder = remainder;
            }
        }

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        protected override byte[] HashFinal()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 4);

            return BitConverter.GetBytes(this.Result);
        }

        /// <summary>
        /// Gets the index into the lookup array for a given remainder and data byte. Data byte reversal is taken into account.
        /// </summary>
        /// <param name="remainder">The current remainder.</param>
        /// <param name="data">The data byte.</param>
        /// <returns>The index into the lookup array.</returns>
        private byte ReflectedIndex(uint remainder, byte data)
        {
            unchecked
            {
                if (this.definition.ReverseDataBytes)
                {
                    return (byte)(remainder ^ data);
                }
                else
                {
                    return (byte)((remainder >> 24) ^ data);
                }
            }
        }

        /// <summary>
        /// Shifts a byte out of the remainder. This is the high byte or low byte, depending on whether the data bytes are reversed.
        /// </summary>
        /// <param name="remainder">The remainder value.</param>
        /// <returns>The shifted remainder value.</returns>
        private uint ReflectedShift(uint remainder)
        {
            unchecked
            {
                if (this.definition.ReverseDataBytes)
                {
                    return remainder >> 8;
                }
                else
                {
                    return remainder << 8;
                }
            }
        }

        /// <summary>
        /// Holds parameters for a CRC-32 algorithm.
        /// </summary>
        public sealed class Definition
        {
            /// <summary>
            /// Gets a CRC-32 defined by the old IEEE standard; used by Ethernet, zip, PNG, etc. Note that this "default" CRC-32 definition is an older IEEE recommendation and there are better polynomials for new protocols. Known as "CRC-32", "CRC-32/ADCCP", and "PKZIP".
            /// </summary>
            public static Definition Default
            {
                get
                {
                    Contract.Ensures(Contract.Result<Definition>() != null);
                    return new Definition
                    {
                        TruncatedPolynomial = 0x04C11DB7,
                        Initializer = 0xFFFFFFFF,
                        FinalXorValue = 0xFFFFFFFF,
                        ReverseDataBytes = true,
                        ReverseResultBeforeFinalXor = true,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-32 used by BZIP2. Known as "CRC-32/BZIP2" and "B-CRC-32".
            /// </summary>
            public static Definition BZip2
            {
                get
                {
                    Contract.Ensures(Contract.Result<Definition>() != null);
                    return new Definition
                    {
                        TruncatedPolynomial = 0x04C11DB7,
                        Initializer = 0xFFFFFFFF,
                        FinalXorValue = 0xFFFFFFFF,
                    };
                }
            }

            /// <summary>
            /// Gets a modern CRC-32 defined in RFC 3720. Known as "CRC-32C", "CRC-32/ISCSI", and "CRC-32/CASTAGNOLI".
            /// </summary>
            public static Definition Castagnoli
            {
                get
                {
                    Contract.Ensures(Contract.Result<Definition>() != null);
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1EDC6F41,
                        Initializer = 0xFFFFFFFF,
                        FinalXorValue = 0xFFFFFFFF,
                        ReverseDataBytes = true,
                        ReverseResultBeforeFinalXor = true,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-32 used by the MPEG-2 standard. Known as "CRC-32/MPEG-2".
            /// </summary>
            public static Definition Mpeg2
            {
                get
                {
                    Contract.Ensures(Contract.Result<Definition>() != null);
                    return new Definition
                    {
                        TruncatedPolynomial = 0x04C11DB7,
                        Initializer = 0xFFFFFFFF,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-32 used by the POSIX "chksum" command; note that the chksum command-line program appends the file length to the contents unless it is empty. Known as "CRC-32/POSIX" and "CKSUM".
            /// </summary>
            public static Definition Posix
            {
                get
                {
                    Contract.Ensures(Contract.Result<Definition>() != null);
                    return new Definition
                    {
                        TruncatedPolynomial = 0x04C11DB7,
                        FinalXorValue = 0xFFFFFFFF,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-32 used in the Aeronautical Information eXchange Model. Known as "CRC-32Q".
            /// </summary>
            public static Definition Aixm
            {
                get
                {
                    Contract.Ensures(Contract.Result<Definition>() != null);
                    return new Definition
                    {
                        TruncatedPolynomial = 0x814141AB,
                    };
                }
            }

            /// <summary>
            /// Gets a very old CRC-32, appearing in "Numerical Recipes in C". Known as "XFER".
            /// </summary>
            public static Definition Xfer
            {
                get
                {
                    Contract.Ensures(Contract.Result<Definition>() != null);
                    return new Definition
                    {
                        TruncatedPolynomial = 0x000000AF,
                    };
                }
            }

            /// <summary>
            /// Gets or sets the normal (non-reversed, non-reciprocal) polynomial to use for the CRC calculations.
            /// </summary>
            public uint TruncatedPolynomial { get; set; }

            /// <summary>
            /// Gets or sets the value to which the remainder is initialized at the beginning of the CRC calculation.
            /// </summary>
            public uint Initializer { get; set; }

            /// <summary>
            /// Gets or sets the value by which the remainder is XOR'ed at the end of the CRC calculation.
            /// </summary>
            public uint FinalXorValue { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether incoming data bytes are reversed/reflected.
            /// </summary>
            public bool ReverseDataBytes { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the final remainder is reversed/reflected at the end of the CRC calculation before it is XOR'ed with <see cref="FinalXorValue"/>.
            /// </summary>
            public bool ReverseResultBeforeFinalXor { get; set; }

            /// <summary>
            /// Creates a new <see cref="CRC32"/> instance using this definition.
            /// </summary>
            /// <returns>A new <see cref="CRC32"/> instance using this definition.</returns>
            public CRC32 Create()
            {
                Contract.Ensures(Contract.Result<CRC32>() != null);
                return new CRC32(this);
            }
        }
    }
}
