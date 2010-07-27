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
        private readonly ProjectedDictionary<TrackedConcurrentDictionary<TKey, RegisteredObjectId>, TKey, TKey, TValue, RegisteredObjectId> dictionary;

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
                this.dictionary.Source.Source.AsDictionary().Add(key, storedValue.Value);
                GC.KeepAlive(value);
                storedValue.Value = null;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.Source.Source.ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return this.dictionary.Source.Source.Keys; }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return this.dictionary.Source.Remove(key);
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
                this.dictionary.Source[key] = this.StoreValue(key, value);
                GC.KeepAlive(value);
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            using (var storedValue = this.StoreValue(item.Key, item.Value).MutableWrapper())
            {
                this.dictionary.Source.Add(new KeyValuePair<TKey, RegisteredObjectId>(item.Key, storedValue.Value));
                GC.KeepAlive(item.Value);
                storedValue.Value = null;
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            // Simply clearing the underlying dictionary is not efficient in terms of unregistering the callbacks,
            // but failure to unregister is benign compared to the concurrency issues of handling Clear any other way.
            this.dictionary.Source.Source.Clear();
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
            get { return this.dictionary.Source.Source.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return this.dictionary.Source.Source.AsCollection().IsReadOnly; }
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
                return this.Where(kvp => kvp.Value != null);
            }
        }

        public void Purge()
        {
        }

        public void Dispose()
        {
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            // TODO: unresolvable race condition between GC thread and value factories...
            return this.dictionary.ValueMapStoredToExposed(
                this.dictionary.Source.AddOrUpdate(
                    key,
                    k => this.StoreValue(k, addValueFactory(k)),
                    (k, oldStoredValue) => this.StoreValue(k, updateValueFactory(k, this.dictionary.ValueMapStoredToExposed(oldStoredValue)))));
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            // TODO: unresolvable race condition between GC thread and value factories...
            return this.AddOrUpdate(key, _ => addValue, updateValueFactory);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            // TODO: unresolvable race condition between GC thread and value factories...
            return this.dictionary.ValueMapStoredToExposed(
                this.dictionary.Source.GetOrAdd(
                    key,
                    k => this.StoreValue(k, valueFactory(k))));
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            var ret = this.GetOrAdd(key, _ => value);
            GC.KeepAlive(value);
            return ret;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            using (var storedValue = this.StoreValue(key, value).MutableWrapper())
            {
                if (this.dictionary.Source.TryAdd(key, storedValue.Value))
                {
                    storedValue.Value = null;
                    GC.KeepAlive(value);
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
                    GC.KeepAlive(newValue);
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