using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Nito.KitchenSink
{
    /// <summary>
    /// A wrapper around a source <see cref="TextReader"/>, providing stream position. This class does not perform buffering.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="TextReader"/> being wrapped.</typeparam>
    public sealed class PositionTrackingTextReader<T> : SourceTextReader<T> where T : TextReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PositionTrackingTextReader&lt;T&gt;"/> class wrapping the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="source">The source <see cref="TextReader"/> to wrap.</param>
        public PositionTrackingTextReader(T source)
            : base(source)
        {
        }

        /// <summary>
        /// Gets the position in the stream (the number of characters that have been read).
        /// </summary>
        public long Position { get; private set; }

        /// <summary>
        /// Reads the next character from the input stream and advances the character position by one character.
        /// </summary>
        /// <returns>The next character from the input stream, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Read()
        {
            int ret = base.Read();
            if (ret != -1)
            {
                ++this.Position;
            }

            return ret;
        }
    }
}
