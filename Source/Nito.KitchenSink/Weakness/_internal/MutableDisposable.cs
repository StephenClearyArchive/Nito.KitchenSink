// <copyright file="MutableDisposable.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;

    /// <summary>
    /// A disposable wrapper for an optional disposable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class MutableDisposable<T> : IDisposable where T : class, IDisposable
    {
        /// <summary>
        /// The underlying disposable value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Disposes the underlying disposable value, unless it is <c>null</c>.
        /// </summary>
        public void Dispose()
        {
            if (this.Value != null)
            {
                this.Value.Dispose();
            }
        }
    }
}
