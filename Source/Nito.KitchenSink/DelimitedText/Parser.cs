using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.DelimitedText
{
    using System.Diagnostics;

    /// <summary>
    /// Parses a token stream into records.
    /// </summary>
    public sealed class Parser : IEnumerable<List<string>>
    {
        /// <summary>
        /// The trace source to which messages are written during parsing.
        /// </summary>
        private readonly static TraceSource Tracer = new TraceSource("Nito.KitchenSink.DelimitedText.Parser");

        /// <summary>
        /// The underlying lexer.
        /// </summary>
        private readonly IEnumerable<Token> lexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class using a specified lexer.
        /// </summary>
        /// <param name="lexer">The lexer used to produce tokens.</param>
        public Parser(IEnumerable<Token> lexer)
        {
            this.lexer = lexer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class with the default lexer.
        /// </summary>
        /// <param name="data">The delimited text data.</param>
        /// <param name="fieldSeparator">The field separator character.</param>
        public Parser(IEnumerable<char> data, char fieldSeparator = ',')
        {
            this.lexer = new Lexer(data, fieldSeparator);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<List<string>> GetEnumerator()
        {
            var record = new List<string>();

            // The last token that was read; this stays set to EndOfRecord until a FieldData or FieldSeparator is read. It never gets set back to EndOfRecord.
            TokenType lastTokenType = TokenType.EndOfRecord;

            // Read the tokens until there are no more.
            foreach (var token in this.lexer)
            {
                if (token.Type == TokenType.EndOfRecord)
                {
                    // An EndOfRecord may indicate the completion of an actual record of data, or just an empty line.

                    if (lastTokenType == TokenType.EndOfRecord)
                    {
                        // Ignore but warn about empty lines.
                        //  record.Count returns 0 at this point.
                        Warning("Parser: Empty line detected in file.");
                    }
                    else if (lastTokenType == TokenType.FieldSeparator)
                    {
                        // A FieldSeparator followed by an EndOfRecord implies an empty field at the end of the record.
                        record.Add(string.Empty);
                    }
                    else
                    {
                        // The record has ended, so we return it to the caller.
                        //  record.Count is > 0 at this point.
                        Information("Parser: End of record (with data).");
                        yield return record;
                        record.Clear();
                    }
                }
                else if (token.Type == TokenType.FieldSeparator)
                {
                    // A FieldSeparator following FieldData just separates fields; a FieldSeparator following a FieldSeparator or EndOfRecord implies an empty FieldData.

                    if (lastTokenType == TokenType.FieldData)
                    {
                        // Since this FieldSepartor is following FieldData, it should just be ignored.
                        Information("Parser: Ignoring FieldSeparator following FieldData.");
                    }
                    else
                    {
                        // Since this FieldSeparator is following a FieldSeparator or EndOfRecord, an empty FieldData is implied.
                        Information("Parser: Appending empty field data.");
                        record.Add(string.Empty);
                    }
                }
                else // (token.Type == TokenType.FieldData)
                {
                    // Two FieldData tokens in a row is possible if there is data (even just whitespace) outside the quotes of a quoted FieldData.

                    if (lastTokenType == TokenType.FieldData)
                    {
                        // Just ignore extraneous data.
                        Warning("Parser: Extra data in field. Ignoring \"" + token.Data + "\".");
                    }
                    else
                    {
                        // Add FieldData to the record.
                        Information("Parser: Appending field data.");
                        record.Add(token.Data);
                    }
                }

                lastTokenType = token.Type;
            }

            // The end of the file has been reached. There is a record to return only if a FieldData or FieldSeparator has been read.
            if (lastTokenType != TokenType.EndOfRecord)
            {
                yield return record;
            }
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
    }
}
