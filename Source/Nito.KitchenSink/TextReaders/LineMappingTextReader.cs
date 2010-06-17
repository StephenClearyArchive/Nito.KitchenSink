using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nito.KitchenSink
{
    /// <summary>
    /// A wrapper around a source <see cref="TextReader"/>, counting lines and columns. This class does not perform any buffering.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="TextReader"/> being wrapped.</typeparam>
    public sealed class LineMappingTextReader<T> : SourceTextReader<PositionTrackingTextReader<T>> where T : TextReader
    {
        /// <summary>
        /// The offset of each '\n' line ending seen in the source stream.
        /// </summary>
        private List<long> lineEndings = new List<long>() { -1 };

        /// <summary>
        /// Initializes a new instance of the <see cref="LineMappingTextReader{T}"/> class, wrapping the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="source">The source <see cref="TextReader"/> to wrap.</param>
        public LineMappingTextReader(T source)
            : base(source.ApplyPositionTracking())
        {
        }

        /// <summary>
        /// Gets the line number and line offset for a speicified position in the stream.
        /// </summary>
        /// <param name="position">The position in the stream for which to get the line number and line offset. This should not be negative or larger than the number of characters read.</param>
        /// <returns>The line number and line offset for the specified position in the stream.</returns>
        public Tuple<int, long> GetLineNumberAndLineOffset(long position)
        {
            int index = this.lineEndings.BinarySearch(position);
            if (index > 0)
            {
                return Tuple.Create(index - 1, (long)0);
            }

            int lineNumber = ~index - 1;
            return Tuple.Create(lineNumber, position - this.lineEndings[lineNumber]);
        }

        /// <summary>
        /// Reads the next character from the input stream and advances the character position by one character.
        /// </summary>
        /// <returns>The next character from the input stream, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Read()
        {
            int ret = base.Read();

            if (ret == -1)
            {
                return ret;
            }

            if (ret == '\n')
            {
                this.lineEndings.Add(this.BaseReader.Position);
            }

            return ret;
        }
    }
}
