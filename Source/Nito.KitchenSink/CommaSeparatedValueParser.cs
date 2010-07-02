using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

namespace Nito.KitchenSink
{
    using System.Diagnostics;

    /// <summary>
    /// Parses RFC4180-compliant CSV streams.
    /// </summary>
    /// <remarks>
    /// This class supports three ways to define the headers for a CSV stream:
    ///   1) Passed as a constructor parameter. If there is a header row in the data, the number of fields must match (but the names are not checked).
    ///   2) Dynamic, determined by the header row in the data.
    ///   3) Unknown. The number of fields must be the same for each record, but the names of the fields are unknown.
    /// </remarks>
    public sealed class CommaSeparatedValueParser
    {
        /// <summary>
        /// The stream from which to read.
        /// </summary>
        private readonly IEnumerable<List<string>> parser;

        /// <summary>
        /// Whether the stream has a header row that hasn't been read yet.
        /// </summary>
        private bool headerRow;

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
        /// <param name="data">The CSV data to parse.</param>
        /// <param name="headerRow">If set to <c>true</c>, the CSV data contains a header row.</param>
        /// <param name="headers">The headers for the CSV data. This parameter may be <c>null</c>.</param>
        public CommaSeparatedValueParser(string data, bool headerRow = true, IEnumerable<string> headers = null)
            :this(new DelimitedText.Parser(data), headerRow, headers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaSeparatedValueParser"/> class.
        /// </summary>
        /// <param name="parser">The parser providing the CSV data.</param>
        /// <param name="headerRow">If set to <c>true</c>, the CSV data contains a header row.</param>
        /// <param name="headers">The headers for the CSV data. This parameter may be <c>null</c>.</param>
        public CommaSeparatedValueParser(IEnumerable<List<string>> parser, bool headerRow = true, IEnumerable<string> headers = null)
        {
            this.parser = parser;
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
            using (var source = this.parser.CreateEnumeratorWrapper())
            {
                while (!source.Done)
                {
                    // Read in the header row first if necessary
                    if (this.headerRow)
                    {
                        this.ReadHeaderRow(source);
                        source.MoveNext();
                        this.headerRow = false;
                        continue;
                    }

                    var ret = source.Current;

                    if (this.fieldCount == 0)
                    {
                        // We have unknown field names; just keep track of how many there are.
                        this.fieldCount = ret.Count;
                    }

                    if (ret.Count != this.fieldCount)
                    {
                        throw new InvalidDataException("CSV record had " + ret.Count + " fields; expected " + this.fieldCount + ".");
                    }

                    yield return ret;
                    source.MoveNext();
                }
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
                yield return new ExpandoObject().AddProperties(record, this.headers);
            }
        }

        /// <summary>
        /// Reads the header row, either populating <see cref="headers"/> or comparing the actual headers with the user-provided <see cref="headers"/>.
        /// </summary>
        private void ReadHeaderRow(EnumeratorWrapper<List<string>> source)
        {
            if (this.fieldCount == 0)
            {
                // Read the header definitions from the CSV stream.
                this.headers.AddRange(source.Current);
                this.fieldCount = this.headers.Count;
            }
            else
            {
                // Read the header definitions from the CSV stream and ensure that there are the same number of fields as expected.
                var headersReadCount = source.Current.Count;
                if (headersReadCount != this.fieldCount)
                {
                    throw new InvalidDataException("CSV header row contains " + headersReadCount + " headers; expected " + this.fieldCount + ".");
                }
            }
        }
    }
}
