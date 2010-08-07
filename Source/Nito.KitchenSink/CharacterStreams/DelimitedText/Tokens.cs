using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.CharacterStreams.DelimitedText
{
    /// <summary>
    /// Token types read from delimited text data.
    /// </summary>
    public static class Tokens
    {
        /// <summary>
        /// Field data token, representing field data in the original file. Field data supports CSV-style quoting, which is unquoted during lexing.
        /// </summary>
        public sealed class FieldData : StringToken
        {
        }

        /// <summary>
        /// A field separator.
        /// </summary>
        public sealed class FieldSeparator : Token
        {
        }

        /// <summary>
        /// An end of record indicator: '\r\n', '\n', or '\r'. An end of record indicator will not appear at the end of the input.
        /// </summary>
        public sealed class EndOfRecord : Token
        {
        }
    }
}
