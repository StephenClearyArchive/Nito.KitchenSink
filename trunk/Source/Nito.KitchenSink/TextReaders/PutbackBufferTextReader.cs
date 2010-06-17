namespace Nito.KitchenSink
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// A wrapper around a source <see cref="TextReader"/>, providing a "putback buffer", where characters can be pushed back to the <see cref="TextReader"/>. This class does perform buffering.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="TextReader"/> being wrapped.</typeparam>
    public sealed class PutbackBufferTextReader<T> : SourceTextReader<T> where T : TextReader
    {
        /// <summary>
        /// The buffer of characters that have been put back and not yet read again.
        /// </summary>
        private readonly Stack<char> putbackBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PutbackBufferTextReader{T}"/> class wrapping the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="source">The source <see cref="TextReader"/> to wrap.</param>
        public PutbackBufferTextReader(T source)
            : base(source)
        {
            this.putbackBuffer = new Stack<char>();
        }

        /// <summary>
        /// Gets the putback buffer count.
        /// </summary>
        public int BufferCount
        {
            get { return this.putbackBuffer.Count; }
        }

        /// <summary>
        /// Pushes the specified character back onto this <see cref="TextReader"/>. The character pushed back should be the most recently read character that has not already been pushed back (i.e., characters should only be pushed back if they were read).
        /// </summary>
        /// <param name="ch">The character to push back.</param>
        public void Putback(char ch)
        {
            this.putbackBuffer.Push(ch);
        }

        /// <summary>
        /// Reads the next character without changing the state of the reader or the character source. Returns the next available character without actually reading it from the input stream.
        /// </summary>
        /// <returns>An integer representing the next character to be read, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Peek()
        {
            if (this.putbackBuffer.Count != 0)
            {
                return this.putbackBuffer.Peek();
            }

            int ret = this.source.Peek();
            if (ret != -1)
            {
                return ret;
            }

            int nextCh = this.source.Read();
            if (nextCh == -1)
            {
                return nextCh;
            }

            this.putbackBuffer.Push((char)nextCh);
            return nextCh;
        }

        /// <summary>
        /// Reads the next character from the input stream and advances the character position by one character.
        /// </summary>
        /// <returns>The next character from the input stream, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Read()
        {
            if (this.putbackBuffer.Count != 0)
            {
                return this.putbackBuffer.Pop();
            }

            return this.source.Read();
        }
    }
}
