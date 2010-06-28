using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Nito.KitchenSink.DelimitedText
{
    /// <summary>
    /// Parses an input stream into tokens.
    /// </summary>
    public sealed class Lexer : IEnumerable<Token>
    {
        /// <summary>
        /// The character used as a field separator.
        /// </summary>
        private readonly char fieldSeparator;

        /// <summary>
        /// The source data.
        /// </summary>
        private readonly IEnumerable<char> data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        /// <param name="data">The delimited text data.</param>
        /// <param name="fieldSeparator">The field separator character.</param>
        public Lexer(IEnumerable<char> data, char fieldSeparator = ',')
        {
            this.fieldSeparator = fieldSeparator;
            this.data = data;
        }

        /// <summary>
        /// Gets or sets the trace source to which messages are written during lexing.
        /// </summary>
        public TraceSource Tracer { get; set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Token> GetEnumerator()
        {
            var token = new Token();

            // Wrapping the source enumerator allows us to cache the return value of the IEnumerator.MoveNext method.
            using (var source = this.data.CreateEnumeratorWrapper())
            {
                while (!source.Done)
                {
                    // Read the current character.
                    var ch = source.Current;
                    if (ch == this.fieldSeparator)
                    {
                        // Consume the field separator character and publish a FieldSeparator token.
                        source.MoveNext();
                        token.Type = TokenType.FieldSeparator;
                        this.Information("Lexer: field separator.");
                        yield return token;
                    }
                    else if (ch == '\r')
                    {
                        // Consume the '\r' character.
                        source.MoveNext();

                        // Consume a '\n' character if it immediately follows the '\r'.
                        if (!source.Done && source.Current == '\n')
                        {
                            source.MoveNext();
                        }

                        // Publish an EndOfRecord token.
                        token.Type = TokenType.EndOfRecord;
                        this.Information("Lexer: end of record.");
                        yield return token;
                    }
                    else if (ch == '\n')
                    {
                        // Consume the '\n' character and publish an EndOfRecord token.
                        source.MoveNext();
                        token.Type = TokenType.EndOfRecord;
                        this.Information("Lexer: end of record.");
                        yield return token;
                    }
                    else
                    {
                        // At this point, the token must be FieldData, so prepare to read in the field data, one character at a time.
                        token.Type = TokenType.FieldData;
                        var sb = new StringBuilder();

                        if (ch != '\"')
                        {
                            // The field data is not escaped.
                            token.Data = this.ReadUnescapedFieldValue(source);
                            this.Information("Lexer: unescaped field data.");
                            yield return token;
                        }
                        else
                        {
                            // The field data is escaped.
                            token.Data = sb.ToString();
                            this.Information("Lexer: escaped field data.");
                            yield return token;
                        }
                    }
                }
            }

            this.Information("Lexer: no more tokens.");
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
        private void Information(string message)
        {
            if (this.Tracer != null)
            {
                this.Tracer.TraceEvent(TraceEventType.Information, 0, message);
            }
        }

        /// <summary>
        /// Traces the specified warning message.
        /// </summary>
        /// <param name="message">The warning message to trace.</param>
        private void Warning(string message)
        {
            if (this.Tracer != null)
            {
                this.Tracer.TraceEvent(TraceEventType.Warning, 0, message);
            }
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

            while (!source.Done)
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

            while (!source.Done)
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
            this.Warning("Lexer: Last entry in file is not complete; check for file truncation.");
            return sb.ToString();
        }
    }
}
