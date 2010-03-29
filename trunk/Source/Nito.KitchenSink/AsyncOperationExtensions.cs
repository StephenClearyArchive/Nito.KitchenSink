// <copyright file="AsyncOperationExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    /// <summary>
    /// Provides methods useful when dealing with <see cref="AsyncOperation"/> instances.
    /// </summary>
    public static class AsyncOperationExtensions
    {
        /// <summary>
        /// Invokes a parameterless delegate using the captured <see cref="System.Threading.SynchronizationContext"/>.
        /// </summary>
        /// <param name="asyncOperation">The asynchronous operation that holds the captured <see cref="System.Threading.SynchronizationContext"/>.</param>
        /// <param name="method">The parameterless delegate to invoke.</param>
        public static void Post(this AsyncOperation asyncOperation, Action method)
        {
            asyncOperation.Post(_ => method(), null);
        }

        /// <summary>
        /// Invokes a parameterless delegate using the captured <see cref="System.Threading.SynchronizationContext"/>, and ends the asynchronous operation.
        /// </summary>
        /// <param name="asyncOperation">The asynchronous operation that holds the captured <see cref="System.Threading.SynchronizationContext"/>.</param>
        /// <param name="method">The parameterless delegate to invoke.</param>
        public static void PostOperationCompleted(this AsyncOperation asyncOperation, Action method)
        {
            asyncOperation.PostOperationCompleted(_ => method(), null);
        }

        /// <summary>
        /// Invokes a parameterless delegate.
        /// </summary>
        /// <param name="synchronizationContext">The synchronization context on which to post the delegate.</param>
        /// <param name="method">The parameterless delegate to invoke.</param>
        public static void Post(this SynchronizationContext synchronizationContext, Action method)
        {
            synchronizationContext.Post(_ => method(), null);
        }

        /// <summary>
        /// Invokes a parameterless delegate.
        /// </summary>
        /// <param name="synchronizationContext">The synchronization context on which to post the delegate.</param>
        /// <param name="method">The parameterless delegate to invoke.</param>
        public static void Send(this SynchronizationContext synchronizationContext, Action method)
        {
            synchronizationContext.Send(_ => method(), null);
        }
    }
}
