// <copyright file="CollectionHelpers.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;

    internal static class CollectionHelpers
    {
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

        public static ICollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source, Func<int> count = null, Func<T, bool> contains = null)
        {
            return new ReadOnlyCollection<T>(source, count, contains);
        }
    }
}
