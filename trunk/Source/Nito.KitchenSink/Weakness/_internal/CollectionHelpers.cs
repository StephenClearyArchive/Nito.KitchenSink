// <copyright file="CollectionHelpers.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Static methods that assist in implementing collections.
    /// </summary>
    internal static class CollectionHelpers
    {
        /// <summary>
        /// Implements the <see cref="ICollection{T}.CopyTo"/> method for any collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <param name="array">The array to which the elements are copied.</param>
        /// <param name="arrayIndex">The starting array index to which the elements are copied.</param>
        public static void CopyToImpl<T>(this ICollection<T> collection, T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", "Array is null");
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", "Invalid offset " + arrayIndex);
            }

            if (array.Length - arrayIndex < collection.Count)
            {
                throw new ArgumentException("Invalid offset (" + arrayIndex + ") for source length " + array.Length);
            }

            int i = arrayIndex;
            foreach (var item in collection)
            {
                array[i] = item;
                ++i;
            }
        }

        /// <summary>
        /// Wraps an enumerable in a read-only collection, with optional optimizations.
        /// </summary>
        /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <param name="count">An optional delegate to determine the number of elements in the sequence.</param>
        /// <param name="contains">An optional delegate to determine whether a certain element is in the sequence.</param>
        /// <returns>The read-only collection.</returns>
        public static ICollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source, Func<int> count = null, Func<T, bool> contains = null)
        {
            return new ReadOnlyCollection<T>(source, count, contains);
        }

        /// <summary>
        /// Restricts the compile-time type of the argument to the dictionary interface.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to restrict.</param>
        /// <returns><paramref name="dictionary"/>, as a dictionary interface.</returns>
        public static IDictionary<TKey, TValue> AsDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return dictionary;
        }
    }
}
