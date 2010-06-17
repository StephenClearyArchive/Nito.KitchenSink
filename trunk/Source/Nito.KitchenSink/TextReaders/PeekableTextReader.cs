namespace Nito.KitchenSink
{
    using System.IO;

    /// <summary>
    /// A wrapper around a source <see cref="TextReader"/>, providing a single-character buffer. <see cref="Peek"/> only returns <c>-1</c> if the end of the stream is reached. This class does perform buffering.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="TextReader"/> being wrapped.</typeparam>
    public sealed class PeekableTextReader<T> : SourceTextReader<T> where T : TextReader
    {
        /// <summary>
        /// The single-character buffer, holding the next character that has been "peeked." If the source <see cref="TextReader"/> supports peeking, then this member is always <c>null</c>.
        /// </summary>
        private char? buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeekableTextReader{T}"/> class wrapping the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="source">The source <see cref="TextReader"/> to wrap.</param>
        public PeekableTextReader(T source)
            : base(source)
        {
        }

        /// <summary>
        /// Gets the buffer count.
        /// </summary>
        public int BufferCount
        {
            get { return this.buffer.HasValue ? 1 : 0; }
        }

        /// <summary>
        /// Reads the next character without changing the state of the reader or the character source. Returns the next available character without actually reading it from the input stream.
        /// </summary>
        /// <returns>An integer representing the next character to be read, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Peek()
        {
            if (this.buffer.HasValue)
            {
                // This character has already been peeked and the source stream does not support peeking, so it's actually been read from the source stream.
                return this.buffer.Value;
            }

            // Attempt to just pass the peek request to the source stream.
            int ret = this.source.Peek();
            if (ret != -1)
            {
                // The source stream supports peeking.
                return ret;
            }

            // Determine why the source stream returned -1: either it's at the end, or it does not support peeking.
            int value = this.source.Read();
            if (value == -1)
            {
                // The source stream has actually ended.
                return -1;
            }

            // The source stream does not support peeking.
            this.buffer = (char)value;
            return value;
        }

        /// <summary>
        /// Reads the next character from the input stream and advances the character position by one character.
        /// </summary>
        /// <returns>The next character from the input stream, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Read()
        {
            if (this.buffer.HasValue)
            {
                // This character has already been peeked and the source stream does not support peeking, so it's actually been read from the source stream.
                int ret = this.buffer.Value;
                this.buffer = null;
                return ret;
            }

            return this.source.Read();
        }
    }
}