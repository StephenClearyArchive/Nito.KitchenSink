// <copyright file="StringExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
using System.Text.RegularExpressions;

    /// <summary>
    /// Provides useful extension methods for string operations.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// The standard escape sequences defined by the C# language.
        /// </summary>
        private static Dictionary<char, string> csharpEscapeSequences = new Dictionary<char, string>
        {
            { '\'', @"\'" },
            { '\"', "\\\"" },
            { '\\', @"\\" },
            { '\0', @"\0" },
            { '\a', @"\a" },
            { '\b', @"\b" },
            { '\f', @"\f" },
            { '\n', @"\n" },
            { '\r', @"\r" },
            { '\t', @"\t" },
            { '\v', @"\v" }
        };

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
        /// Returns a flattened, printable C#-escaped equivalent of the input string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>A backslash-escaped copy of the source string.</returns>
        public static string PrintableEscape(this string source)
        {
            StringBuilder ret = new StringBuilder(source.Length);
            foreach (string str in source.TextElements())
            {
                // Almost always, a text element is a single character
                if (str.Length == 1)
                {
                    // If the text element needs escaping, then escape it
                    if (csharpEscapeSequences.ContainsKey(str[0]))
                    {
                        ret.Append(csharpEscapeSequences[str[0]]);
                        continue;
                    }

                    // Pass the char through if it's already printable
                    if (str[0] >= 32 && str[0] <= 126)
                    {
                        ret.Append(str[0]);
                        continue;
                    }
                }

                // Surrogate characters, combining characters, and non-printable ASCII characters end up here
                foreach (char ch in str)
                {
                    // For these situations, we use C#'s UTF-16 Unicode escape sequence
                    ret.Append(@"\u" + ((ushort)ch).ToString("X4", NumberFormatInfo.InvariantInfo));
                }
            }

            return ret.ToString();
        }

        /// <summary>
        /// Returns the string itself if it is printable and flattened, otherwise a <see cref="PrintableEscape"/> copy of the string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>The source string or a backslash-escaped copy of the source string.</returns>
        public static string Printable(this string source)
        {
            foreach (char ch in source)
            {
                if (ch < 32 || ch > 126)
                {
                    return source.PrintableEscape();
                }
            }

            return source;
        }

        /// <summary>
        /// Returns a printable string of the byte sequence, interpreting it as ASCII if possible. If the sequence is interpreted as a string, the flattened, escaped string is returned enclosed in double-quotes; otherwise, each byte of the sequence is converted to hex, separated by spaces, and enclosed in square brackets.
        /// </summary>
        /// <param name="data">The data to dump.</param>
        /// <returns>A printable string.</returns>
        public static string PrettyDump(this IEnumerable<byte> data)
        {
            // First determine if the array probably contains ASCII data
            bool pretty = true;
            foreach (byte ch in data)
            {
                if (ch != 9 && ch != 10 && ch != 13 && (ch < 32 || ch > 126))
                {
                    pretty = false;
                    break;
                }
            }

            if (pretty)
            {
                // Format the ASCII data as a string (escaped and flattened)
                return "\"" + data.Select(ch => prettyEscapeSequences.ContainsKey(ch) ? prettyEscapeSequences[ch] : ((char)ch).ToString()).Join() + "\"";
            }
            else
            {
                // Format the binary data as a byte array
                return "[" + data.Select(x => x.ToString("X2", NumberFormatInfo.InvariantInfo)).Join(" ") + "]";
            }
        }

        /// <summary>
        /// Replaces any '\r' or '\n' characters in the string with spaces.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>The flattened string.</returns>
        public static string Flatten(this string source)
        {
            StringBuilder ret = new StringBuilder(source.Length);
            foreach (char ch in source)
            {
                if (ch == '\r' || ch == '\n')
                {
                    ret.Append(' ');
                }
                else
                {
                    ret.Append(ch);
                }
            }

            return ret.ToString();
        }

        /// <summary>
        /// Gets all text elements (Unicode glyphs) for a given string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>All the text elements in the source string.</returns>
        public static IEnumerable<string> TextElements(this string source)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(source);
            while (enumerator.MoveNext())
            {
                yield return enumerator.GetTextElement();
            }
        }

        /// <summary>
        /// Concatenates a separator between each element of a string enumeration.
        /// </summary>
        /// <param name="source">The string enumeration.</param>
        /// <param name="separator">The separator string. This may not be null.</param>
        /// <returns>The concatenated string.</returns>
        public static string Join(this IEnumerable<string> source, string separator)
        {
            StringBuilder ret = new StringBuilder();
            bool first = true;
            foreach (string str in source)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    ret.Append(separator);
                }

                ret.Append(str);
            }

            return ret.ToString();
        }

        /// <summary>
        /// Concatenates a sequence of strings.
        /// </summary>
        /// <param name="source">The sequence of strings.</param>
        /// <returns>The concatenated string.</returns>
        public static string Join(this IEnumerable<string> source)
        {
            return source.Join(string.Empty);
        }

        private static readonly Regex WhitespaceRegex = new Regex(@"\s+");

        /// <summary>
        /// Collapses whitespace sequences in a string to a single space. The string is flattened as a side effect.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>The string with whitespace collapsed.</returns>
        public static string CollapseWhitespace(this string source)
        {
            return WhitespaceRegex.Replace(source, " ");
        }
    }
}
