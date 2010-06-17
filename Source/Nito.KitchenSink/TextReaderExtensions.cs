namespace Nito.KitchenSink
{
    using System.IO;

    /// <summary>
    /// Extension methods for <see cref="TextReader"/> classes.
    /// </summary>
    public static class TextReaderExtensions
    {
        /// <summary>
        /// Wraps this <see cref="TextReader"/> with a putback buffer.
        /// </summary>
        /// <param name="source">The <see cref="TextReader"/> to wrap.</param>
        /// <returns>A <see cref="TextReader"/> with a putback buffer.</returns>
        public static PutbackBufferTextReader<T> ApplyPutbackBuffer<T>(this T source) where T : TextReader
        {
            return new PutbackBufferTextReader<T>(source);
        }

        /// <summary>
        /// Wraps this <see cref="TextReader"/>, converting any <c>\r\n</c>, <c>\r</c>, or <c>\n</c> character sequences to just <c>\n</c>.
        /// </summary>
        /// <param name="source">The <see cref="TextReader"/> to wrap.</param>
        /// <returns>A <see cref="TextReader"/> with normalized line endings.</returns>
        public static NormalizeCrLfTextReader<T> NormalizeCrLf<T>(this T source) where T : TextReader
        {
            return new NormalizeCrLfTextReader<T>(source);
        }

        /// <summary>
        /// Wraps this <see cref="TextReader"/>, counting the lines and columns as characters are read.
        /// </summary>
        /// <param name="source">The <see cref="TextReader"/> to wrap.</param>
        /// <returns>A <see cref="TextReader"/> with counted lines and columns.</returns>
        public static LineMappingTextReader<T> ApplyLineMapping<T>(this T source) where T : TextReader
        {
            return new LineMappingTextReader<T>(source);
        }

        /// <summary>
        /// Wraps this <see cref="TextReader"/>, ensuring that <see cref="TextReader.Peek"/> will only return <c>-1</c> when the end of the stream is reached.
        /// </summary>
        /// <param name="source">The <see cref="TextReader"/> to wrap.</param>
        /// <returns>A peekable <see cref="TextReader"/>.</returns>
        public static PeekableTextReader<T> Peekable<T>(this T source) where T : TextReader
        {
            return new PeekableTextReader<T>(source);
        }

        /// <summary>
        /// Wraps this <see cref="TextReader"/>, tracking the position of the stream as it is read.
        /// </summary>
        /// <param name="source">The <see cref="TextReader"/> to wrap.</param>
        /// <returns>A <see cref="TextReader"/> with position tracking.</returns>
        public static PositionTrackingTextReader<T> ApplyPositionTracking<T>(this T source) where T : TextReader
        {
            return new PositionTrackingTextReader<T>(source);
        }
    }
}