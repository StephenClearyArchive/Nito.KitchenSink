// <copyright file="SafeDisposable.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;

    /// <summary>
    /// Provides a non-throwing implementation of <see cref="IDisposable.Dispose"/> for a wrapped disposable object.
    /// </summary>
    /// <typeparam name="T">The type of object to wrap.</typeparam>
    public sealed class SafeDisposable<T> : IDisposable where T : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeDisposable&lt;T&gt;"/> class, wrapping the specified disposable object.
        /// </summary>
        /// <param name="value">The disposable object to wrap.</param>
        public SafeDisposable(T value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the underlying disposable object.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Disposes the underlying disposable object, swallowing all exceptions.
        /// </summary>
        void IDisposable.Dispose()
        {
            try
            {
                this.Value.Dispose();
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
                // Normally, an empty catch block is a Really Bad Idea.
                // In this case, it's necessary to work around broken code from Microsoft (WCF client proxies, in particular).
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return Convert.ToString(this.Value);
        }
    }
}
