// <copyright file="ConcurrentWeakValueDictionary.cs" company="Nito Programs">
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
    /// A concurrent dictionary that has weak references to its values. All value instances passed to the methods of this class must be reference-equatable instances. There is no need to periodically purge this collection; it will purge itself over time. The methods <see cref="ICollection{T}.Clear"/>, <see cref="IConcurrentDictionary{TKey, TValue}.TryUpdate"/>, and <see cref="ICollection{T}.Remove"/> do not release resources immediately.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value. This must be a reference type.</typeparam>
    public sealed class ConcurrentWeakValueDictionary<TKey, TValue> : IConcurrentWeakDictionary<TKey, TValue> where TValue : class
    {
        /// <summary>
        /// The storage dictionary, which handles the wrapping for read-only values, and disposing most of the weak references for values when they are removed.
        /// </summary>
        private readonly ProjectedDictionary<TrackedConcurrentDictionary<TKey, RegisteredObjectId>, TKey, TKey, TValue, RegisteredObjectId> dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentWeakValueDictionary&lt;TKey, TValue&gt;"/> class with the specified storage dictionary.
        /// </summary>
        /// <param name="dictionary">The storage dictionary.</param>
        private ConcurrentWeakValueDictionary(ConcurrentDictionary<TKey, RegisteredObjectId> dictionary)
        {
            this.dictionary =
                new ProjectedDictionary<TrackedConcurrentDictionary<TKey, RegisteredObjectId>, TKey, TKey, TValue, RegisteredObjectId>(
                    dictionary.DisposableValues(),
                    x => x.Id.TargetAs<TValue>(),
                    x => new RegisteredObjectId(ObjectTracker.Default.TrackObject(x)),
                    x => x,
                    x => x);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            using (var storedValue = this.StoreValue(key, value).MutableWrapper())
            {
                this.dictionary.WithoutProjection.WithoutTracking.AsDictionary().Add(key, storedValue.Value);
                GC.KeepAlive(value);
                storedValue.Value = null;
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
            return this.dictionary.WithoutProjection.WithoutTracking.ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return this.dictionary.WithoutProjection.WithoutTracking.Keys; }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return this.dictionary.WithoutProjection.Remove(key);
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
            get { return this.dictionary.Values; }
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
                this.dictionary.WithoutProjection[key] = this.StoreValue(key, value);
                GC.KeepAlive(value);
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            this.AsDictionary().Add(item.Key, item.Value);
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
                return this.Where(kvp => kvp.Value != null);
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
            TValue value = null;
            var ret = this.dictionary.ValueMapStoredToExposed(
                this.dictionary.WithoutProjection.AddOrUpdate(
                    key,
                    k =>
                    {
                        value = addValueFactory(k);
                        return this.StoreValue(k, value);
                    },
                    (k, oldStoredValue) =>
                    {
                        value = updateValueFactory(k, this.dictionary.ValueMapStoredToExposed(oldStoredValue));
                        return this.StoreValue(k, value);
                    }));

            GC.KeepAlive(key);
            GC.KeepAlive(value);
            return ret;
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
            TValue value = null;
            var ret = this.dictionary.ValueMapStoredToExposed(
                this.dictionary.WithoutProjection.GetOrAdd(
                    key,
                    k =>
                    {
                        value = valueFactory(k);
                        return this.StoreValue(k, value);
                    }));

            GC.KeepAlive(value);
            return ret;
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
            using (var storedValue = this.StoreValue(key, value).MutableWrapper())
            {
                if (this.dictionary.WithoutProjection.TryAdd(key, storedValue.Value))
                {
                    storedValue.Value = null;
                    GC.KeepAlive(value);
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
            RegisteredObjectId storedValue;
            if (this.dictionary.WithoutProjection.TryRemove(key, out storedValue))
            {
                value = this.dictionary.ValueMapStoredToExposed(storedValue);
                return true;
            }

            value = null;
            return false;
        }

        bool IConcurrentDictionary<TKey, TValue>.TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            using (var newStoredValue = this.StoreValue(key, newValue).MutableWrapper())
            {
                var comparisonStoredValue = this.dictionary.ValueMapExposedToStored(comparisonValue);
                if (this.dictionary.WithoutProjection.TryUpdate(key, newStoredValue.Value, comparisonStoredValue))
                {
                    // This method does "leak" a registered callback action when it returns true.
                    GC.KeepAlive(newValue);
                    newStoredValue.Value = null;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Maps an exposed value to a stored value, expecting to store the actual value.
        /// </summary>
        /// <param name="key">The key for the stored value.</param>
        /// <param name="value">The exposed value to store.</param>
        /// <returns>A stored value.</returns>
        private RegisteredObjectId StoreValue(TKey key, TValue value)
        {
            return new RegisteredObjectId(ObjectTracker.Default.TrackObject(value), id => this.dictionary.WithoutProjection.AsDictionary().Remove(new KeyValuePair<TKey, RegisteredObjectId>(key, new RegisteredObjectId(id))));
        }
    }
}