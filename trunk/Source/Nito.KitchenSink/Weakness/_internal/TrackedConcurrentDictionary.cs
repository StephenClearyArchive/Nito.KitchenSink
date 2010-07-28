// <copyright file="TrackedConcurrentDictionary.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Makes a best effort to track keys and values as they leave a concurrent dictionary. The only methods that cannot do this are <see cref="Clear"/>, <see cref="TryUpdate"/>, and <see cref="Remove(KeyValuePair{TKey, TValue})"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    internal sealed class TrackedConcurrentDictionary<TKey, TValue> : IConcurrentDictionary<TKey, TValue>
    {
        /// <summary>
        /// The action to take when a key/value pair is removed.
        /// </summary>
        private readonly Action<TKey, TValue> kvpRemoved;

        /// <summary>
        /// The action to take when a key/value pair has its value updated. This delegate may be invoked in the middle of an atomic update, so it should not modify the dictionary.
        /// </summary>
        private readonly Action<TKey, TValue, TValue> kvpUpdated;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedConcurrentDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="dictionary">The underlying concurrent dictionary.</param>
        /// <param name="kvpRemoved">The action to take when a key/value pair is removed.</param>
        /// <param name="kvpUpdated">The action to take when a key/value pair has its value updated. This delegate may be invoked in the middle of an atomic update, so it should not modify the dictionary.</param>
        public TrackedConcurrentDictionary(
            ConcurrentDictionary<TKey, TValue> dictionary,
            Action<TKey, TValue> kvpRemoved,
            Action<TKey, TValue, TValue> kvpUpdated)
        {
            this.WithoutTracking = dictionary;
            this.kvpRemoved = kvpRemoved;
            this.kvpUpdated = kvpUpdated;
        }

        /// <summary>
        /// Gets the underlying concurrent dictionary.
        /// </summary>
        public ConcurrentDictionary<TKey, TValue> WithoutTracking { get; private set; }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            this.WithoutTracking.AsDictionary().Add(key, value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return this.WithoutTracking.ContainsKey(key);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TKey> Keys
        {
            get { return this.WithoutTracking.Keys; }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>, and then invokes the removed delegate.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public bool Remove(TKey key)
        {
            TValue value;
            if (this.WithoutTracking.TryRemove(key, out value))
            {
                this.kvpRemoved(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.WithoutTracking.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TValue> Values
        {
            get { return this.WithoutTracking.Values; }
        }

        /// <summary>
        /// Gets or sets the element with the specified key. If this updates the value for an existing key, then this invokes the updated delegate during the update.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return this.WithoutTracking[key];
            }

            set
            {
                this.WithoutTracking.AddOrUpdate(
                    key,
                    value,
                    (k, oldValue) =>
                    {
                        this.kvpUpdated(k, oldValue, value);
                        return value;
                    });
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.WithoutTracking.AsDictionary().Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>. Does not invoke the removed delegate.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            // Note: does not notify of removal.
            this.WithoutTracking.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.WithoutTracking.AsDictionary().Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <see cref="KeyValuePair{TKey, TValue}"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.WithoutTracking.AsDictionary().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return this.WithoutTracking.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return this.WithoutTracking.AsDictionary().IsReadOnly; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>. Does not invoke the removed delegate.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.WithoutTracking.AsDictionary().Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.WithoutTracking.GetEnumerator();
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
            return this.WithoutTracking.GetEnumerator();
        }

        /// <summary>
        /// Adds or updates the value for a key. If this method updates the value for an existing key, then the update delegate is invoked during the update.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="addValueFactory">The generator of the value to add, if the key does not already exist.</param>
        /// <param name="updateValueFactory">The generator of the updated value, if the key does already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the return value of <paramref name="updateValueFactory"/>; otherwise, this is the return value of <paramref name="addValueFactory"/>.</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return this.WithoutTracking.AddOrUpdate(
                key,
                addValueFactory,
                (k, oldValue) =>
                {
                    var value = updateValueFactory(k, oldValue);
                    this.kvpUpdated(key, oldValue, value);
                    return value;
                });
        }

        /// <summary>
        /// Adds or updates the value for a key. If this method updates the value for an existing key, then the update delegate is invoked during the update.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="addValue">The value to add, if the key does not already exist.</param>
        /// <param name="updateValueFactory">The generator of the updated value, if the key does already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the return value of <paramref name="updateValueFactory"/>; otherwise, this is <paramref name="addValue"/>.</returns>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return this.AddOrUpdate(key, _ => addValue, updateValueFactory);
        }

        /// <summary>
        /// Adds a key and value if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="valueFactory">The generator of the value to add, if the key does not already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the existing value; otherwise, this is the return value of <paramref name="valueFactory"/>.</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return this.WithoutTracking.GetOrAdd(key, valueFactory);
        }

        /// <summary>
        /// Adds a key and value if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="value">The value to add, if the key does not already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the existing value; otherwise, this is <paramref name="value"/>.</returns>
        public TValue GetOrAdd(TKey key, TValue value)
        {
            return this.WithoutTracking.GetOrAdd(key, value);
        }

        /// <summary>
        /// Attempts to add a key and value.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <returns><c>true</c> if the key and value were added; <c>false</c> if the key already existed.</returns>
        public bool TryAdd(TKey key, TValue value)
        {
            return this.WithoutTracking.TryAdd(key, value);
        }

        /// <summary>
        /// Attempts to atomically retrieve and remove the value for a specified key and then invoke the removed delegate.
        /// </summary>
        /// <param name="key">The key whose value is removed and returned.</param>
        /// <param name="value">On return, contains the value removed, or the default value if the key was not found.</param>
        /// <returns><c>true</c> if the value was removed; <c>false</c> if the key was not found.</returns>
        public bool TryRemove(TKey key, out TValue value)
        {
            if (this.WithoutTracking.TryRemove(key, out value))
            {
                this.kvpRemoved(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Atomically looks up the existing value for the specified key, compares it with the specified value, and (if they are equal) updates the key with a third value. Does not invoke the updated delegate.
        /// </summary>
        /// <param name="key">The key used to lookup the existing value.</param>
        /// <param name="newValue">The new value for the key, if the old value is equal to <paramref name="comparisonValue"/>.</param>
        /// <param name="comparisonValue">The value to compare against the existing value of <paramref name="key"/>.</param>
        /// <returns><c>true</c> if the existing value for the key was equal to <paramref name="comparisonValue"/> and was replaced with <paramref name="newValue"/>; otherwise, <c>false</c>.</returns>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            return this.WithoutTracking.TryUpdate(key, newValue, comparisonValue);
        }
    }
}
