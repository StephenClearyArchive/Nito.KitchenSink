// <copyright file="ConcurrentWeakDictionary.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A concurrent dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Adds or updates the value for a key.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="addValueFactory">The generator of the value to add, if the key does not already exist.</param>
        /// <param name="updateValueFactory">The generator of the updated value, if the key does already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the return value of <paramref name="updateValueFactory"/>; otherwise, this is the return value of <paramref name="addValueFactory"/>.</returns>
        TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory);

        /// <summary>
        /// Adds or updates the value for a key.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="addValue">The value to add, if the key does not already exist.</param>
        /// <param name="updateValueFactory">The generator of the updated value, if the key does already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the return value of <paramref name="updateValueFactory"/>; otherwise, this is <paramref name="addValue"/>.</returns>
        TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory);

        /// <summary>
        /// Adds a key and value if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="valueFactory">The generator of the value to add, if the key does not already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the existing value; otherwise, this is the return value of <paramref name="valueFactory"/>.</returns>
        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

        /// <summary>
        /// Adds a key and value if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to add or lookup.</param>
        /// <param name="value">The value to add, if the key does not already exist.</param>
        /// <returns>The new value for the key. If the key already existed, this is the existing value; otherwise, this is <paramref name="value"/>.</returns>
        TValue GetOrAdd(TKey key, TValue value);

        /// <summary>
        /// Attempts to add a key and value.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        /// <returns><c>true</c> if the key and value were added; <c>false</c> if the key already existed.</returns>
        bool TryAdd(TKey key, TValue value);

        /// <summary>
        /// Attempts to atomically retrieve and remove the value for a specified key.
        /// </summary>
        /// <param name="key">The key whose value is removed and returned.</param>
        /// <param name="value">On return, contains the value removed, or the default value if the key was not found.</param>
        /// <returns><c>true</c> if the value was removed; <c>false</c> if the key was not found.</returns>
        bool TryRemove(TKey key, out TValue value);

        /// <summary>
        /// Atomically looks up the existing value for the specified key, compares it with the specified value, and (if they are equal) updates the key with a third value.
        /// </summary>
        /// <param name="key">The key used to lookup the existing value.</param>
        /// <param name="newValue">The new value for the key, if the old value is equal to <paramref name="comparisonValue"/>.</param>
        /// <param name="comparisonValue">The value to compare against the existing value of <paramref name="key"/>.</param>
        /// <returns><c>true</c> if the existing value for the key was equal to <paramref name="comparisonValue"/> and was replaced with <paramref name="newValue"/>; otherwise, <c>false</c>.</returns>
        bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue);
    }

    /// <summary>
    /// A concurrent dictionary that has weak references to its keys, its values, or both keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IConcurrentWeakDictionary<TKey, TValue> : IWeakDictionary<TKey, TValue>, IConcurrentDictionary<TKey, TValue>
    {
    }
}
