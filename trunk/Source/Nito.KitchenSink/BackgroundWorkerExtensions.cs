namespace Nito.KitchenSink
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for the <see cref="BackgroundWorker"/> class.
    /// </summary>
    public static class BackgroundWorkerExtensions
    {
        /// <summary>
        /// Provides a <see cref="Task"/> interface for <see cref="BackgroundWorker"/>. This wrapper supports successful results, exceptions, and BGW-style cancellation.
        /// </summary>
        /// <param name="this">The <see cref="BackgroundWorker"/> to wrap.</param>
        /// <returns>A <see cref="Task"/> interface for the <see cref="BackgroundWorker"/>.</returns>
        public static Task<object> ToTask(this BackgroundWorker @this)
        {
            var control = new TaskCompletionSource<object>(TaskCreationOptions.LongRunning);
            @this.RunWorkerCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        control.SetException(args.Error);
                    }
                    else if (args.Cancelled)
                    {
                        control.SetCanceled();
                    }
                    else
                    {
                        control.SetResult(args.Result);
                    }
                };
            return control.Task;
        }

        /// <summary>
        /// Provides a <see cref="Task"/> interface for <see cref="BackgroundWorker"/>. This wrapper supports successful results, exceptions, BGW-style cancellation, and Task-style cancellation.
        /// </summary>
        /// <param name="this">The <see cref="BackgroundWorker"/> to wrap.</param>
        /// <param name="cancellationToken">The cancellation token used for Task-style cancellation. When this token is signalled, <see cref="BackgroundWorker.CancelAsync"/> is invoked, and the BGW may use BGW-style or Task-style cancellation.</param>
        /// <returns>A <see cref="Task"/> interface for the <see cref="BackgroundWorker"/>.</returns>
        public static Task<object> ToTask(this BackgroundWorker @this, CancellationToken cancellationToken)
        {
            cancellationToken.Register(@this.CancelAsync);
            var control = new TaskCompletionSource<object>(TaskCreationOptions.LongRunning);
            @this.RunWorkerCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        var oce = args.Error as OperationCanceledException;
                        if (oce != null && oce.CancellationToken == cancellationToken)
                        {
                            control.SetCanceled();
                        }
                        else
                        {
                            control.SetException(args.Error);
                        }
                    }
                    else if (args.Cancelled)
                    {
                        control.SetCanceled();
                    }
                    else
                    {
                        control.SetResult(args.Result);
                    }
                };
            return control.Task;
        }
    }
}
