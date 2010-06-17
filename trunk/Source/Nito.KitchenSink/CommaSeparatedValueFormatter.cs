using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Provides RFC4180-compliant CSV formatting for a data stream.
    /// </summary>
    public sealed class CommaSeparatedValueFormatter
    {
        /// <summary>
        /// The stream of data to format as CSV.
        /// </summary>
        private readonly IEnumerable<IEnumerable<string>> data;

        /// <summary>
        /// The headers to write to the CSV, if any.
        /// </summary>
        private readonly IEnumerable<string> headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaSeparatedValueFormatter"/> class.
        /// </summary>
        /// <param name="data">The stream of data to format as CSV.</param>
        /// <param name="headers">The headers to write to the CSV, if any. This parameter may be <c>null</c>.</param>
        public CommaSeparatedValueFormatter(IEnumerable<IEnumerable<string>> data, IEnumerable<string> headers = null)
        {
            this.data = data;
            this.headers = headers;
        }

        /// <summary>
        /// Writes the data as a CSV stream to the specified writer.
        /// </summary>
        /// <param name="writer">The writer to which to write the CSV stream.</param>
        public void Write(TextWriter writer)
        {
            if (this.headers != null)
            {
                writer.WriteLine(string.Join(",", this.headers.Select(EscapeFieldValue)));
            }

            foreach (var record in this.data)
            {
                writer.WriteLine(string.Join(",", record.Select(EscapeFieldValue)));
            }
        }

        /// <summary>
        /// Escapes a field value.
        /// </summary>
        /// <param name="fieldValue">The field value to escape.</param>
        /// <returns>The escaped field value.</returns>
        private static string EscapeFieldValue(string fieldValue)
        {
            return "\"" + fieldValue.Replace("\"", "\"\"") + "\"";
        }
    }
}
