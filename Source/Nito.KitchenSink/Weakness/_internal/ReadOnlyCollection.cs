// <copyright file="ReadOnlyCollection.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A read-only collection wrapper around a source enumerable, with optional optimizations for the <see cref="Count"/> and <see cref="Contains"/> operations.
    /// </summary>
    /// <typeparam name="T">The type of element in the enumerable.</typeparam>
    internal sealed class ReadOnlyCollection<T> : ICollection<T>
    {
        /// <summary>
        /// The source enumerable.
        /// </summary>
        private readonly IEnumerable<T> source;

        /// <summary>
        /// The delegate used to determine the number of elements in the enumerable. This may be <c>null</c>.
        /// </summary>
        private readonly Func<int> count;

        /// <summary>
        /// The delegate used to determine whether the enumerable contains an element. This may be <c>null</c>.
        /// </summary>
        private readonly Func<T, bool> contains;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="source">The source enumerable.</param>
        /// <param name="count">The delegate used to determine the number of elements in the enumerable. This may be <c>null</c>.</param>
        /// <param name="contains">The delegate used to determine whether the enumerable contains an element. This may be <c>null</c>.</param>
        public ReadOnlyCollection(IEnumerable<T> source, Func<int> count, Func<T, bool> contains)
        {
            this.source = source;
            this.count = count;
            this.contains = contains;
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="item">This parameter is ignored.</param>
        public void Add(T item)
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public void Clear()
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(T item)
        {
            if (this.contains == null)
            {
                return this.source.Contains(item);
            }

            return this.contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <typeparamref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CopyToImpl(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
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

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        /// <returns><c>true</c></returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="item">This parameter is ignored.</param>
        /// <returns>This method does not return.</returns>
        public bool Remove(T item)
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return this.source.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns a new <see cref="NotSupportedException"/> with an appropriate message for attempting to modify a read-only collection.
        /// </summary>
        /// <returns>A new <see cref="NotSupportedException"/> with an appropriate message for attempting to modify a read-only collection.</returns>
        private static Exception ReadOnlyException()
        {
            return new NotSupportedException("This operation is not supported on a read-only collection.");
        }
    }
}
