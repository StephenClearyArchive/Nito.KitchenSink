using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.DelimitedText
{
    /// <summary>
    /// The type of token read.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Field data token, representing field data in the original file. Field data supports CSV-style quoting, which is unquoted during lexing.
        /// </summary>
        FieldData,

        /// <summary>
        /// A field separator.
        /// </summary>
        FieldSeparator,

        /// <summary>
        /// An end of record indicator: '\r\n', '\n', or '\r'. An end of record indicator may not appear at the end of the file.
        /// </summary>
        EndOfRecord,
    }

    /// <summary>
    /// A token read from the file.
    /// </summary>
    public sealed class Token
    {
        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        public TokenType Type { get; set; }

        /// <summary>
        /// Gets or sets the field data of the token. This is <c>null</c> unless <see cref="Type"/> is <see cref="TokenType.FieldData"/>.
        /// </summary>
        public string Data { get; set; }
    }
}
