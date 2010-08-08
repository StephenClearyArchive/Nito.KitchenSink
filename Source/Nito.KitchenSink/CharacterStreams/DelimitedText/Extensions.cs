using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.CharacterStreams.DelimitedText
{
    /// <summary>
    /// Extension methods for working with delimited text.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Lexes a sequence of characters, producing a sequence of tokens.
        /// </summary>
        /// <param name="source">The source stream of characters.</param>
        /// <param name="fieldSeparator">The field separator to use. The default value is <c>,</c>.</param>
        /// <returns>An enumerable sequence of tokens.</returns>
        public static IEnumerable<Token> LexDelimitedText(this IEnumerable<char> source, char fieldSeparator = ',')
        {
            return new EnumerableLexer(source, fieldSeparator);
        }

        /// <summary>
        /// Parses a sequence of tokens, producing a sequence of records.
        /// </summary>
        /// <param name="source">The source stream of tokens.</param>
        /// <returns>An enumerable sequence of records.</returns>
        public static IEnumerable<List<string>> ParseDelimitedText(this IEnumerable<Token> source)
        {
            return new EnumerableParser(source);
        }

        /// <summary>
        /// Lexes and parses a sequence of characters, producing a sequence of records.
        /// </summary>
        /// <param name="source">The source stream of characters.</param>
        /// <param name="fieldSeparator">The field separator to use. The default value is <c>,</c>.</param>
        /// <returns>An enumerable sequence of records.</returns>
        public static IEnumerable<List<string>> ParseDelimitedText(this IEnumerable<char> source, char fieldSeparator = ',')
        {
            return new EnumerableParser(source, fieldSeparator);
        }
    }
}
