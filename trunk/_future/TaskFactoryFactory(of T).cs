// <copyright file="TaskFactoryFactory(of T).cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A factory for task factories. This class provides optional properties which may be set, and a single <see cref="Factory"/> class which exposes the resulting task factory.
    /// </summary>
    /// <typeparam name="TResult">The type of the result of tasks created by <see cref="Factory"/>.</typeparam>
    public sealed class TaskFactoryFactory<TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskFactoryFactory{TResult}"/> class with the default options.
        /// </summary>
        public TaskFactoryFactory()
        {
            this.Factory = Task<TResult>.Factory;
        }

        /// <summary>
        /// Gets the task factory that represents the current value of all the options.
        /// </summary>
        public TaskFactory<TResult> Factory { get; private set; }

        /// <summary>
        /// Gets or sets the cancellation token assigned by default to tasks created by <see cref="Factory"/>. This may be <c>null</c>.
        /// </summary>
        public CancellationToken CancellationToken
        {
            get
            {
                return this.Factory.CancellationToken;
            }

            set
            {
                this.Factory = new TaskFactory<TResult>(value, this.Factory.CreationOptions, this.Factory.ContinuationOptions, this.Factory.Scheduler);
            }
        }

        /// <summary>
        /// Gets or sets the creation options assigned by default to tasks created by <see cref="Factory"/>.
        /// </summary>
        public TaskCreationOptions CreationOptions
        {
            get
            {
                return this.Factory.CreationOptions;
            }

            set
            {
                this.Factory = new TaskFactory<TResult>(this.Factory.CancellationToken, value, this.Factory.ContinuationOptions, this.Factory.Scheduler);
            }
        }

        /// <summary>
        /// Gets or sets the continuation options assigned by default to tasks created by <see cref="Factory"/>.
        /// </summary>
        public TaskContinuationOptions ContinuationOptions
        {
            get
            {
                return this.Factory.ContinuationOptions;
            }

            set
            {
                this.Factory = new TaskFactory<TResult>(this.Factory.CancellationToken, this.Factory.CreationOptions, value, this.Factory.Scheduler);
            }
        }

        /// <summary>
        /// Gets or sets the scheduler assigned by default to tasks created by <see cref="Factory"/>. This may be <c>null</c>.
        /// </summary>
        public TaskScheduler Scheduler
        {
            get
            {
                return this.Factory.Scheduler;
            }

            set
            {
                this.Factory = new TaskFactory<TResult>(this.Factory.CancellationToken, this.Factory.CreationOptions, this.Factory.ContinuationOptions, value);
            }
        }
    }
}
