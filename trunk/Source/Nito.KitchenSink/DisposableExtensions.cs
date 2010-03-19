using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Provides extension methods for <see cref="IDisposable"/> types.
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Wraps this disposable object in a reference-counted disposable wrapper.
        /// </summary>
        /// <typeparam name="T">The type of the underlying disposable object.</typeparam>
        /// <param name="disposable">The underlying disposable object to wrap.</param>
        /// <returns>A reference-counted disposable wrapper.</returns>
        public static ReferenceCountedDisposable<T> ReferenceCounted<T>(this T disposable) where T : IDisposable
        {
            return new ReferenceCountedDisposable<T>(disposable);
        }
    }
}
