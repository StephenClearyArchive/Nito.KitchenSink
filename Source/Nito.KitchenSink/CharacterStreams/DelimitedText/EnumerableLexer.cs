﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Nito.KitchenSink.CharacterStreams.DelimitedText
{
    /// <summary>
    /// Parses an input stream into tokens.
    /// </summary>
    public sealed class EnumerableLexer : IEnumerable<Token>
    {
        /// <summary>
        /// The trace source to which messages are written during lexing.
        /// </summary>
        private readonly static TraceSource Tracer = new TraceSource("Nito.KitchenSink.CharacterStreams.DelimitedText.EnumerableLexer");

        /// <summary>
        /// The character used as a field separator.
        /// </summary>
        private readonly char fieldSeparator;

        /// <summary>
        /// The source data.
        /// </summary>
        private readonly IEnumerable<char> data;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableLexer"/> class.
        /// </summary>
        /// <param name="data">The delimited text data.</param>
        /// <param name="fieldSeparator">The field separator character.</param>
        public EnumerableLexer(IEnumerable<char> data, char fieldSeparator = ',')
        {
            this.fieldSeparator = fieldSeparator;
            this.data = data;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Token> GetEnumerator()
        {
            // Wrapping the source enumerator allows us to cache the return value of the IEnumerator.MoveNext method.
            using (var source = this.data.CreateEnumeratorWrapper())
            {
                while (source.More)
                {
                    // Read the current character.
                    var ch = source.Current;
                    if (ch == this.fieldSeparator)
                    {
                        // Consume the field separator character and publish a FieldSeparator token.
                        source.MoveNext();
                        Information("Lexer: field separator.");
                        yield return new Tokens.FieldSeparator();
                    }
                    else if (ch == '\r')
                    {
                        // Consume the '\r' character.
                        source.MoveNext();

                        // Consume a '\n' character if it immediately follows the '\r'.
                        if (source.More && source.Current == '\n')
                        {
                            source.MoveNext();
                        }

                        // Publish an EndOfRecord token.
                        Information("Lexer: end of record.");
                        yield return new Tokens.EndOfRecord();
                    }
                    else if (ch == '\n')
                    {
                        // Consume the '\n' character and publish an EndOfRecord token.
                        source.MoveNext();
                        Information("Lexer: end of record.");
                        yield return new Tokens.EndOfRecord();
                    }
                    else
                    {
                        // At this point, the token must be FieldData, so read in the field data, one character at a time.

                        if (ch != '\"')
                        {
                            // The field data is not escaped.
                            Information("Lexer: unescaped field data.");
                            yield return new Tokens.FieldData { Data = this.ReadUnescapedFieldValue(source) };
                        }
                        else
                        {
                            // The field data is escaped.
                            Information("Lexer: escaped field data.");
                            yield return new Tokens.FieldData { Data = this.ReadEscapedFieldValue(source) };
                        }
                    }
                }
            }

            Information("Lexer: no more tokens.");
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Traces the specified informational message.
        /// </summary>
        /// <param name="message">The informational message to trace.</param>
        private static void Information(string message)
        {
            Tracer.TraceEvent(TraceEventType.Information, 0, message);
        }

        /// <summary>
        /// Traces the specified warning message.
        /// </summary>
        /// <param name="message">The warning message to trace.</param>
        private static void Warning(string message)
        {
            Tracer.TraceEvent(TraceEventType.Warning, 0, message);
        }

        /// <summary>
        /// Reads a single unescaped field data value from the source. The source must be currently positioned on the first character of the field data. On return, the source is either at the end of the input or is positioned on the first character that is not part of the field data.
        /// </summary>
        /// <param name="source">The source from which to read.</param>
        /// <returns>The field data value.</returns>
        private string ReadUnescapedFieldValue(EnumeratorWrapper<char> source)
        {
            var sb = new StringBuilder();

            // Consume the first character of the field data.
            sb.Append(source.Current);
            source.MoveNext();

            while (source.More)
            {
                var ch = source.Current;
                if (ch == this.fieldSeparator || ch == '\r' || ch == '\n')
                {
                    // Unescaped field data can be terminated by a FieldSeparator or EndOfRecord.
                    // Publish the FieldData token, but do not consume the characters for the FieldSeparator or EndOfRecord tokens.
                    return sb.ToString();
                }

                // Consume the next character of the field data.
                sb.Append(ch);
                source.MoveNext();
            }

            // The while loop was exited by reaching the end of the source sequence, so publish the last FieldData token.
            return sb.ToString();
        }

        /// <summary>
        /// Reads a single escaped field data value from the source. The source must be currently positioned on the opening double quote of the field data. On return, the source is either at the end of the input or is positioned on the first character that is not part of the field data (past the closing double quote).
        /// </summary>
        /// <param name="source">The source from which to read.</param>
        /// <returns>The field data value.</returns>
        private string ReadEscapedFieldValue(EnumeratorWrapper<char> source)
        {
            var sb = new StringBuilder();

            // Consume the '\"' character.
            source.MoveNext();

            while (source.More)
            {
                var ch = source.Current;
                if (ch == '\"')
                {
                    // A '\"' character within escaped field data may be the end of the escaped field data or it may be the first character of a 2DQUOTE escape sequence.
                    source.MoveNext();
                    if (source.Done || source.Current != '\"')
                    {
                        // The '\"' was the end of the escaped field data.
                        return sb.ToString();
                    }
                }

                // Consume the next character of the field data.
                sb.Append(ch);
                source.MoveNext();
            }

            // The while loop was exited by reaching the end of the source sequence, so publish the last FieldData token (with a warning, since the ending double quote was never found).
            Warning("Lexer: Last entry in file is not complete; check for file truncation.");
            return sb.ToString();
        }
    }
}
