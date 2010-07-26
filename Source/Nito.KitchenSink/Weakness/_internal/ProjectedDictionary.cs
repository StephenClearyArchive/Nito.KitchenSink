// <copyright file="ProjectedDictionary.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ProjectedDictionary<TSource, TExposedKey, TStoredKey, TExposedValue, TStoredValue> : IDictionary<TExposedKey, TExposedValue> where TSource : IDictionary<TStoredKey, TStoredValue>
    {
        private readonly Func<TStoredValue, TExposedValue> mapStoredValueToExposedValue;

        private readonly Func<TExposedValue, TStoredValue> mapExposedValueToStoredValue;

        private readonly Func<TStoredKey, TExposedKey> mapStoredKeyToExposedKey;

        private readonly Func<TExposedKey, TStoredKey> mapExposedKeyToStoredKey;

        public ProjectedDictionary(
            TSource source,
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

        public TSource Source { get; private set; }

        public void Add(TExposedKey key, TExposedValue value)
        {
            this.Source.Add(this.KeyMapExposedToStored(key), this.ValueMapExposedToStored(value));
        }

        public bool ContainsKey(TExposedKey key)
        {
            return this.Source.ContainsKey(this.KeyMapExposedToStored(key));
        }

        public ICollection<TExposedKey> Keys
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

        public ICollection<TExposedValue> Values
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

        public void Add(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            this.Source.Add(this.KeyMapExposedToStored(item.Key), this.ValueMapExposedToStored(item.Value));
        }

        public void Clear()
        {
            this.Source.Clear();
        }

        public bool Contains(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            return this.Source.Contains(this.MapExposedToStored(item));
        }

        public void CopyTo(KeyValuePair<TExposedKey, TExposedValue>[] array, int arrayIndex)
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

        public bool Remove(KeyValuePair<TExposedKey, TExposedValue> item)
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

        public KeyValuePair<TExposedKey, TExposedValue> MapStoredToExposed(KeyValuePair<TStoredKey, TStoredValue> value)
        {
            return new KeyValuePair<TExposedKey, TExposedValue>(this.KeyMapStoredToExposed(value.Key), this.ValueMapStoredToExposed(value.Value));
        }

        public KeyValuePair<TStoredKey, TStoredValue> MapExposedToStored(KeyValuePair<TExposedKey, TExposedValue> value)
        {
            return new KeyValuePair<TStoredKey, TStoredValue>(this.KeyMapExposedToStored(value.Key), this.ValueMapExposedToStored(value.Value));
        }

        public TExposedValue ValueMapStoredToExposed(TStoredValue value)
        {
            return this.mapStoredValueToExposedValue(value);
        }

        public TStoredValue ValueMapExposedToStored(TExposedValue value)
        {
            return this.mapExposedValueToStoredValue(value);
        }

        public TExposedKey KeyMapStoredToExposed(TStoredKey key)
        {
            return this.mapStoredKeyToExposedKey(key);
        }

        public TStoredKey KeyMapExposedToStored(TExposedKey key)
        {
            return this.mapExposedKeyToStoredKey(key);
        }
    }
}
