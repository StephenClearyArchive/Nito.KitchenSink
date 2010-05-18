﻿// <copyright file="CRC16.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using Nito.Linq;

    /// <summary>
    /// A generalized CRC-16 algorithm.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class CRC16 : HashAlgorithm
    {
        /// <summary>
        /// The lookup tables for non-reversed polynomials.
        /// </summary>
        private static readonly Dictionary<ushort, ushort[]> NormalLookupTables = new Dictionary<ushort, ushort[]>(0);
        
        /// <summary>
        /// The lookup tables for reversed polynomials.
        /// </summary>
        private static readonly Dictionary<ushort, ushort[]> ReversedLookupTables = new Dictionary<ushort, ushort[]>(0);

        /// <summary>
        /// A reference to the lookup table.
        /// </summary>
        private readonly ushort[] lookupTable;

        /// <summary>
        /// The CRC-32 algorithm definition.
        /// </summary>
        private readonly Definition definition;

        /// <summary>
        /// The current value of the remainder.
        /// </summary>
        private ushort remainder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC16"/> class with the specified definition and lookup table.
        /// </summary>
        /// <param name="definition">The CRC-16 algorithm definition.</param>
        /// <param name="lookupTable">The lookup table.</param>
        public CRC16(Definition definition, ushort[] lookupTable)
        {
            this.HashSizeValue = 16;
            this.definition = definition;
            this.lookupTable = lookupTable;
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC16"/> class with the specified definition.
        /// </summary>
        /// <param name="definition">The CRC-16 algorithm definition.</param>
        public CRC16(Definition definition)
            : this(definition, FindOrGenerateLookupTable(definition))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC16"/> class with the default definition.
        /// </summary>
        public CRC16()
            : this(Definition.Default)
        {
        }

        /// <summary>
        /// Gets the result of the CRC-16 algorithm.
        /// </summary>
        public ushort Result
        {
            get
            {
                if (this.definition.ReverseResultBeforeFinalXor != this.definition.ReverseDataBytes)
                {
                    return (ushort)(HackersDelight.Reverse(this.remainder) ^ this.definition.FinalXorValue);
                }
                else
                {
                    return (ushort)(this.remainder ^ this.definition.FinalXorValue);
                }
            }
        }

        /// <summary>
        /// Searches the known lookup tables for one matching the given CRC-16 definition; if none is found, a new lookup table is generated and added to the known lookup tables.
        /// </summary>
        /// <param name="definition">The CRC-16 definition.</param>
        /// <returns>The lookup table for the given CRC-16 definition.</returns>
        public static ushort[] FindOrGenerateLookupTable(Definition definition)
        {
            Dictionary<ushort, ushort[]> tables;
            if (definition.ReverseDataBytes)
            {
                tables = ReversedLookupTables;
            }
            else
            {
                tables = NormalLookupTables;
            }

            lock (tables)
            {
                ushort[] ret;
                if (!tables.TryGetValue(definition.TruncatedPolynomial, out ret))
                {
                    ret = GenerateLookupTable(definition);
                    tables.Add(definition.TruncatedPolynomial, ret);
                }

                return ret;
            }
        }

        /// <summary>
        /// Generates a lookup table for a CRC-16 algorithm definition. Both <see cref="Definition.TruncatedPolynomial"/> and <see cref="Definition.ReverseDataBytes"/> are used in the calculations.
        /// </summary>
        /// <param name="definition">The CRC-16 algorithm definition.</param>
        /// <returns>The lookup table.</returns>
        public static ushort[] GenerateLookupTable(Definition definition)
        {
            unchecked
            {
                ushort[] ret = new ushort[256];

                byte dividend = 0;
                do
                {
                    ushort remainder = 0;

                    for (byte mask = 0x80; mask != 0; mask >>= 1)
                    {
                        if ((dividend & mask) != 0)
                        {
                            remainder ^= 0x8000;
                        }

                        if ((remainder & 0x8000) != 0)
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
                        ret[HackersDelight.Reverse(dividend)] = HackersDelight.Reverse(remainder);
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
        /// Initializes the CRC-16 calculations.
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
        /// Updates the hash value by hashing the provided byte buffer. <see cref="Initialize"/> should be called before invoking this method for the first time.
        /// </summary>
        /// <param name="buffer">The input byte array to include in the hash calculation.</param>
        public void Update(IList<byte> buffer)
        {
            unchecked
            {
                ushort remainder = this.remainder;
                foreach (byte data in buffer)
                {
                    byte index = this.ReflectedIndex(remainder, data);
                    remainder = this.ReflectedShift(remainder);
                    remainder ^= this.lookupTable[index];
                }

                this.remainder = remainder;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return "0x" + this.Result.ToString("X4");
        }

        /// <summary>
        /// Routes data written to the object into the hash algorithm for computing the hash.
        /// </summary>
        /// <param name="array">The input to compute the hash code for.</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the byte array to use as data.</param>
        protected override void HashCore(byte[] array, int offset, int count)
        {
            this.Update(array.Slice(offset, count));
        }

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(this.Result);
        }

        /// <summary>
        /// Gets the index into the lookup array for a given remainder and data byte. Data byte reversal is taken into account.
        /// </summary>
        /// <param name="remainder">The current remainder.</param>
        /// <param name="data">The data byte.</param>
        /// <returns>The index into the lookup array.</returns>
        private byte ReflectedIndex(ushort remainder, byte data)
        {
            unchecked
            {
                if (this.definition.ReverseDataBytes)
                {
                    return (byte)(remainder ^ data);
                }
                else
                {
                    return (byte)((remainder >> 8) ^ data);
                }
            }
        }

        /// <summary>
        /// Shifts a byte out of the remainder. This is the high byte or low byte, depending on whether the data bytes are reversed.
        /// </summary>
        /// <param name="remainder">The remainder value.</param>
        /// <returns>The shifted remainder value.</returns>
        private ushort ReflectedShift(ushort remainder)
        {
            unchecked
            {
                if (this.definition.ReverseDataBytes)
                {
                    return (ushort)(remainder >> 8);
                }
                else
                {
                    return (ushort)(remainder << 8);
                }
            }
        }

        /// <summary>
        /// Holds parameters for a CRC-16 algorithm.
        /// </summary>
        public struct Definition
        {
            /// <summary>
            /// Gets a common CRC-16, used by ARC and LHA.
            /// </summary>
            public static Definition Default
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x8005,
                        ReverseDataBytes = true,
                        ReverseResultBeforeFinalXor = true,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-16 used by floppy disk formats, commonly misidentified as CCITT.
            /// </summary>
            public static Definition CcittFalse
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1021,
                        Initializer = 0xFFFF,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-16 known as CCITT, used by Kermit. Appears in "Numerical Recipes in C".
            /// </summary>
            public static Definition Ccitt
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1021,
                        ReverseDataBytes = true,
                        ReverseResultBeforeFinalXor = true,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-16 used by XMODEM and ZMODEM. Appears in "Numerical Recipes in C".
            /// </summary>
            public static Definition XModem
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1021,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-16 used by X.25, V.42, T.30, RFC 1171. Appears in "Numerical Recipes in C".
            /// </summary>
            public static Definition X25
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1021,
                        Initializer = 0xFFFF,
                        FinalXorValue = 0xFFFF,
                        ReverseDataBytes = true,
                        ReverseResultBeforeFinalXor = true,
                    };
                }
            }

            /// <summary>
            /// Gets or sets the normal (non-reversed, non-reciprocal) polynomial to use for the CRC calculations.
            /// </summary>
            public ushort TruncatedPolynomial { get; set; }

            /// <summary>
            /// Gets or sets the value to which the remainder is initialized at the beginning of the CRC calculation.
            /// </summary>
            public ushort Initializer { get; set; }

            /// <summary>
            /// Gets or sets the value by which the remainder is XOR'ed at the end of the CRC calculation.
            /// </summary>
            public ushort FinalXorValue { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether incoming data bytes are reversed/reflected.
            /// </summary>
            public bool ReverseDataBytes { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the final remainder is reversed/reflected at the end of the CRC calculation before it is XOR'ed with <see cref="FinalXorValue"/>.
            /// </summary>
            public bool ReverseResultBeforeFinalXor { get; set; }
        }
    }
}
