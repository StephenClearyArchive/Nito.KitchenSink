using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.CharacterStreams.DelimitedText
{
    using System.Diagnostics;

    /// <summary>
    /// Parses a token stream into records.
    /// </summary>
    public sealed class EnumerableParser : IEnumerable<List<string>>
    {
        /// <summary>
        /// The trace source to which messages are written during parsing.
        /// </summary>
        private readonly static TraceSource Tracer = new TraceSource("Nito.KitchenSink.CharacterStreams.DelimitedText.EnumerableParser");

        /// <summary>
        /// The underlying lexer.
        /// </summary>
        private readonly IEnumerable<Token> lexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableParser"/> class using a specified lexer.
        /// </summary>
        /// <param name="lexer">The lexer used to produce tokens.</param>
        public EnumerableParser(IEnumerable<Token> lexer)
        {
            this.lexer = lexer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableParser"/> class with the default lexer.
        /// </summary>
        /// <param name="data">The delimited text data.</param>
        /// <param name="fieldSeparator">The field separator character.</param>
        public EnumerableParser(IEnumerable<char> data, char fieldSeparator = ',')
        {
            this.lexer = new EnumerableLexer(data, fieldSeparator);
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

            // The last token that was read; this is null until after the first token is read.
            Token lastToken = null;

            // Read the tokens until there are no more.
            foreach (var token in this.lexer)
            {
                var fieldDataToken = token as Tokens.FieldData;

                if (token is Tokens.EndOfRecord)
                {
                    // An EndOfRecord may indicate the completion of an actual record of data, or just an empty line.

                    if (lastToken is Tokens.EndOfRecord)
                    {
                        // Ignore but warn about empty lines.
                        //  record.Count returns 0 at this point.
                        Warning("Parser: Empty line detected in file.");
                    }
                    else if (lastToken is Tokens.FieldSeparator)
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
                else if (token is Tokens.FieldSeparator)
                {
                    // A FieldSeparator following FieldData just separates fields; a FieldSeparator following a FieldSeparator or EndOfRecord implies an empty FieldData.

                    if (lastToken is Tokens.FieldData)
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
                else if (fieldDataToken != null)
                {
                    // Two FieldData tokens in a row is possible if there is data (even just whitespace) outside the quotes of a quoted FieldData.

                    if (lastToken is Tokens.FieldData)
                    {
                        // Just ignore extraneous data.
                        Warning("Parser: Extra data in field. Ignoring \"" + fieldDataToken.Data + "\".");
                    }
                    else
                    {
                        // Add FieldData to the record.
                        Information("Parser: Appending field data.");
                        record.Add(fieldDataToken.Data);
                    }
                }

                lastToken = token;
            }

            // The end of the file has been reached. There is a record to return only if a FieldData or FieldSeparator has been read.
            if (!(lastToken is Tokens.EndOfRecord))
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
