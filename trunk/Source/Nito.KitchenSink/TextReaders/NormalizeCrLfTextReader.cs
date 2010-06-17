namespace Nito.KitchenSink
{
    using System.IO;

    /// <summary>
    /// A wrapper around a source <see cref="TextReader"/>, converting any <c>\r\n</c>, <c>\r</c>, or <c>\n</c> character sequences to just <c>\n</c>. This class does perform buffering.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="TextReader"/> being wrapped.</typeparam>
    public sealed class NormalizeCrLfTextReader<T> : SourceTextReader<PeekableTextReader<T>> where T : TextReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeCrLfTextReader{T}"/> class wrapping the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="source">The source <see cref="TextReader"/> to wrap. This <see cref="TextReader"/> is wrapped by a <see cref="PeekableTextReader{T}"/> before being wrapped by this class.</param>
        public NormalizeCrLfTextReader(T source)
            : base(source.Peekable())
        {
        }

        /// <summary>
        /// Reads the next character without changing the state of the reader or the character source. Returns the next available character without actually reading it from the input stream.
        /// </summary>
        /// <returns>An integer representing the next character to be read, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Peek()
        {
            int ret = base.Peek();
            if (ret == -1)
            {
                return ret;
            }

            if ((char)ret == '\r')
            {
                return '\n';
            }

            return ret;
        }

        /// <summary>
        /// Reads the next character from the input stream and advances the character position by one character.
        /// </summary>
        /// <returns>The next character from the input stream, or -1 if no more characters are available.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextReader"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override int Read()
        {
            int ch = base.Read();
            if (ch == -1)
            {
                return ch;
            }

            if ((char)ch == '\r')
            {
                int nextCh = base.Peek();
                if (nextCh == -1)
                {
                    return '\n';
                }

                if ((char)nextCh == '\n')
                {
                    return base.Read();
                }

                return '\n';
            }

            return ch;
        }
    }
}