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

        /// <summary>
        /// Attempts to determine the count of an enumeration without enumerating it. Returns <c>true</c> if the count was successfully determined.
        /// </summary>
        /// <typeparam name="T">The type of objects in the enumeration.</typeparam>
        /// <param name="source">The source enumeration.</param>
        /// <param name="result">On return, contains the count of the enumeration if the return value was <c>true</c>. If the return value was <c>false</c>, this value is undefined.</param>
        /// <returns><c>true</c> if the count was successfully determined; otherwise, <c>false</c>.</returns>
        public static bool TryGetCount<T>(this IEnumerable<T> source, out int result)
        {
            var collection1 = source as ICollection<T>;
            if (collection1 != null)
            {
                result = collection1.Count;
                return true;
            }

            var collection2 = source as System.Collections.ICollection;
            if (collection2 != null)
            {
                result = collection2.Count;
                return true;
            }

            result = -1;
            return false;
        }
    }
}
