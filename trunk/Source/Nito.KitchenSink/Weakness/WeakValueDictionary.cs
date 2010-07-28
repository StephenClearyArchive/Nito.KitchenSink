// <copyright file="WeakValueDictionary.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A dictionary that has weak references to its values.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value. This must be a reference type.</typeparam>
    public sealed class WeakValueDictionary<TKey, TValue> : IWeakDictionary<TKey, TValue> where TValue : class
    {
        /// <summary>
        /// The storage dictionary, which handles most of the weak reference wrapping.
        /// </summary>
        private readonly ProjectedDictionary<Dictionary<TKey, EquatableWeakReference<TValue>>, TKey, TKey, TValue, EquatableWeakReference<TValue>> dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakValueDictionary&lt;TKey, TValue&gt;"/> class with the specified storage dictionary.
        /// </summary>
        /// <param name="dictionary">The storage dictionary.</param>
        private WeakValueDictionary(Dictionary<TKey, EquatableWeakReference<TValue>> dictionary)
        {
            this.dictionary = new ProjectedDictionary<Dictionary<TKey, EquatableWeakReference<TValue>>, TKey, TKey, TValue, EquatableWeakReference<TValue>>(
                dictionary,
                x => x.Target,
                x => new EquatableWeakReference<TValue>(x),
                x => x,
                x => x);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakValueDictionary&lt;TKey, TValue&gt;"/> class with the specified capacity and equality comparer.
        /// </summary>
        /// <param name="capacity">The capacity to use for this instance.</param>
        /// <param name="comparer">The equality comparer.</param>
        public WeakValueDictionary(int capacity, IEqualityComparer<TKey> comparer = null)
            : this(new Dictionary<TKey, EquatableWeakReference<TValue>>(capacity, comparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakValueDictionary&lt;TKey, TValue&gt;"/> class with the specified equality comparer.
        /// </summary>
        /// <param name="comparer">The equality comparer.</param>
        public WeakValueDictionary(IEqualityComparer<TKey> comparer = null)
            : this(new Dictionary<TKey, EquatableWeakReference<TValue>>(comparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakValueDictionary&lt;TKey, TValue&gt;"/> class with the specified initial elements and equality comparer.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are added to this instance.</param>
        /// <param name="comparer">The equality comparer.</param>
        public WeakValueDictionary(ICollection<KeyValuePair<TKey, TValue>> dictionary, IEqualityComparer<TKey> comparer)
            : this(new Dictionary<TKey, EquatableWeakReference<TValue>>(dictionary.Count, comparer))
        {
            foreach (var kvp in dictionary)
            {
                this.dictionary.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            this.dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key. Does not indicate whether the value is dead or alive.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return this.dictionary.WithoutProjection.ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return this.dictionary.WithoutProjection.Keys; }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public bool Remove(TKey key)
        {
            EquatableWeakReference<TValue> value;
            if (this.dictionary.WithoutProjection.TryGetValue(key, out value))
            {
                value.Dispose();
                return this.dictionary.WithoutProjection.Remove(key);
            }

            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key. If the value is dead, then <paramref name="value"/> is set to <c>null</c> and this method returns <c>true</c>.
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
        /// Gets or sets the element with the specified key. The returned value will be <c>null</c> if it is no longer alive.
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
                this.Remove(key);
                this.Add(key, value);
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
            foreach (var kvp in this.dictionary.WithoutProjection)
            {
                kvp.Value.Dispose();
            }

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
                var purgedValues = new List<TKey>();

                foreach (var kvp in this.dictionary.WithoutProjection)
                {
                    var ret = new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Target);
                    if (ret.Value == null)
                    {
                        purgedValues.Add(kvp.Key);
                    }

                    yield return ret;
                }

                foreach (var key in purgedValues)
                {
                    this.Remove(key);
                }
            }
        }

        /// <summary>
        /// Removes all dead objects from the collection.
        /// </summary>
        public void Purge()
        {
            var purgedValues = (from kvp in this.dictionary.WithoutProjection where !kvp.Value.IsAlive select kvp.Key).ToList();

            foreach (var key in purgedValues)
            {
                this.Remove(key);
            }
        }

        /// <summary>
        /// Releases all weak reference handles.
        /// </summary>
        public void Dispose()
        {
            this.Clear();
        }
    }
}
