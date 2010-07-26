using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Nito.Weakness
{
    using ObjectTracking;

    public sealed class ConcurrentWeakValueDictionary<TKey, TValue> : IWeakDictionary<TKey, TValue> where TValue : class
    {
        private readonly ProjectedDictionary<ConcurrentDictionary<TKey, ObjectId>, TKey, TKey, TValue, ObjectId> dictionary;

        private ConcurrentWeakValueDictionary(ConcurrentDictionary<TKey, ObjectId> dictionary)
        {
            this.dictionary =
                new ProjectedDictionary<ConcurrentDictionary<TKey, ObjectId>, TKey, TKey, TValue, ObjectId>(
                    dictionary,
                    x => x.TargetAs<TValue>(),
                    x =>
                    {
                        var ret = ObjectTracker.Default.TrackObject(x);
                        ret.Register(id => this.dictionary.Source.AsDictionary().Remove(new KeyValuePair<TKey, ObjectId>(x, id)));
                        return ret;
                    },
                    x => x,
                    x => x);
        }

        public void Add(TKey key, TValue value)
        {
            this.dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return this.dictionary.Source.ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return this.dictionary.Source.Keys; }
        }

        public bool Remove(TKey key)
        {
            return this.dictionary.Remove(key);
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
                this.dictionary[key] = value;
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            this.dictionary.Add(item);
        }

        public void Clear()
        {
            this.dictionary.Source.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Source.AsDictionary().Contains(
                new KeyValuePair<TKey, ObjectId>(item.Key, ObjectTracker.Default.TrackObject(item.Value)));
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
            return this.dictionary.Source.AsDictionary().Remove(
                new KeyValuePair<TKey, ObjectId>(item.Key, ObjectTracker.Default.TrackObject(item.Value)));
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
                    var value = kvp.Value.TargetAs<TValue>();
                    if (value == null)
                    {
                        this.dictionary.Source.AsDictionary().Remove(kvp.Key);
                        continue;
                    }

                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, value);
                }
            }
        }

        public void Purge()
        {
            foreach (var kvp in this.dictionary.Source.Where(kvp => !kvp.Value.IsAlive))
            {
                this.dictionary.Source.AsDictionary().Remove(kvp.Key);
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
                    k => this.dictionary.ValueMapExposedToStored(addValueFactory(k)),
                    (k, v) => this.dictionary.ValueMapExposedToStored(updateValueFactory(k, this.dictionary.ValueMapStoredToExposed(v)))));
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {

        }


    }
}