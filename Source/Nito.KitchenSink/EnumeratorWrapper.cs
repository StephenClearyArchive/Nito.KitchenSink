using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Provides a wrapper around a source <see cref="IEnumerator{T}"/>, with a cached <see cref="Done"/> value.
    /// </summary>
    /// <typeparam name="T">The type of objects being enumerated.</typeparam>
    public sealed class EnumeratorWrapper<T> : IEnumerator<T>
    {
        /// <summary>
        /// The source enumerator.
        /// </summary>
        private readonly IEnumerator<T> source;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumeratorWrapper&lt;T&gt;"/> class, taking its source enumerator from <see cref="IEnumerable{T}.GetEnumerator"/>. <see cref="System.Collections.IEnumerator.MoveNext"/> is also called once before returning.
        /// </summary>
        /// <param name="source">The source enumerable.</param>
        public EnumeratorWrapper(IEnumerable<T> source)
        {
            this.source = source.GetEnumerator();
            this.More = this.source.MoveNext();
        }

        /// <summary>
        /// Gets a value indicating whether the enumerable sequence has completed. If <see cref="Done"/> is true, then <see cref="Current"/> is undefined.
        /// </summary>
        public bool Done
        {
            get { return !this.More; }
        }

        /// <summary>
        /// Gets a value indicating whether the enumerable sequence has completed. If <see cref="More"/> is false, then <see cref="Current"/> is undefined.
        /// </summary>
        public bool More { get; private set; }

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public T Current
        {
            get { return this.source.Current; }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection. Returns <see cref="Done"/>.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            this.More = this.source.MoveNext();
            return !this.Done;
        }

        /// <summary>
        /// Disposes the wrapped enumerator.
        /// </summary>
        public void Dispose()
        {
            this.source.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        void System.Collections.IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }
}
