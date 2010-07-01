using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

#if NO
    internal interface IObservablePipeline<T> : IObservable<T>
    {
        IObservable<Notification<T>> GetConsumingObservable();
        IEnumerable<Notification<T>> GetConsumingEnumerable();
    }

    internal abstract class PipelineStageBase<T> : IObservablePipeline<T>
    {
        private readonly BlockingCollection<Notification<T>> buffer;

        protected PipelineStageBase(int boundedCapacity)
        {
            this.buffer = boundedCapacity == 0 ? new BlockingCollection<Notification<T>>() : new BlockingCollection<Notification<T>>(boundedCapacity);
        }

        IObservable<Notification<T>> IObservablePipeline<T>.GetConsumingObservable()
        {
            return this.buffer.GetConsumingObservable();
        }

        IEnumerable<Notification<T>> IObservablePipeline<T>.GetConsumingEnumerable()
        {
            return this.buffer.GetConsumingEnumerable();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.buffer.GetConsumingObservable().Dematerialize().Subscribe(observer);
        }

        // TODO: other Add/TryAdd methods
        protected void Add(T item)
        {
            this.buffer.Add(EnumerableEx.Return(item).Materialize().First());
        }
    }

    internal sealed class SimplePipelineStage<TInput, TOutput> : PipelineStageBase<TOutput>
    {
        private readonly Func<TInput, TOutput> transform;
        private readonly Task task;

        public SimplePipelineStage(int boundedCapacity, Func<TInput, TOutput> transform, IObservablePipeline<TInput> source)
            : base(boundedCapacity)
        {
            this.transform = transform;
            var enumerator = source.GetConsumingEnumerable();
            this.task = Task.Factory.StartNew(() =>
            {

            });
        }


    }
#endif

    internal class SubjectWithSubscriptionNotification<T> : ISubject<T>
    {
        private readonly Subject<T> subject;

        public SubjectWithSubscriptionNotification()
        {
            this.subject = new Subject<T>();
        }

        public event Action Subscribed;

        public void OnCompleted()
        {
            this.subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            this.subject.OnError(error);
        }

        public void OnNext(T value)
        {
            this.subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            IDisposable ret = this.subject.Subscribe(observer);
            if (this.Subscribed != null)
            {
                this.Subscribed();
            }

            return ret;
        }
    }

    internal interface IPublishingTask<T>
    {
        Task Task { get; }
        IObservable<T> Observable { get; }
    }

    internal sealed class AnonymousPublishingTask<T> : IPublishingTask<T>
    {
        public Task Task { get; set; }
        public IObservable<T> Observable { get; set; }
    }

    internal static class RxExtensions
    {
        /// <summary>
        /// Subscribes a blocking collection to an observable sequence. Values from the observable sequence are added to the blocking collection. Exceptions from the observable sequence are ignored. Generally, a materialized observable sequence is stored in a collection.
        /// </summary>
        /// <typeparam name="T">The type of the items in the observable sequence. This is normally a <see cref="Notification{T}"/> type.</typeparam>
        /// <param name="this">The observable sequence to which to subscribe.</param>
        /// <param name="collection">The blocking collection that receives values from the observable sequence.</param>
        /// <returns>The subscription, as a disposable object.</returns>
        public static IDisposable Subscribe<T>(this IObservable<T> @this, BlockingCollection<T> collection)
        {
            return @this.Subscribe(collection.Add, _ => collection.CompleteAdding(), collection.CompleteAdding);
        }

        public static IDisposable Subscribe<T>(this IObservable<T> @this, BlockingCollection<T> collection, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static IPublishingTask<T> GetConsumingObservable<T>(this BlockingCollection<T> collection)
        {
            return collection.GetConsumingPublishingTask(CancellationToken.None);
        }

        public static IPublishingTask<T> GetConsumingPublishingTask<T>(this BlockingCollection<T> collection, CancellationToken cancellationToken)
        {
            var privateCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken).Token;
            var subject = new Subject<T>();
            var ret = new Task(
                () =>
                {
                    foreach (var item in collection.GetConsumingEnumerable(privateCancellationToken))
                    {
                        subject.OnNext(item);
                    }
                },
                privateCancellationToken).ContinueWith(
                    task =>
                    {
                        if (task.IsCanceled)
                        {
                            subject.OnError(new OperationCanceledException(cancellationToken));
                        }
                        else if (task.IsFaulted)
                        {
                            subject.OnError(task.Exception);
                        }
                        else
                        {
                            subject.OnCompleted();
                        }
                    });

            return new AnonymousPublishingTask<T> { Task = ret, Observable = subject };
        }
    }
}
