﻿// <copyright file="ConcurrentWeakKeyDictionary.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Nito.Weakness.ObjectTracking;

    /// <summary>
    /// A concurrent dictionary that has weak references to its keys. All key instances passed to methods of this class must be reference-equatable instances. There is no need to periodically purge this collection; it will purge itself over time. The methods <see cref="Clear"/> and <see cref="Remove(KeyValuePair{TKey, TValue})"/> do not release resources immediately.
    /// </summary>
    /// <typeparam name="TKey">The type of the key. This must be a reference type.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public sealed class ConcurrentWeakKeyDictionary<TKey, TValue> : IConcurrentWeakDictionary<TKey, TValue> where TKey : class
    {
        /// <summary>
        /// The storage dictionary, which handles the wrapping for read-only values, and disposing most of the weak references for values when they are removed.
        /// </summary>
        private readonly ProjectedDictionary<TrackedConcurrentDictionary<RegisteredObjectId, TValue>, TKey, RegisteredObjectId, TValue, TValue> dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentWeakKeyDictionary&lt;TKey, TValue&gt;"/> class with the specified storage dictionary.
        /// </summary>
        /// <param name="dictionary">The storage dictionary.</param>
        private ConcurrentWeakKeyDictionary(ConcurrentDictionary<RegisteredObjectId, TValue> dictionary)
        {
            this.dictionary =
                new ProjectedDictionary<TrackedConcurrentDictionary<RegisteredObjectId, TValue>, TKey, RegisteredObjectId, TValue, TValue>(
                    dictionary.DisposableKeys(),
                    x => x,
                    x => x,
                    x => x.Id.TargetAs<TKey>(),
                    x => new RegisteredObjectId(ObjectTracker.Default.TrackObject(x)));
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            using (var storedKey = this.StoreKey(key).MutableWrapper())
            {
                this.dictionary.WithoutProjection.WithoutTracking.AsDictionary().Add(storedKey.Value, value);
                GC.KeepAlive(key);
                storedKey.Value = null;
            }
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
            return this.dictionary.ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return this.dictionary.Keys; }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return this.dictionary.Remove(key);
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
            return this.dictionary.TryGetValue(key, out value);
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return this.dictionary.WithoutProjection.WithoutTracking.Values; }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }

            set
            {
                this.dictionary[key] = value;
                GC.KeepAlive(key);
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            using (var storedKey = this.StoreKey(item.Key).MutableWrapper())
            {
                this.dictionary.WithoutProjection.Add(new KeyValuePair<RegisteredObjectId, TValue>(storedKey.Value, item.Value));
                GC.KeepAlive(item.Key);
                storedKey.Value = null;
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            // Simply clearing the underlying dictionary is not efficient in terms of unregistering the callbacks,
            // but failure to unregister is benign compared to the concurrency issues of handling Clear any other way.
            this.dictionary.WithoutProjection.WithoutTracking.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return this.dictionary.WithoutProjection.WithoutTracking.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return this.dictionary.WithoutProjection.WithoutTracking.AsDictionary().IsReadOnly; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            // This method does "leak" a registered callback action.
            return this.dictionary.Remove(item);
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
            return this.dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets a sequence of live objects from the collection. Does not actually cause a purge, as no purges are needed for this collection.
        /// </summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> LiveList
        {
            get
            {
                return this.Where(kvp => kvp.Key != null);
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Purge()
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Adds or updates the value for a key.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="addValueFactory">The generator of the value to add, if the key does not already exist.</param>
        /// <param name="updateValueFactory">The generator of the updated value, if the key does already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the return value of <paramref name="updateValueFactory"/>; otherwise, this is the return value of <paramref name="addValueFactory"/>.</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            using (var storedKey = this.StoreKey(key).MutableWrapper())
            {
                // Exception -> dispose
                // Existing key -> dispose
                // New key -> do not dispose
                var ret = this.dictionary.WithoutProjection.AddOrUpdate(
                    storedKey.Value,
                    k => addValueFactory(key),
                    (k, oldValue) =>
                    {
                        storedKey.Dispose();
                        return updateValueFactory(key, oldValue);
                    });
                storedKey.Value = null;
                return ret;
            }
        }

        /// <summary>
        /// Adds or updates the value for a key.
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
            using (var storedKey = this.StoreKey(key).MutableWrapper())
            {
                // Exception -> dispose
                // Existing key -> dispose
                // New key -> do not dispose
                return this.dictionary.WithoutProjection.GetOrAdd(
                    storedKey.Value,
                    k =>
                    {
                        storedKey.Value = null;
                        return valueFactory(key);
                    });
            }
        }

        /// <summary>
        /// Adds a key and value if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="value">The value to add, if the key does not already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the existing value; otherwise, this is <paramref name="value"/>.</returns>
        public TValue GetOrAdd(TKey key, TValue value)
        {
            return this.GetOrAdd(key, _ => value);
        }

        /// <summary>
        /// Attempts to add a key and value.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <returns><c>true</c> if the key and value were added; <c>false</c> if the key already existed.</returns>
        public bool TryAdd(TKey key, TValue value)
        {
            using (var storedKey = this.StoreKey(key).MutableWrapper())
            {
                if (this.dictionary.WithoutProjection.TryAdd(storedKey.Value, value))
                {
                    storedKey.Value = null;
                    GC.KeepAlive(key);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Attempts to atomically retrieve and remove the value for a specified key.
        /// </summary>
        /// <param name="key">The key whose value is removed and returned.</param>
        /// <param name="value">On return, contains the value removed, or the default value if the key was not found.</param>
        /// <returns><c>true</c> if the value was removed; <c>false</c> if the key was not found.</returns>
        public bool TryRemove(TKey key, out TValue value)
        {
            return this.dictionary.WithoutProjection.TryRemove(this.dictionary.KeyMapExposedToStored(key), out value);
        }

        /// <summary>
        /// Atomically looks up the existing value for the specified key, compares it with the specified value, and (if they are equal) updates the key with a third value.
        /// </summary>
        /// <param name="key">The key used to lookup the existing value.</param>
        /// <param name="newValue">The new value for the key, if the old value is equal to <paramref name="comparisonValue"/>.</param>
        /// <param name="comparisonValue">The value to compare against the existing value of <paramref name="key"/>.</param>
        /// <returns><c>true</c> if the existing value for the key was equal to <paramref name="comparisonValue"/> and was replaced with <paramref name="newValue"/>; otherwise, <c>false</c>.</returns>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            var ret = this.dictionary.WithoutProjection.TryUpdate(this.dictionary.KeyMapExposedToStored(key), newValue, comparisonValue);
            GC.KeepAlive(key);
            return ret;
        }

        /// <summary>
        /// Maps an exposed key to a stored key, expecting to store the actual value.
        /// </summary>
        /// <param name="key">The key to be stored.</param>
        /// <returns>A stored key.</returns>
        private RegisteredObjectId StoreKey(TKey key)
        {
            return new RegisteredObjectId(ObjectTracker.Default.TrackObject(key), id => this.dictionary.WithoutProjection.WithoutTracking.AsDictionary().Remove(new RegisteredObjectId(id)));
        }
    }
}