using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Nito.Weakness
{
    using ObjectTracking;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class ConcurrentWeakValueDictionary<TKey, TValue> : IWeakDictionary<TKey, TValue> where TValue : class
    {
        private readonly ProjectedDictionary<ConcurrentDictionary<TKey, RegisteredObjectId>, TKey, TKey, TValue, RegisteredObjectId> dictionary;

        private ConcurrentWeakValueDictionary(ConcurrentDictionary<TKey, RegisteredObjectId> dictionary)
        {
            this.dictionary =
                new ProjectedDictionary<ConcurrentDictionary<TKey, RegisteredObjectId>, TKey, TKey, TValue, RegisteredObjectId>(
                    dictionary,
                    x => x.Id.TargetAs<TValue>(),
                    x => new RegisteredObjectId(ObjectTracker.Default.TrackObject(x)),
                    x => x,
                    x => x);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            using (var storedValue = this.StoreValue(key, value).MutableWrapper())
            {
                this.dictionary.Source.AsDictionary().Add(key, storedValue.Value);
                storedValue.Value = null;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.Source.ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return this.dictionary.Source.Keys; }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            RegisteredObjectId value;
            if (this.dictionary.Source.TryRemove(key, out value))
            {
                value.Dispose();
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
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
                this.dictionary.Source.AddOrUpdate(key,
                    _ => this.StoreValue(key, value),
                    (_, oldValue) =>
                    {
                        oldValue.Dispose();
                        return this.StoreValue(key, value);
                    });
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            using (var storedValue = this.StoreValue(item.Key, item.Value).MutableWrapper())
            {
                this.dictionary.Source.AsDictionary().Add(new KeyValuePair<TKey, RegisteredObjectId>(item.Key, storedValue.Value));
                storedValue.Value = null;
            }
        }

        public void Clear()
        {
            // Simply clearing the underlying dictionary is not efficient in terms of unregistering the callbacks,
            // but failure to unregister is benign compared to the concurrency issues of handling Clear any other way.
            this.dictionary.Source.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.dictionary.Source.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return this.dictionary.Source.AsCollection().IsReadOnly; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            // This method does "leak" a registered callback action.
            return this.dictionary.Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> LiveList
        {
            get
            {
                foreach (var kvp in this.dictionary.Source)
                {
                    var value = kvp.Value.Id.TargetAs<TValue>();
                    if (value == null)
                    {
                        this.AsDictionary().Remove(kvp.Key);
                        continue;
                    }

                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                }
            }
        }

        public void Purge()
        {
            foreach (var kvp in this.dictionary.Source.Where(kvp => !kvp.Value.Id.IsAlive))
            {
                this.AsDictionary().Remove(kvp.Key);
            }
        }

        public void Dispose()
        {
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return this.dictionary.ValueMapStoredToExposed(
                this.dictionary.Source.AddOrUpdate(
                    key,
                    k => this.StoreValue(k, addValueFactory(k)),
                    (k, oldStoredValue) =>
                    {
                        var updatedExposedValue = updateValueFactory(k, this.dictionary.ValueMapStoredToExposed(oldStoredValue));
                        oldStoredValue.Dispose();
                        return this.StoreValue(k, updatedExposedValue);
                    }));
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return this.AddOrUpdate(key, _ => addValue, updateValueFactory);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return this.dictionary.ValueMapStoredToExposed(
                this.dictionary.Source.GetOrAdd(
                    key,
                    k => this.StoreValue(k, valueFactory(k))));
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            return this.GetOrAdd(key, _ => value);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            using (var storedValue = this.StoreValue(key, value).MutableWrapper())
            {
                if (this.dictionary.Source.TryAdd(key, storedValue.Value))
                {
                    storedValue.Value = null;
                    return true;
                }

                return false;
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            RegisteredObjectId storedValue;
            if (this.dictionary.Source.TryRemove(key, out storedValue))
            {
                value = this.dictionary.ValueMapStoredToExposed(storedValue);
                storedValue.Dispose();
                return true;
            }

            value = null;
            return false;
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            using (var newStoredValue = this.StoreValue(key, newValue).MutableWrapper())
            {
                var comparisonStoredValue = this.dictionary.ValueMapExposedToStored(comparisonValue);
                if (this.dictionary.Source.TryUpdate(key, newStoredValue.Value, comparisonStoredValue))
                {
                    // This method does "leak" a registered callback action when it returns true.
                    newStoredValue.Value = null;
                    return true;
                }

                return false;
            }
        }

        private RegisteredObjectId StoreValue(TKey key, TValue value)
        {
            return new RegisteredObjectId(ObjectTracker.Default.TrackObject(value), id => this.dictionary.Source.AsDictionary().Remove(new KeyValuePair<TKey, RegisteredObjectId>(key, new RegisteredObjectId(id))));
        }
    }
}