// <copyright file="ProjectedCollection.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Utility
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// An observable collection of results of applying a property path to a source observable collection.
    /// </summary>
    /// <remarks>
    /// <para>When enumerating this collection, null values will be enumerated if there are binding errors.</para>
    /// <para>The two important properties of this class are <see cref="SourceCollection"/> and <see cref="Path"/>. <see cref="SourceCollection"/> specifies the source collection; for best results, it should implement <see cref="INotifyCollectionChanged"/>. <see cref="Path"/> specifies a simple property path to apply on each element of <see cref="SourceCollection"/>; see <see cref="SimplePropertyPath"/> for details.</para>
    /// </remarks>
    public sealed class ProjectedCollection : NotifyPropertyChangedBase<ProjectedCollection>,
        INotifyCollectionChanged, IList, ICollection, IEnumerable, IDisposable
    {
        /// <summary>
        /// The <see cref="SimplePropertyPath"/> subscription and last known value for each object in the source collection. This is kept in sync with <see cref="sourceCollection"/>.
        /// </summary>
        private List<Subscription> subscriptions;

        /// <summary>
        /// The actual source collection. This same object usually implements <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        private IEnumerable sourceCollection;

        /// <summary>
        /// The subscription for source collection changes. If this is not null, then <see cref="sourceCollection"/> is not null and implements <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        private NotifyCollectionChangedEventHandler sourceCollectionChanged;

        /// <summary>
        /// Backing field for <see cref="CollectionChanged"/>.
        /// </summary>
        private NotifyCollectionChangedEventHandler collectionChanged;

        /// <summary>
        /// Backing field for <see cref="Path"/>.
        /// </summary>
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectedCollection"/> class.
        /// </summary>
        public ProjectedCollection()
        {
            this.subscriptions = new List<Subscription>();
        }

        /// <summary>
        /// Provides notification when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { this.collectionChanged += value; }
            remove { this.collectionChanged -= value; }
        }

        /// <summary>
        /// Gets the number of items in this collection.
        /// </summary>
        public int Count
        {
            get { return this.subscriptions.Count; }
        }

        /// <summary>
        /// Gets or sets the source collection of this projected collection. <see cref="ProjectedCollection{T}"/> is more efficient if the source collection implements <see cref="IList"/> and <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        public IEnumerable SourceCollection
        {
            get
            {
                return this.sourceCollection;
            }

            set
            {
                this.Dismantle();
                this.sourceCollection = value;
                this.Construct();

                this.OnPropertyChanged(x => x.SourceCollection);
                this.OnItemsPropertyChanged();
                this.OnPropertyChanged(x => x.Count);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Gets or sets the property path for this projected collection.
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }

            set
            {
                this.Dismantle();
                this.path = value;
                this.Construct();

                this.OnPropertyChanged(x => x.Path);
                this.OnItemsPropertyChanged();
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        #region Semi-deprecated interface properties

        /// <summary>
        /// Gets a value indicating whether access is synchronized. Always returns false.
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access. Always returns the current instance.
        /// </summary>
        object ICollection.SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is of a fixed size. Always returns false.
        /// </summary>
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read only. Always returns false.
        /// </summary>
        bool IList.IsReadOnly
        {
            get { return false; }
        }

        #endregion

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to retrieve.</param>
        /// <returns>The item at that index, or null if there was a binding error.</returns>
        public object this[int index]
        {
            get
            {
                return this.subscriptions[index].SimplePropertyPath.Value;
            }

            set
            {
                this.subscriptions[index].SimplePropertyPath.Value = value;
            }
        }

        /// <summary>
        /// Unsubscribes from all collection and property notifications.
        /// </summary>
        public void Dispose()
        {
            this.Dismantle();
        }

        /// <summary>
        /// Forces a reevaluation of each element in the collection and the collection itself.
        /// </summary>
        /// <remarks>
        /// <para>Normally, this method will not be needed. It is only needed if <see cref="SourceCollection"/> does not support <see cref="INotifyCollectionChanged"/> or at least one object along <see cref="Path"/> does not support <see cref="INotifyPropertyChanged"/>.</para>
        /// </remarks>
        public void Refresh()
        {
            this.Dismantle();
            this.Construct();
            this.OnItemsPropertyChanged();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection. This enumerator may enumerate null values and/or values of differing types.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            foreach (Subscription subscription in this.subscriptions)
            {
                yield return subscription.SimplePropertyPath.Value;
            }
        }

        #region Semi-deprecated interface methods

        /// <summary>
        /// Copies the elements of the collection to an <see cref="Array"/>, starting at a specified index.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="index">The index in <paramref name="array"/> at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            this.subscriptions.Select(x => x.SimplePropertyPath.Value).ToArray().CopyTo(array, index);
        }

        /// <summary>
        /// Determines whether the collection contains a specified value.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>Whether the value exists in the collection.</returns>
        bool IList.Contains(object value)
        {
            return (this as ICollection).Cast<object>().Contains(value);
        }

        /// <summary>
        /// Returns the index of a specified item.
        /// </summary>
        /// <param name="value">The item to find.</param>
        /// <returns>The index of the item, or -1 if the item was not found.</returns>
        int IList.IndexOf(object value)
        {
            for (int i = 0; i != this.subscriptions.Count; ++i)
            {
                if (value == this.subscriptions[i].SimplePropertyPath.Value)
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion

        #region Modifying interface methods (raise NotSupportedException)

        /// <summary>
        /// Does not add an item to the collection. Always raises <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value">The object that is not added to the collection.</param>
        /// <returns>An undefined value, since this method always raises an exception.</returns>
        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Does not clear all items from the collection. Always raises <see cref="NotSupportedException"/>.
        /// </summary>
        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Does not insert an item into the collection. Always raises <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="index">The index at which the item is not inserted.</param>
        /// <param name="value">The item which is not inserted into the collection.</param>
        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Does not remove an item from the collection. Always raises <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value">The item to not remove from the collection.</param>
        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Does not remove an item from the collection. Always raises <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="index">The index of the item to not remove from the collection.</param>
        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion

        /// <summary>
        /// Retrieves the count of the <see cref="SourceCollection"/>.
        /// </summary>
        /// <returns>The count of items in <see cref="SourceCollection"/>.</returns>
        private int SourceCollectionCount()
        {
            IList list = this.sourceCollection as IList;
            if (list != null)
            {
                return list.Count;
            }

            return this.sourceCollection.Cast<object>().Count();
        }

        /// <summary>
        /// Unsubscribes from all property path subscriptions and the collection subscription.
        /// </summary>
        private void Dismantle()
        {
            foreach (Subscription subscription in this.subscriptions)
            {
                subscription.SimplePropertyPath.Dispose();
            }

            this.subscriptions.Clear();

            if (this.sourceCollectionChanged != null)
            {
                (this.sourceCollection as INotifyCollectionChanged).CollectionChanged -= this.sourceCollectionChanged;
                this.sourceCollectionChanged = null;
            }
        }

        /// <summary>
        /// Subscribes to all property path changes and collection changes.
        /// </summary>
        private void Construct()
        {
            // If no subscriptions are possible yet, just skip it
            if (this.sourceCollection == null || this.path == null)
            {
                return;
            }

            this.subscriptions.Capacity = this.SourceCollectionCount();
            foreach (object obj in this.sourceCollection)
            {
                this.subscriptions.Add(this.CreateLiveSubscription(obj));
            }

            this.sourceCollectionChanged = (sender, args) => this.ProcessSourceCollectionChanged(args);
            (this.sourceCollection as INotifyCollectionChanged).CollectionChanged += this.sourceCollectionChanged;
        }

        /// <summary>
        /// Creates a subscription that is tied into <see cref="ProcessChildPropertyChanged"/>.
        /// </summary>
        /// <param name="root">The object to which to subscribe.</param>
        /// <returns>The live subscription.</returns>
        private Subscription CreateLiveSubscription(object root)
        {
            Subscription ret = new Subscription();
            ret.SimplePropertyPath = new SimplePropertyPath { Root = root, Path = this.path };
            ret.LastKnownValue = ret.SimplePropertyPath.Value;
            ret.SimplePropertyPath.SubscribeToPropertyChanged(x => x.Value, x => this.ProcessChildPropertyChanged(ret));
            return ret;
        }

        /// <summary>
        /// Retrieves a range of subscriptions, copied from <see cref="this.subscriptions"/>.
        /// </summary>
        /// <param name="index">The starting index of the range to copy.</param>
        /// <param name="count">The length of the range to copy.</param>
        /// <returns>The range of subscriptions.</returns>
        private List<Subscription> GetSubscriptionRange(int index, int count)
        {
            List<Subscription> ret = new List<Subscription>(count);
            for (int i = index; i != index + count; ++i)
            {
                ret.Add(this.subscriptions[i]);
            }

            return ret;
        }

        /// <summary>
        /// Retrieves a range of evaluated subscription values.
        /// </summary>
        /// <param name="index">The index of the start of the range.</param>
        /// <param name="count">The length of the range.</param>
        /// <returns>A range of evaluated subscription values.</returns>
        private IList GetRange(int index, int count)
        {
            List<object> ret = new List<object>(count);
            for (int i = index; i != index + count; ++i)
            {
                ret.Add(this.subscriptions[i].SimplePropertyPath.Value);
            }

            return ret;
        }

        /// <summary>
        /// Removes all subscriptions and adds new subscriptions to the objects in <see cref="sourceCollection"/>.
        /// </summary>
        private void ResetSubscriptions()
        {
            foreach (Subscription subscription in this.subscriptions)
            {
                subscription.SimplePropertyPath.Dispose();
            }

            this.subscriptions.Clear();
            this.subscriptions.Capacity = this.SourceCollectionCount();
            foreach (object obj in this.sourceCollection)
            {
                this.subscriptions.Add(this.CreateLiveSubscription(obj));
            }
        }

        /// <summary>
        /// Adds a range of subscriptions to source objects.
        /// </summary>
        /// <param name="index">The index where these subscriptions should be inserted.</param>
        /// <param name="objects">The source objects to subscribe to.</param>
        /// <returns>The range of evaluated subscription values for the new subscription.</returns>
        private IList AddSubscriptions(int index, IList objects)
        {
            this.subscriptions.Capacity = this.subscriptions.Count + objects.Count;
            this.subscriptions.InsertRange(index, objects.Cast<object>().Select(x => this.CreateLiveSubscription(x)));
            return this.GetRange(index, objects.Count);
        }

        /// <summary>
        /// Removes a range of subscriptions to source objects.
        /// </summary>
        /// <param name="index">The index where the range begins.</param>
        /// <param name="count">The length of the range.</param>
        /// <returns>The range of evaluated subscription values for the removed subscriptions.</returns>
        private IList RemoveSubscriptions(int index, int count)
        {
            IList ret = this.GetRange(index, count);
            for (int i = index; i != index + count; ++i)
            {
                this.subscriptions[i].SimplePropertyPath.Dispose();
            }

            this.subscriptions.RemoveRange(index, count);
            return ret;
        }

        /// <summary>
        /// Replaces a range of subscriptions with subscriptions to new source objects.
        /// </summary>
        /// <param name="index">The starting index of the range to be replaced.</param>
        /// <param name="objects">The new source objects to subscribe to.</param>
        /// <returns>The range of evaluated subscription values for the old, replaced subscriptions.</returns>
        private IList ReplaceSubscriptions(int index, IList objects)
        {
            IList ret = this.GetRange(index, objects.Count);
            for (int i = 0; i != objects.Count; ++i)
            {
                this.subscriptions[index + i].SimplePropertyPath.Root = objects[i];
                this.subscriptions[index + i].LastKnownValue = this.subscriptions[index + i].SimplePropertyPath.Value;
            }

            return ret;
        }

        /// <summary>
        /// Moves a range of subscriptions from one index to another.
        /// </summary>
        /// <param name="removeIndex">The starting index of the range to move.</param>
        /// <param name="addIndex">The index to re-insert the range (interpreted as after the range has been removed).</param>
        /// <param name="count">The length of the range to move.</param>
        /// <returns>The range of evaluated subscription values for the moved subscriptions.</returns>
        private IList MoveSubscriptions(int removeIndex, int addIndex, int count)
        {
            List<Subscription> items = this.GetSubscriptionRange(removeIndex, count);
            this.subscriptions.RemoveRange(removeIndex, count);
            this.subscriptions.InsertRange(addIndex, items);
            return this.GetRange(addIndex, count);
        }

        /// <summary>
        /// Handles changes in the source collection.
        /// </summary>
        /// <param name="args">Identifies which objects changed and how.</param>
        private void ProcessSourceCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            // There are situations where NewStartingIndex or OldStartingIndex is unexpectedly -1.
            NotifyCollectionChangedAction action = args.Action;
            if ((action == NotifyCollectionChangedAction.Add && args.NewStartingIndex == -1) ||
                (action == NotifyCollectionChangedAction.Remove && args.OldStartingIndex == -1) ||
                (action == NotifyCollectionChangedAction.Replace && args.OldStartingIndex == -1))
            {
                action = NotifyCollectionChangedAction.Reset;
            }

            // The MSDN documentation for NotifyCollectionChangedEventArgs is incomplete.
            // For the ultimate reference, use Reflector to look at:
            //   [WindowsBase.dll, 3.0.0.0] System.Collections.ObjectModel.ObservableCollection<T> - SetItem, RemoveItem, MoveItem, etc.
            //   [PresentationFramework.dll, 3.0.0.0] System.Windows.Data.CollectionView - ProcessCollectionChanged, AdjustCurrencyFor*
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    // args.NewItems - contains the added items (not the complete list)
                    // args.NewStartingIndex - contains the index where the new items were added
                    IList addNewItems = this.AddSubscriptions(args.NewStartingIndex, args.NewItems);
                    this.OnItemsPropertyChanged();
                    this.OnPropertyChanged(x => x.Count);
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addNewItems, args.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // args.OldItems - contains the removed items (not the complete list)
                    // args.OldStartingIndex - contains the index where the old items were removed
                    IList removeOldItems = this.RemoveSubscriptions(args.OldStartingIndex, args.OldItems.Count);
                    this.OnItemsPropertyChanged();
                    this.OnPropertyChanged(x => x.Count);
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removeOldItems, args.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // args.OldItems - contains the replaced items (not the complete list)
                    // args.NewItems - contains the replacement items (not the complete list)
                    // args.OldStartingIndex - contains the index where the items were replaced
                    IList replaceOldItems = this.ReplaceSubscriptions(args.OldStartingIndex, args.NewItems);
                    IList replaceNewItems = this.GetRange(args.OldStartingIndex, args.NewItems.Count);
                    this.OnItemsPropertyChanged();
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, replaceNewItems, replaceOldItems, args.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Move:
                    // args.NewItems - contains the items that moved (not the complete list)
                    // args.OldStartingIndex - contains the index where the old items were removed
                    // args.NewStartingIndex - contains the index where the new items were added (after the old items were removed)
                    IList moveNewItems = this.MoveSubscriptions(args.OldStartingIndex, args.NewStartingIndex, args.NewItems.Count);
                    this.OnItemsPropertyChanged();
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, moveNewItems, args.NewStartingIndex, args.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Drastic changes; "args" is useless here.
                    this.ResetSubscriptions();
                    this.OnItemsPropertyChanged();
                    this.OnPropertyChanged(x => x.Count);
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        /// <summary>
        /// Handles changes in the property path result.
        /// </summary>
        /// <param name="subscription">The subscription containing the property path that changed.</param>
        private void ProcessChildPropertyChanged(Subscription subscription)
        {
            int index = this.subscriptions.IndexOf(subscription);
            this.OnItemsPropertyChanged();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace,
                new object[] { subscription.SimplePropertyPath.Value },
                new object[] { subscription.LastKnownValue },
                index));
            subscription.LastKnownValue = subscription.SimplePropertyPath.Value;
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event with the supplied arguments.
        /// </summary>
        /// <param name="args">The arguments to pass to the event handlers.</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (this.collectionChanged != null)
            {
                this.collectionChanged(this, args);
            }
        }

        /// <summary>
        /// Holds a property path subscription along with the last known evaluated value.
        /// </summary>
        private sealed class Subscription
        {
            /// <summary>
            /// Gets or sets the simple property path subscription.
            /// </summary>
            public SimplePropertyPath SimplePropertyPath { get; set; }

            /// <summary>
            /// Gets or sets the last known evaluated value.
            /// </summary>
            public object LastKnownValue { get; set; }
        }
    }
}
