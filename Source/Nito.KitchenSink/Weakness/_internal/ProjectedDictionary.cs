using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.Weakness
{
    internal interface ISourceDictionary<TExposedKey, TStoredKey, TExposedValue, TStoredValue> : IDictionary<TExposedKey, TExposedValue>
    {
        IDictionary<TStoredKey, TStoredValue> Source { get; }
    }

    internal sealed class ProjectedDictionary<TExposedKey, TStoredKey, TExposedValue, TStoredValue> : ISourceDictionary<TExposedKey, TStoredKey, TExposedValue, TStoredValue>
    {
        private readonly Func<TStoredValue, TExposedValue> mapStoredValueToExposedValue;

        private readonly Func<TExposedValue, TStoredValue> mapExposedValueToStoredValue;

        private readonly Func<TStoredKey, TExposedKey> mapStoredKeyToExposedKey;

        private readonly Func<TExposedKey, TStoredKey> mapExposedKeyToStoredKey;

        public ProjectedDictionary(
            IDictionary<TStoredKey, TStoredValue> source,
            Func<TStoredValue, TExposedValue> mapValueStoredToExposed,
            Func<TExposedValue, TStoredValue> mapValueExposedToStored,
            Func<TStoredKey, TExposedKey> mapKeyStoredToExposed,
            Func<TExposedKey, TStoredKey> mapKeyExposedToStored)
        {
            this.Source = source;
            this.mapStoredValueToExposedValue = mapValueStoredToExposed;
            this.mapExposedValueToStoredValue = mapValueExposedToStored;
            this.mapStoredKeyToExposedKey = mapKeyStoredToExposed;
            this.mapExposedKeyToStoredKey = mapKeyExposedToStored;
        }

        public IDictionary<TStoredKey, TStoredValue> Source { get; private set; }

        public void Add(TExposedKey key, TExposedValue value)
        {
            this.Source.Add(this.KeyMapExposedToStored(key), this.ValueMapExposedToStored(value));
        }

        public bool ContainsKey(TExposedKey key)
        {
            return this.Source.ContainsKey(this.KeyMapExposedToStored(key));
        }

        ICollection<TExposedKey> IDictionary<TExposedKey, TExposedValue>.Keys
        {
            get { return this.Source.Keys.Select(this.KeyMapStoredToExposed).ToReadOnlyCollection(() => this.Source.Count, this.ContainsKey); }
        }

        public bool Remove(TExposedKey key)
        {
            return this.Source.Remove(this.KeyMapExposedToStored(key));
        }

        public bool TryGetValue(TExposedKey key, out TExposedValue value)
        {
            TStoredValue ret;
            if (!this.Source.TryGetValue(this.KeyMapExposedToStored(key), out ret))
            {
                value = default(TExposedValue);
                return false;
            }

            value = this.ValueMapStoredToExposed(ret);
            return true;
        }

        ICollection<TExposedValue> IDictionary<TExposedKey, TExposedValue>.Values
        {
            get
            {
                return this.Source.Values.Select(this.ValueMapStoredToExposed).ToReadOnlyCollection(() => this.Source.Count, value => this.Source.Values.Contains(this.ValueMapExposedToStored(value)));
            }
        }

        public TExposedValue this[TExposedKey key]
        {
            get
            {
                return this.ValueMapStoredToExposed(this.Source[this.KeyMapExposedToStored(key)]);
            }

            set
            {
                this.Source[this.KeyMapExposedToStored(key)] = this.ValueMapExposedToStored(value);
            }
        }

        void ICollection<KeyValuePair<TExposedKey, TExposedValue>>.Add(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            this.Source.Add(this.KeyMapExposedToStored(item.Key), this.ValueMapExposedToStored(item.Value));
        }

        public void Clear()
        {
            this.Source.Clear();
        }

        bool ICollection<KeyValuePair<TExposedKey, TExposedValue>>.Contains(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            return this.Source.Contains(this.MapExposedToStored(item));
        }

        void ICollection<KeyValuePair<TExposedKey, TExposedValue>>.CopyTo(KeyValuePair<TExposedKey, TExposedValue>[] array, int arrayIndex)
        {
            this.CopyToImpl(array, arrayIndex);
        }

        public int Count
        {
            get { return this.Source.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.Source.IsReadOnly; }
        }

        bool ICollection<KeyValuePair<TExposedKey, TExposedValue>>.Remove(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            return this.Source.Remove(this.MapExposedToStored(item));
        }

        public IEnumerator<KeyValuePair<TExposedKey, TExposedValue>> GetEnumerator()
        {
            return this.Source.Select(this.MapStoredToExposed).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private KeyValuePair<TExposedKey, TExposedValue> MapStoredToExposed(KeyValuePair<TStoredKey, TStoredValue> value)
        {
            return new KeyValuePair<TExposedKey, TExposedValue>(this.KeyMapStoredToExposed(value.Key), this.ValueMapStoredToExposed(value.Value));
        }

        private KeyValuePair<TStoredKey, TStoredValue> MapExposedToStored(KeyValuePair<TExposedKey, TExposedValue> value)
        {
            return new KeyValuePair<TStoredKey, TStoredValue>(this.KeyMapExposedToStored(value.Key), this.ValueMapExposedToStored(value.Value));
        }

        private TExposedValue ValueMapStoredToExposed(TStoredValue value)
        {
            return this.mapStoredValueToExposedValue(value);
        }

        private TStoredValue ValueMapExposedToStored(TExposedValue value)
        {
            return this.mapExposedValueToStoredValue(value);
        }

        private TExposedKey KeyMapStoredToExposed(TStoredKey key)
        {
            return this.mapStoredKeyToExposedKey(key);
        }

        private TStoredKey KeyMapExposedToStored(TExposedKey key)
        {
            return this.mapExposedKeyToStoredKey(key);
        }
    }
}
