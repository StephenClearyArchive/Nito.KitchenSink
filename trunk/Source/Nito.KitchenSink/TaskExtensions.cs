﻿// <copyright file="TaskExtensions.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for the <see cref="Task"/> class, and task-related functionality.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Returns a delegate that invokes the original delegate asynchronously (on a ThreadPool thread), applying a timeout and supporting cancellation. If timeout or cancellation occurs, the delegate runs to completion but its results and any exceptions it raises are ignored.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the delegate.</typeparam>
        /// <param name="function">The delegate to invoke asynchronously.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> to wait indefinitely.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting.</param>
        /// <returns>A delegate that invokes the original delegate asynchronously with a timeout and supporting cancellation.</returns>
        public static Func<TResult> Async<TResult>(this Func<TResult> function, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return () =>
            {
                using (var signalOuter = new ManualResetEvent(false).ReferenceCounted())
                {
                    TResult result = default(TResult);
                    Exception error = null;
                    var signalInner = signalOuter.AddReference();
                    IAsyncResult asyncResult = function.BeginInvoke(
                        innerResult =>
                        {
                            try
                            {
                                result = function.EndInvoke(innerResult);
                            }
                            catch (Exception ex)
                            {
                                error = ex;
                            }

                            signalInner.Value.Set();
                            ((IDisposable)signalInner).Dispose();
                        },
                        null);

                    if (asyncResult.AsyncWaitHandle.WaitOne(millisecondsTimeout, cancellationToken))
                    {
                        signalOuter.Value.WaitOne();
                        if (error != null)
                        {
                            throw error.PrepareForRethrow();
                        }

                        return result;
                    }
                    else
                    {
                        throw new TimeoutException("Task did not complete in a timely manner.");
                    }
                }
            };
        }

        /// <summary>
        /// Waits for a handle to be signalled, allowing cancellation.
        /// </summary>
        /// <param name="waitHandle">The wait handle to observe.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="OperationCanceledException">The cancellation token was signalled before the operation completed.</exception>
        public static void WaitOne(this WaitHandle waitHandle, CancellationToken cancellationToken)
        {
            waitHandle.WaitOne(Timeout.Infinite, cancellationToken);
        }

        /// <summary>
        /// Waits for a handle to be signalled for a specified time, allowing cancellation. Returns <c>true</c> if the handle was signalled, and <c>false</c> if there was a timeout.
        /// </summary>
        /// <param name="waitHandle">The wait handle to observe.</param>
        /// <param name="millisecondsTimeout">The amount of time to wait for the handle to be signalled, in milliseconds; or <see cref="Timeout.Infinite"/> for an infinite wait.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="OperationCanceledException">The cancellation token was signalled before the operation completed.</exception>
        /// <returns>Returns <c>true</c> if the handle was signalled, and <c>false</c> if there was a timeout.</returns>
        public static bool WaitOne(this WaitHandle waitHandle, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var waitFor = new WaitHandle[] { waitHandle, cancellationToken.WaitHandle };
            int result = WaitHandle.WaitAny(waitFor, millisecondsTimeout);
            return result != WaitHandle.WaitTimeout;
        }
    }
}
