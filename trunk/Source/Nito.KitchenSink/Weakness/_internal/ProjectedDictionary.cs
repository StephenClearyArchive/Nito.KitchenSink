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
            this.WithoutProjection = source;
            this.mapStoredValueToExposedValue = mapValueStoredToExposed;
            this.mapExposedValueToStoredValue = mapValueExposedToStored;
            this.mapStoredKeyToExposedKey = mapKeyStoredToExposed;
            this.mapExposedKeyToStoredKey = mapKeyExposedToStored;
        }

        public TSource WithoutProjection { get; private set; }

        public void Add(TExposedKey key, TExposedValue value)
        {
            this.WithoutProjection.Add(this.KeyMapExposedToStored(key), this.ValueMapExposedToStored(value));
        }

        public bool ContainsKey(TExposedKey key)
        {
            return this.WithoutProjection.ContainsKey(this.KeyMapExposedToStored(key));
        }

        public ICollection<TExposedKey> Keys
        {
            get { return this.WithoutProjection.Keys.Select(this.KeyMapStoredToExposed).ToReadOnlyCollection(() => this.WithoutProjection.Count, this.ContainsKey); }
        }

        public bool Remove(TExposedKey key)
        {
            return this.WithoutProjection.Remove(this.KeyMapExposedToStored(key));
        }

        public bool TryGetValue(TExposedKey key, out TExposedValue value)
        {
            TStoredValue ret;
            if (!this.WithoutProjection.TryGetValue(this.KeyMapExposedToStored(key), out ret))
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
                return this.WithoutProjection.Values.Select(this.ValueMapStoredToExposed).ToReadOnlyCollection(() => this.WithoutProjection.Count, value => this.WithoutProjection.Values.Contains(this.ValueMapExposedToStored(value)));
            }
        }

        public TExposedValue this[TExposedKey key]
        {
            get
            {
                return this.ValueMapStoredToExposed(this.WithoutProjection[this.KeyMapExposedToStored(key)]);
            }

            set
            {
                this.WithoutProjection[this.KeyMapExposedToStored(key)] = this.ValueMapExposedToStored(value);
            }
        }

        public void Add(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            this.WithoutProjection.Add(this.KeyMapExposedToStored(item.Key), this.ValueMapExposedToStored(item.Value));
        }

        public void Clear()
        {
            this.WithoutProjection.Clear();
        }

        public bool Contains(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            return this.WithoutProjection.Contains(this.PairMapExposedToStored(item));
        }

        public void CopyTo(KeyValuePair<TExposedKey, TExposedValue>[] array, int arrayIndex)
        {
            this.CopyToImpl(array, arrayIndex);
        }

        public int Count
        {
            get { return this.WithoutProjection.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.WithoutProjection.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<TExposedKey, TExposedValue> item)
        {
            return this.WithoutProjection.Remove(this.PairMapExposedToStored(item));
        }

        public IEnumerator<KeyValuePair<TExposedKey, TExposedValue>> GetEnumerator()
        {
            return this.WithoutProjection.Select(this.PairMapStoredToExposed).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public KeyValuePair<TExposedKey, TExposedValue> PairMapStoredToExposed(KeyValuePair<TStoredKey, TStoredValue> value)
        {
            return new KeyValuePair<TExposedKey, TExposedValue>(this.KeyMapStoredToExposed(value.Key), this.ValueMapStoredToExposed(value.Value));
        }

        public KeyValuePair<TStoredKey, TStoredValue> PairMapExposedToStored(KeyValuePair<TExposedKey, TExposedValue> value)
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
