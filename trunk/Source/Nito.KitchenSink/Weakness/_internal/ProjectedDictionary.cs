// <copyright file="ProjectedDictionary.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A projection of a source dictionary, mapping exposed keys and values to and from stored keys and values.
    /// </summary>
    /// <typeparam name="TSource">The type of the underlying storage dictionary.</typeparam>
    /// <typeparam name="TExposedKey">The type of key exposed to end-user code.</typeparam>
    /// <typeparam name="TStoredKey">The type of key actually stored in the dictionary.</typeparam>
    /// <typeparam name="TExposedValue">The type of value exposed to end-user code.</typeparam>
    /// <typeparam name="TStoredValue">The type of value actually stored in the dictionary.</typeparam>
    internal sealed class ProjectedDictionary<TSource, TExposedKey, TStoredKey, TExposedValue, TStoredValue> : IDictionary<TExposedKey, TExposedValue> where TSource : IDictionary<TStoredKey, TStoredValue>
    {
        /// <summary>
        /// Maps a stored value to an exposed value.
        /// </summary>
        private readonly Func<TStoredValue, TExposedValue> mapStoredValueToExposedValue;

        /// <summary>
        /// Maps an exposed value to a stored value.
        /// </summary>
        private readonly Func<TExposedValue, TStoredValue> mapExposedValueToStoredValue;

        /// <summary>
        /// Maps a stored key to an exposed key.
        /// </summary>
        private readonly Func<TStoredKey, TExposedKey> mapStoredKeyToExposedKey;

        /// <summary>
        /// Maps an exposed key to a stored key.
        /// </summary>
        private readonly Func<TExposedKey, TStoredKey> mapExposedKeyToStoredKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectedDictionary&lt;TSource, TExposedKey, TStoredKey, TExposedValue, TStoredValue&gt;"/> class.
        /// </summary>
        /// <param name="source">The source dictionary, used as the underlying storage dictionary.</param>
        /// <param name="mapValueStoredToExposed">The delegate used to map stored values to exposed values.</param>
        /// <param name="mapValueExposedToStored">The delegate used to map exposed values to stored values.</param>
        /// <param name="mapKeyStoredToExposed">The delegate used to map stored keys to exposed keys.</param>
        /// <param name="mapKeyExposedToStored">The delegate used to map exposed keys to stored keys.</param>
        public ProjectedDictionary(
            TSource source,
            Func<TStoredValue, TExposedValue> mapValueStoredToExposed,
            Func<TExposedValue, TStoredValue> mapValueExposedToStored,
            Func<TStoredKey, TExposedKey> mapKeyStoredToExposed,
            Func<TExposedKey, TStoredKey> mapKeyExposedToStored)
        {
            this.WithoutProjection = source;
            this.mapStoredValueToExposedValue = mapValueStoredToExposed;
            this.mapExposedValueToStoredValue = mapValueExposedToStored;
            this.mapStoredKeyToExposedKey = mapKeyStoredToExposed;
            this.mapExposedKeyToStoredKey = mapKeyExposedToStored;
        }

        /// <summary>
        /// Gets the underlying storage dictionary, without any mapping projection applied.
        /// </summary>
        public TSource WithoutProjection { get; private set; }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(TExposedKey key, TExposedValue value)
        {
            this.WithoutProjection.Add(this.KeyMapExposedToStored(key), this.ValueMapExposedToStored(value));
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(TExposedKey key)
        {
            return this.WithoutProjection.ContainsKey(this.KeyMapExposedToStored(key));
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TExposedKey> Keys
        {
            get { return this.WithoutProjection.Keys.Select(this.KeyMapStoredToExposed).ToReadOnlyCollection(() => this.WithoutProjection.Count, this.ContainsKey); }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public bool Remove(TExposedKey key)
        {
            return this.WithoutProjection.Remove(this.KeyMapExposedToStored(key));
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TExposedKey key, out TExposedValue value)
        {
            TStoredValue ret;
            if (!this.WithoutProjection.TryGetValue(this.KeyMapExposedToStored(key), out ret))
            {
                value = default(TExposedValue);
                return false;
            }

            value = this.ValueMapStoredToExposed(ret);
            return true;
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TExposedValue> Values
        {
            get
            {
                return this.WithoutProjection.Values.Select(this.ValueMapStoredToExposed).ToReadOnlyCollection(() => this.WithoutProjection.Count, value => this.WithoutProjection.Values.Contains(this.ValueMapExposedToStored(value)));
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public TExposedValue this[TExposedKey key]
        {
            get
            {
                return this.ValueMapStoredToExposed(this.WithoutProjection[this.KeyMapExposedToStored(key)]);
            }

            set
            {
                this.WithoutProjection[this.KeyMapExposedToStored(key)] = this.ValueMapExposedToStored(value);
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            this.WithoutProjection.Add(this.KeyMapExposedToStored(item.Key), this.ValueMapExposedToStored(item.Value));
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            this.WithoutProjection.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            return this.WithoutProjection.Contains(this.PairMapExposedToStored(item));
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <see cref="KeyValuePair{TExposedKey, TExposedValue}"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<TExposedKey, TExposedValue>[] array, int arrayIndex)
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
            get { return this.WithoutProjection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return this.WithoutProjection.IsReadOnly; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            return this.WithoutProjection.Remove(this.PairMapExposedToStored(item));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TExposedKey, TExposedValue>> GetEnumerator()
        {
            return this.WithoutProjection.Select(this.PairMapStoredToExposed).GetEnumerator();
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
        /// Maps a stored key/value pair to an exposed key/value pair.
        /// </summary>
        /// <param name="value">The stored key/value pair to map.</param>
        /// <returns>The equivalent exposed key/value pair.</returns>
        public KeyValuePair<TExposedKey, TExposedValue> PairMapStoredToExposed(KeyValuePair<TStoredKey, TStoredValue> value)
        {
            return new KeyValuePair<TExposedKey, TExposedValue>(this.KeyMapStoredToExposed(value.Key), this.ValueMapStoredToExposed(value.Value));
        }

        /// <summary>
        /// Maps an exposed key/value pair to a stored key/value pair.
        /// </summary>
        /// <param name="value">The exposed key/value pair to map.</param>
        /// <returns>The equivalent stored key/value pair.</returns>
        public KeyValuePair<TStoredKey, TStoredValue> PairMapExposedToStored(KeyValuePair<TExposedKey, TExposedValue> value)
        {
            return new KeyValuePair<TStoredKey, TStoredValue>(this.KeyMapExposedToStored(value.Key), this.ValueMapExposedToStored(value.Value));
        }

        /// <summary>
        /// Maps a stored value to an exposed value.
        /// </summary>
        /// <param name="value">The stored value to map.</param>
        /// <returns>The equivalent exposed value.</returns>
        public TExposedValue ValueMapStoredToExposed(TStoredValue value)
        {
            return this.mapStoredValueToExposedValue(value);
        }

        /// <summary>
        /// Maps an exposed value to a stored value.
        /// </summary>
        /// <param name="value">The exposed value to map.</param>
        /// <returns>The equivalent stored value.</returns>
        public TStoredValue ValueMapExposedToStored(TExposedValue value)
        {
            return this.mapExposedValueToStoredValue(value);
        }

        /// <summary>
        /// Maps a stored key to an exposed key.
        /// </summary>
        /// <param name="key">The stored key to map.</param>
        /// <returns>The equivalent exposed key.</returns>
        public TExposedKey KeyMapStoredToExposed(TStoredKey key)
        {
            return this.mapStoredKeyToExposedKey(key);
        }

        /// <summary>
        /// Maps an exposed key to a stored key.
        /// </summary>
        /// <param name="key">The exposed key to map.</param>
        /// <returns>The equivalent stored key.</returns>
        public TStoredKey KeyMapExposedToStored(TExposedKey key)
        {
            return this.mapExposedKeyToStoredKey(key);
        }
    }
}
