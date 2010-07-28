// <copyright file="AnonymousDisposable.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;

    /// <summary>
    /// An object that implements <see cref="IDisposable"/> using a delegate.
    /// </summary>
    internal sealed class AnonymousDisposable : IDisposable
    {
        /// <summary>
        /// Gets or sets the Dispose delegate, which implements <see cref="IDisposable.Dispose"/>.
        /// </summary>
        public Action Dispose { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose();
        }
    }
}
