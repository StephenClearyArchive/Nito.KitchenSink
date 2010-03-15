// <copyright file="A.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;

    /// <summary>
    /// Provides static creation methods for anonymous types.
    /// </summary>
    public static class A
    {
        /// <summary>
        /// Creates and returns an <see cref="AnonymousComparer{T}"/> using the specified delegate.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="compare">Compares two objects and returns a value less than 0 if its first argument is less than its second argument, 0 if its two arguments are equal, or greater than 0 if its first argument is greater than its second argument.</param>
        /// <returns>An <see cref="AnonymousComparer{T}"/>.</returns>
        public static AnonymousComparer<T> Comparer<T>(Func<T, T, int> compare)
        {
            return new AnonymousComparer<T> { Compare = compare };
        }
    }
}
