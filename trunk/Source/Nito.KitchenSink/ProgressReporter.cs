namespace Nito.KitchenSink
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A class used by Tasks to report progress or completion updates back to the UI.
    /// </summary>
    public sealed class ProgressReporter
    {
        /// <summary>
        /// The underlying scheduler for the UI's synchronization context.
        /// </summary>
        private readonly TaskScheduler scheduler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressReporter"/> class. This should be run on a UI thread.
        /// </summary>
        public ProgressReporter()
        {
            this.scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        /// <summary>
        /// Reports the progress to the UI thread. Note that the progress update is asynchronous with respect to the reporting Task. For a synchronous progress update, wait on the returned <see cref="Task"/>.
        /// </summary>
        /// <param name="action">The action to perform in the context of the UI thread. Note that this action is run asynchronously on the UI thread.</param>
        /// <returns>The task queued to the UI thread.</returns>
        public Task ReportProgressAsync(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, this.scheduler);
        }

        /// <summary>
        /// Reports the progress to the UI thread, and waits for the UI thread to process the update before returning.
        /// </summary>
        /// <param name="action">The action to perform in the context of the UI thread.</param>
        public void ReportProgress(Action action)
        {
            this.ReportProgressAsync(action).Wait();
        }
    }
}