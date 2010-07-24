using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Nito.Weakness
{
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

        public static ISourceDictionary<TKey, TKey, TNewValue, TOldValue> SelectValue<TKey, TOldValue, TNewValue>(
            this IDictionary<TKey, TOldValue> source,
            Func<TOldValue, TNewValue> selector,
            Func<TNewValue, TOldValue> reverseSelector)
        {
            return new ProjectedDictionary<TKey, TKey, TNewValue, TOldValue>(source, selector, reverseSelector, x => x, x => x);
        }

        public static ISourceDictionary<TNewKey, TOldKey, TValue, TValue> SelectKey<TOldKey, TNewKey, TValue>(
            this IDictionary<TOldKey, TValue> source,
            Func<TOldKey, TNewKey> selector,
            Func<TNewKey, TOldKey> reverseSelector)
        {
            return new ProjectedDictionary<TNewKey, TOldKey, TValue, TValue>(source, x => x, x => x, selector, reverseSelector);
        }
    }
}
