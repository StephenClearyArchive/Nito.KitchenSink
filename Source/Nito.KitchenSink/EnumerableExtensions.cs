using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Creates a wrapper for the <see cref="IEnumerator{T}"/> returned from <see cref="IEnumerable{T}.GetEnumerator"/>.
        /// </summary>
        /// <typeparam name="T">The type of objects being enumerated.</typeparam>
        /// <param name="source">The source enumeration.</param>
        /// <returns>An enumerator wrapper.</returns>
        public static EnumeratorWrapper<T> CreateEnumeratorWrapper<T>(this IEnumerable<T> source)
        {
            return new EnumeratorWrapper<T>(source);
        }
    }
}
