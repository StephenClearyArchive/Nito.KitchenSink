using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.Weakness
{
    public interface IWeakValueDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IWeakCollection<KeyValuePair<TKey, TValue>>, IDisposable
    {

    }

    public sealed class WeakValueDictionary<TKey, TValue> : IWeakValueDictionary<TKey, TValue> where TValue : class
    {
        // TODO: dispose on Remove/Clear
        private readonly ISourceDictionary<TKey, TKey, TValue, EquatableWeakReference<TValue>> dictionary;

        private WeakValueDictionary(IDictionary<TKey, EquatableWeakReference<TValue>> dictionary)
        {
            this.dictionary = dictionary.SelectValue(x => x.Target, x => new EquatableWeakReference<TValue>(x));
        }

        public void Add(TKey key, TValue value)
        {
            this.dictionary.Add(key, value);
        }

        // Note: makes no guarantees that the value is alive
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
            EquatableWeakReference<TValue> value;
            if (this.dictionary.Source.TryGetValue(key, out value))
            {
                value.Dispose();
                return this.dictionary.Source.Remove(key);
            }

            return false;
        }

        // Note: may return true and set value to null if the value is dead
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
            foreach (var kvp in this.dictionary.Source)
            {
                kvp.Value.Dispose();
            }

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
            get { return this.dictionary.Source.IsReadOnly; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
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

        private IEnumerable<KeyValuePair<TKey, TValue>> PurgingEnumeration()
        {
            var purgedValues = new List<TKey>();

            foreach (var kvp in this.dictionary.Source)
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

        public IEnumerable<KeyValuePair<TKey, TValue>> LiveList
        {
            get { return this.PurgingEnumeration(); }
        }

        public void Purge()
        {
            var purgedValues = (from kvp in this.dictionary.Source where !kvp.Value.IsAlive select kvp.Key).ToList();

            foreach (var key in purgedValues)
            {
                this.Remove(key);
            }
        }

        public void Dispose()
        {
            this.Clear();
        }
    }
}
