using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.Weakness
{
    using System.Collections.Concurrent;

    internal sealed class TrackedConcurrentDictionary<TKey, TValue> : IConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> dictionary;

        private readonly Action<TKey, TValue> kvpAdded; // May be invoked in the middle of an atomic add
        private readonly Action<TKey, TValue> kvpRemoved;
        private readonly Action<TKey, TValue, TValue> kvpUpdated; // May be invoked in the middle of an atomic update

        private readonly Action<TKey, TValue, TValue> kvpUpdatedOriginalValueUnknown;

        public TrackedConcurrentDictionary(
            ConcurrentDictionary<TKey, TValue> dictionary,
            Action<TKey, TValue> kvpAdded,
            Action<TKey, TValue> kvpRemoved,
            Action<TKey, TValue, TValue> kvpUpdated,
            Action<TKey, TValue, TValue> kvpUpdatedOriginalValueUnknown)
        {
            this.dictionary = dictionary;
            this.kvpAdded = kvpAdded;
            this.kvpRemoved = kvpRemoved;
            this.kvpUpdated = kvpUpdated;
            this.kvpUpdatedOriginalValueUnknown = kvpUpdatedOriginalValueUnknown;
        }

        public void Add(TKey key, TValue value)
        {
            this.dictionary.AsDictionary().Add(key, value);
            this.kvpAdded(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return this.dictionary.Keys; }
        }

        public bool Remove(TKey key)
        {
            TValue value;
            if (this.dictionary.TryRemove(key, out value))
            {
                this.kvpRemoved(key, value);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return this.dictionary.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }

            set
            {
                this.dictionary.AddOrUpdate(
                    key,
                    k =>
                    {
                        this.kvpAdded(k, value);
                        return value;
                    },
                    (k, oldValue) =>
                    {
                        this.kvpUpdated(k, oldValue, value);
                        return value;
                    });
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.dictionary.AsDictionary().Add(item);
            this.kvpAdded(item.Key, item.Value);
        }

        public void Clear()
        {
            // Note: does not notify of removal.
            this.dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.AsDictionary().Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.dictionary.AsDictionary().CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.dictionary.AsDictionary().IsReadOnly; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (this.dictionary.AsDictionary().Remove(item))
            {
                this.kvpRemoved(item.Key, item.Value);
                return true;
            }

            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return this.dictionary.AddOrUpdate(
                key,
                k =>
                {
                    var value = addValueFactory(k);
                    this.kvpAdded(key, value);
                    return value;
                },
                (k, oldValue) =>
                {
                    var value = updateValueFactory(k, oldValue);
                    this.kvpUpdated(key, oldValue, value);
                    return value;
                });
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return this.AddOrUpdate(key, _ => addValue, updateValueFactory);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return this.dictionary.GetOrAdd(key, valueFactory);
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            return this.dictionary.GetOrAdd(key, value);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (this.dictionary.TryAdd(key, value))
            {
                this.kvpAdded(key, value);
                return true;
            }

            return false;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if (this.dictionary.TryRemove(key, out value))
            {
                this.kvpRemoved(key, value);
                return true;
            }

            return false;
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (this.dictionary.TryUpdate(key, newValue, comparisonValue))
            {
                this.kvpUpdatedOriginalValueUnknown(key, comparisonValue, newValue);
                return true;
            }

            return false;
        }
    }
}
