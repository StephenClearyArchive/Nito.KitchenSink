using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Parses RFC4180-compliant CSV streams.
    /// </summary>
    /// <remarks>
    /// This class supports three ways to define the headers for a CSV stream:
    ///   1) Passed as a constructor parameter. If there is a header row in the data, the number of fields must match (but the names are not checked).
    ///   2) Dynamic, determined by the header row in the data.
    ///   3) Unknown. The number of fields must be the same for each record, but the names of the fields are unknown.
    /// </remarks>
    public sealed class CommaSeparatedValueParser : IDisposable
    {
        /// <summary>
        /// The stream from which to read.
        /// </summary>
        private readonly PutbackBufferTextReader<LineMappingTextReader<TextReader>> reader;

        /// <summary>
        /// Whether the stream has a header row.
        /// </summary>
        private readonly bool headerRow;

        /// <summary>
        /// Whether the field names are known.
        /// </summary>
        private readonly bool fieldNamesKnown;

        /// <summary>
        /// The number of fields per record.
        /// </summary>
        private int fieldCount;

        /// <summary>
        /// The defined headers, if any. This is only valid if <c>fieldNamesKnown</c> is <c>true</c>.
        /// </summary>
        private readonly List<string> headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaSeparatedValueParser"/> class.
        /// </summary>
        /// <param name="reader">The reader containing the CSV data.</param>
        /// <param name="headerRow">If set to <c>true</c>, the CSV data contains a header row.</param>
        /// <param name="headers">The headers for the CSV data. This parameter may be <c>null</c>.</param>
        public CommaSeparatedValueParser(TextReader reader, bool headerRow = true, IEnumerable<string> headers = null)
        {
            this.reader = reader.ApplyLineMapping().ApplyPutbackBuffer();
            this.headerRow = headerRow;
            this.fieldNamesKnown = headerRow || headers != null;
            if (this.fieldNamesKnown)
            {
                if (headers == null)
                {
                    this.headers = new List<string>();
                }
                else
                {
                    this.headers = headers.ToList();
                    this.fieldCount = this.headers.Count;
                }
            }
        }

        /// <summary>
        /// Reads the CSV records. Each record will contain the same number of fields.
        /// </summary>
        /// <returns>The CSV records.</returns>
        public IEnumerable<List<string>> Read()
        {
            // Read in the header row first if necessary
            if (this.headerRow)
            {
                this.ReadHeaderRow();
            }

            var ret = this.ReadLine().ToList();
            if (ret.Count == 0)
            {
                throw new InvalidDataException("CSV file has no records" + this.LocationString());
            }

            while (ret.Count != 0)
            {
                if (this.fieldCount == 0)
                {
                    // We have unknown field names; just keep track of how many there are.
                    this.fieldCount = ret.Count;
                }

                if (ret.Count != this.fieldCount)
                {
                    throw new InvalidDataException("Invalid number of fields in CSV record" + this.LocationString());
                }

                yield return ret;
                ret = this.ReadLine().ToList();
            }
        }

        /// <summary>
        /// Reads the CSV records. Each record will contain the same fields, exposed as properties on a dynamic object.
        /// </summary>
        /// <returns>The CSV records.</returns>
        public IEnumerable<dynamic> ReadDynamic()
        {
            foreach (var record in this.Read())
            {
                var ret = new ExpandoObject() as IDictionary<string, object>;
                int fieldIndex = 0;
                foreach (var value in record)
                {
                    string fieldName = this.fieldNamesKnown ? this.headers[fieldIndex] : "Field" + fieldIndex;
                    ret.Add(fieldName, value);
                    ++fieldIndex;
                }

                yield return ret;
            }
        }

        /// <summary>
        /// Disposes the underlying <see cref="TextReader"/>.
        /// </summary>
        public void Dispose()
        {
            this.reader.Dispose();
        }

        /// <summary>
        /// Reads the header row, either populating <see cref="headers"/> or comparing the actual headers with the user-provided <see cref="headers"/>.
        /// </summary>
        private void ReadHeaderRow()
        {
            if (this.fieldCount == 0)
            {
                // Read the header definitions from the CSV stream.
                this.headers.AddRange(this.ReadLine());
                this.fieldCount = this.headers.Count;
                if (this.fieldCount == 0)
                {
                    throw new InvalidDataException("CSV stream has no records" + this.LocationString());
                }
            }
            else
            {
                // Read the header definitions from the CSV stream and ensure that there are the same number of fields as expected.
                var headersReadCount = this.ReadLine().Count();
                if (headersReadCount != this.fieldCount)
                {
                    throw new InvalidDataException("CSV header row contains " + headersReadCount + " headers; expected " + this.fieldCount + this.LocationString());
                }
            }
        }

        /// <summary>
        /// Reads a single record from the CSV stream. Returns an empty sequence if the stream is at EOF. Returns a single-element sequence containing an empty string if the stream is at a blank line.
        /// </summary>
        /// <returns>The CSV record.</returns>
        private IEnumerable<string> ReadLine()
        {
            var field = this.ReadField();
            while (field != null)
            {
                yield return field;

                // After calling ReadField, the stream must be at EOF, on a '\r', or on a ','. Any other value is an error.
                int nextCh = this.reader.Read();
                if (nextCh == -1)
                {
                    // The stream is at EOF, ending this line.
                    yield break;
                }

                if (nextCh == '\r')
                {
                    // Ensure that the stream has a complete record separator.
                    nextCh = this.reader.Read();
                    if (nextCh != '\n')
                    {
                        throw new InvalidDataException("Incomplete record terminator in CSV record" + this.LocationString());
                    }

                    // The stream reached a record separator.
                    yield break;
                }

                if (nextCh == ',')
                {
                    // Continue by reading the next field.
                    field = this.ReadField();
                    continue;
                }

                // Other values indicate junk at the end of an escaped field value.
                throw new InvalidDataException("CSV field value has extra data" + this.LocationString());
            }

            // If ReadField returns null, then we're at EOF, so the record ends too.
        }

        /// <summary>
        /// Reads a single field from the underlying <see cref="TextReader"/>. Returns <c>null</c> if the reader is at the end of the stream. When this function returns, the stream should be at EOF, on a '\r', or on a ','; any other character is an error in the input.
        /// </summary>
        /// <returns>A single field value, unescaped.</returns>
        private string ReadField()
        {
            var ret = new StringBuilder();
            var escaped = false;

            var ch = this.reader.Read();
            if (ch == -1)
            {
                // The first character read is at the end of the stream; return "null".
                // On return, the stream is at EOF.
                return null;
            }

            if ((char)ch == '\"')
            {
                // The first character read is a DQUOTE, so this field is escaped.
                escaped = true;
            }
            else
            {
                // Put the first character back into the stream; it is either ',' or '\r' (indicating an empty field), or part of the field value.
                this.reader.Putback((char)ch);
            }

            while (true)
            {
                // Read the next character in the field value.
                ch = this.reader.Read();

                if (escaped)
                {
                    if (ch == -1)
                    {
                        // Reached EOF without a closing DQUOTE.
                        throw new InvalidDataException("EOF while parsing escaped CSV field" + this.LocationString());
                    }

                    if ((char)ch == '\"')
                    {
                        // DQUOTE found within escaped field; determine if it is a 2DQUOTE.
                        int nextCh = this.reader.Peek();
                        if (nextCh != -1 && (char)nextCh == '\"')
                        {
                            // It is in fact a 2DQUOTE, so unescape it into a single DQUOTE.
                            this.reader.Read();
                            ret.Append((char)ch);
                        }
                        else
                        {
                            // The DQUOTE is not the first character of a 2DQUOTE, so it marks the end of this field.
                            // On return, the stream is either at EOF, on a '\r' (as the first character of a record terminator), or on a ',' (as a field separator); any other value should be treated as an error.
                            return ret.ToString();
                        }
                    }
                    else
                    {
                        // The character is not a DQUOTE, so we're still escaped (all characters are treated as part of the field value).
                        ret.Append((char)ch);
                    }
                }
                else
                {
                    if (ch == -1)
                    {
                        // EOF in a non-escaped field: just ends the field value (and the record value).
                        // We know there is at least one character in 'ret' because of the code before the 'while' loop.
                        // On return, the stream is at EOF.
                        return ret.ToString();
                    }

                    if ((char)ch == '\"' || (char)ch == '\n')
                    {
                        // A non-escaped field cannot contain these characters.
                        throw new InvalidDataException("Non-escaped CSV field contains invalid character" + this.LocationString());
                    }

                    if ((char)ch == '\r')
                    {
                        // Back out the record terminator.
                        // On return, the stream is on a '\r' (as the first character of a record terminator).
                        this.reader.Putback((char)ch);
                        return ret.ToString();
                    }

                    if ((char)ch == ',')
                    {
                        // Comma found: end this field value and put the comma back. The field value may be empty.
                        // On return, the stream is on a ',' (as a field separator).
                        this.reader.Putback((char)ch);
                        return ret.ToString();
                    }

                    ret.Append((char)ch);
                }
            }
        }

        /// <summary>
        /// Returns the current stream location (line and line offset) as a string.
        /// </summary>
        /// <returns>The current stream location (line and line offset) as a string.</returns>
        private string LocationString()
        {
            var location = this.reader.BaseReader.GetLineNumberAndLineOffset(this.reader.BaseReader.BaseReader.Position - this.reader.BufferCount);
            return " at line " + location.Item1 + ", offset " + location.Item2 + ".";
        }
    }
}
