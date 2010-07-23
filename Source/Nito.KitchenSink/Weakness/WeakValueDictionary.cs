using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Weakness
{
    public interface IWeakValueDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IWeakCollection<KeyValuePair<TKey, TValue>>, IDisposable
    {

    }

    public sealed class WeakValueDictionary<TKey, TValue> : IWeakValueDictionary<TKey, TValue> where TValue : class
    {
        private readonly Dictionary<TKey, WeakReference<TValue>> dictionary = new Dictionary<TKey, WeakReference<TValue>>();

        public void Add(TKey key, TValue value)
        {
            this.dictionary.Add(key, new WeakReference<TValue>(value));
        }

        // Note: makes no guarantees that the value is alive
        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return this.dictionary.Keys; }
        }

        public bool Remove(TKey key)
        {
            return this.dictionary.Remove(key);
        }

        // Note: may return true and set value to null if the value is dead
        public bool TryGetValue(TKey key, out TValue value)
        {
            WeakReference<TValue> weakValue;
            if (!this.dictionary.TryGetValue(key, out weakValue))
            {
                value = default(TValue);
                return false;
            }

            value = weakValue.Target;
            return true;
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return this.dictionary.Values.Select(weakValue => weakValue.Target).ToList(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                WeakReference<TValue> ret = this.dictionary[key];
                return ret.Target;
            }

            set
            {

                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> LiveList
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> CompleteList
        {
            get { throw new NotImplementedException(); }
        }

        public int CompleteCount
        {
            get { throw new NotImplementedException(); }
        }

        public int DeadCount
        {
            get { throw new NotImplementedException(); }
        }

        public int LiveCount
        {
            get { throw new NotImplementedException(); }
        }

        public int LiveCountWithoutPurge
        {
            get { throw new NotImplementedException(); }
        }

        public void Purge()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
