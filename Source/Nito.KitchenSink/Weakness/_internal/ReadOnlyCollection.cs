// <copyright file="ReadOnlyCollection.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ReadOnlyCollection<T> : ICollection<T>
    {
        private readonly IEnumerable<T> source;

        private readonly Func<int> count;

        private readonly Func<T, bool> contains;

        public ReadOnlyCollection(IEnumerable<T> source, Func<int> count, Func<T, bool> contains)
        {
            this.source = source;
            this.count = count;
            this.contains = contains;
        }

        public void Add(T item)
        {
            throw ReadOnlyException();
        }

        public void Clear()
        {
            throw ReadOnlyException();
        }

        public bool Contains(T item)
        {
            if (this.contains == null)
            {
                return this.source.Contains(item);
            }

            return this.contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CopyToImpl(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                if (this.count == null)
                {
                    return this.source.Count();
                }

                return this.count();
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw ReadOnlyException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.source.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private static Exception ReadOnlyException()
        {
            return new NotSupportedException("This operation is not supported on a read-only collection.");
        }
    }
}
