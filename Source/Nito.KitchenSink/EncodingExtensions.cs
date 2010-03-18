using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Extension methods for the <see cref="Encoding"/> class.
    /// </summary>
    public static class EncodingExtensions
    {
        /// <summary>
        /// Decodes all bytes in a stream into a string.
        /// </summary>
        /// <param name="encoding">The character encoding to fall back to, if there are no byte order marks or if <paramref name="detectEncodingFromByteOrderMarks"/> is <c>false</c>.</param>
        /// <param name="stream">The stream to be read.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Whether to look for byte order marks at the current position of the stream.</param>
        /// <returns>The data from the stream, interpreted as a string according to the specified encoding.</returns>
        public static string GetString(this Encoding encoding, Stream stream, bool detectEncodingFromByteOrderMarks)
        {
            using (StreamReader reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
