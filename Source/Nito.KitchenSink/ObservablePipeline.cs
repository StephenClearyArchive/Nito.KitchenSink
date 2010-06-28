using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
#if NO
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

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

        public static IObservable<T> GetConsumingObservable<T>(this BlockingCollection<T> collection)
        {
            return collection.GetConsumingObservable(CancellationToken.None);
        }

        public static IObservable<T> GetConsumingObservable<T>(this BlockingCollection<T> collection, CancellationToken cancellationToken)
        {
            var ret = new Subject<T>();
            var privateCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken).Token;
            Task.Factory.StartNew(() =>
            {
                foreach (var item in collection.GetConsumingEnumerable(privateCancellationToken))
                {
                    ret.OnNext(item);
                }
            }, privateCancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    ret.OnError(new OperationCanceledException(cancellationToken));
                }
                else if (task.IsFaulted)
                {
                    ret.OnError(task.Exception);
                }
                else
                {
                    ret.OnCompleted();
                }
            });

            return ret;
        }
    }
#endif
}
