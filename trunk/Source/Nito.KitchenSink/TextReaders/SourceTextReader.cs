namespace Nito.KitchenSink
{
    using System.IO;

    /// <summary>
    /// A base class for a <see cref="TextReader"/> that wraps another <see cref="TextReader"/>. This class does not perform any buffering, but derived classes may.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="TextReader"/> being wrapped.</typeparam>
    public abstract class SourceTextReader<T> : TextReader where T : TextReader
    {
        /// <summary>
        /// The source <see cref="TextReader"/>.
        /// </summary>
        protected readonly T source;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceTextReader{T}"/> class.
        /// </summary>
        /// <param name="source">The source <see cref="TextReader"/>.</param>
        protected SourceTextReader(T source)
        {
            this.source = source;
        }

        /// <summary>
        /// Gets the underlying <see cref="TextReader"/>.
        /// </summary>
        public T BaseReader
        {
            get { return this.source; }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.TextReader"/> and optionally releases the managed resources (including the source <see cref="TextReader"/>).
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.source.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Reads the next character without changing the state of the reader or the character source. Returns the next available character without actually reading it from the input stream.
        /// </summary>
        /// <returns>An integer representing the next character to be read, or -1 if no more characters are available or the stream does not support seeking.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Peek()
        {
            return this.source.Peek();
        }

        /// <summary>
        /// Reads the next character from the input stream and advances the character position by one character.
        /// </summary>
        /// <returns>The next character from the input stream, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Read()
        {
            return this.source.Read();
        }
    }
}