// <copyright file="WeakKeyDictionary.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System.Collections.Generic;
    using System.Linq;
    using Nito.Weakness.ObjectTracking;

    /// <summary>
    /// A dictionary that has weak references to its keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the key. This must be a reference type that is reference-equatable.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public sealed class WeakKeyDictionary<TKey, TValue> : IWeakDictionary<TKey, TValue> where TKey : class
    {
        /// <summary>
        /// The storage dictionary, which handles most of the weak reference wrapping.
        /// </summary>
        private readonly ProjectedDictionary<Dictionary<ObjectId, TValue>, TKey, ObjectId, TValue, TValue> dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakKeyDictionary&lt;TKey, TValue&gt;"/> class with the specified storage dictionary.
        /// </summary>
        /// <param name="dictionary">The storage dictionary.</param>
        private WeakKeyDictionary(Dictionary<ObjectId, TValue> dictionary)
        {
            this.dictionary = new ProjectedDictionary<Dictionary<ObjectId, TValue>, TKey, ObjectId, TValue, TValue>(
                dictionary,
                x => x,
                x => x,
                x => x.TargetAs<TKey>(),
                x => ObjectTracker.Default.TrackObject(x));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakKeyDictionary&lt;TKey, TValue&gt;"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity to use for this instance.</param>
        public WeakKeyDictionary(int capacity)
            : this(new Dictionary<ObjectId, TValue>(capacity))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakKeyDictionary&lt;TKey, TValue&gt;"/> class with the specified initial elements.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are added to this instance.</param>
        public WeakKeyDictionary(ICollection<KeyValuePair<TKey, TValue>> dictionary)
            : this(new Dictionary<ObjectId, TValue>(dictionary.Count))
        {
            foreach (var kvp in dictionary)
            {
                this.dictionary.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>. The <paramref name="key"/> object must be a reference-equatable instance.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            this.dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key. The <paramref name="key"/> object must be a reference-equatable instance.
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

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>. The <paramref name="key"/> object must be a reference-equatable instance.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public bool Remove(TKey key)
        {
            return this.dictionary.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key. The <paramref name="key"/> object must be a reference-equatable instance.
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
            get { return this.dictionary.WithoutProjection.Values; }
        }

        /// <summary>
        /// Gets or sets the element with the specified key. The <paramref name="key"/> object must be a reference-equatable instance.
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
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            this.dictionary.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            this.dictionary.WithoutProjection.Clear();
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
            get { return this.dictionary.WithoutProjection.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return this.dictionary.WithoutProjection.AsDictionary().IsReadOnly; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
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
        /// Gets a sequence of live objects from the collection, causing a purge. The purge occurs at the end of the enumeration.
        /// </summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> LiveList
        {
            get
            {
                var purgedKeys = new List<ObjectId>();

                foreach (var kvp in this.dictionary.WithoutProjection)
                {
                    var ret = new KeyValuePair<TKey, TValue>(kvp.Key.TargetAs<TKey>(), kvp.Value);
                    if (ret.Key == null)
                    {
                        purgedKeys.Add(kvp.Key);
                    }

                    yield return ret;
                }

                foreach (var key in purgedKeys)
                {
                    this.dictionary.WithoutProjection.Remove(key);
                }
            }
        }

        /// <summary>
        /// Removes all dead objects from the collection.
        /// </summary>
        public void Purge()
        {
            var purgedKeys = (from kvp in this.dictionary.WithoutProjection where !kvp.Key.IsAlive select kvp.Key).ToList();

            foreach (var key in purgedKeys)
            {
                this.dictionary.WithoutProjection.Remove(key);
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
