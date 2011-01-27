// <copyright file="StringExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.BinaryData
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Provides useful extension methods for byte sequences.
    /// </summary>
    public static class ByteSequenceExtensions
    {
        /// <summary>
        /// The escape sequences used by "pretty printing".
        /// </summary>
        private static Dictionary<byte, string> prettyEscapeSequences = new Dictionary<byte, string>
        {
            { 39, @"\'" },
            { 34, "\\\"" },
            { 92, @"\\" },
            { 10, @"\n" },
            { 13, @"\r" },
            { 9, @"\t" }
        };

        /// <summary>
        /// Returns <c>true</c> if the byte sequence can be interpreted as an ASCII string.
        /// </summary>
        /// <param name="data">The data to test. May not be <c>null</c>.</param>
        /// <returns><c>true</c> if the byte sequence can be interpreted as an ASCII string.</returns>
        public static bool IsAsciiString(this IEnumerable<byte> data)
        {
            Contract.Requires(data != null);

            foreach (byte ch in data)
            {
                if (ch != 9 && ch != 10 && ch != 13 && (ch < 32 || ch > 126))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a printable string of the byte sequence, interpreting it as ASCII if possible. If the sequence is interpreted as a string, the flattened, escaped string is returned enclosed in double-quotes; otherwise, each byte of the sequence is converted to hex, separated by spaces, and enclosed in square brackets.
        /// </summary>
        /// <param name="data">The data to dump. May not be <c>null</c>.</param>
        /// <returns>A printable string.</returns>
        public static string PrettyDump(this IEnumerable<byte> data)
        {
            Contract.Requires(data != null);

            if (data.IsAsciiString())
            {
                // Format the ASCII data as a string (escaped and flattened)
                return "\"" + string.Join(string.Empty, data.Select(ch => prettyEscapeSequences.ContainsKey(ch) ? prettyEscapeSequences[ch] : ((char)ch).ToString())) + "\"";
            }
            else
            {
                // Format the binary data as a byte array
                return "[" + string.Join(" ", data.Select(x => x.ToString("X2", NumberFormatInfo.InvariantInfo))) + "]";
            }
        }
    }
}
