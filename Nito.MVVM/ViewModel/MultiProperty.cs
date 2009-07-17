// <copyright file="MultiProperty.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using Nito.Utility;

    /// <summary>
    /// Represents a multiproperty.
    /// </summary>
    /// <remarks>
    /// <para>A multiproperty is defined by a <see cref="SourceCollection"/> and a <see cref="Path"/>. Applying the property path to the source collection results in a collection of individual values.</para>
    /// <para>The <see cref="Value"/> of a multiproperty is equal to its individual values, if all of its individual values are equivalent.</para>
    /// </remarks>
    /// <typeparam name="T">The type of property value.</typeparam>
    public sealed class MultiProperty<T> : NotifyPropertyChangedBase<MultiProperty<T>>, IDisposable
    {
        /// <summary>
        /// The combined value. Acts as a backing store for <see cref="Value"/>.
        /// </summary>
        private T value;

        /// <summary>
        /// The source collection, projected along a property path.
        /// </summary>
        private ProjectedCollection<T> collection;

        /// <summary>
        /// Whether to ignore collection change notifications.
        /// </summary>
        private bool ignoreCollectionChanges;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiProperty{T}"/> class.
        /// </summary>
        public MultiProperty()
        {
            this.collection = new ProjectedCollection<T>();
            this.collection.CollectionChanged += (sender, e) => this.ProcessSourceCollectionChanged(e);
        }

        /// <summary>
        /// Gets or sets the source collection of this multiproperty. This class is more efficient if the source collection implements <see cref="IList"/> and <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        public IEnumerable SourceCollection
        {
            get
            {
                return this.collection.SourceCollection;
            }

            set
            {
                this.collection.SourceCollection = value;
                this.OnPropertyChanged(x => x.SourceCollection);
            }
        }

        /// <summary>
        /// Gets or sets the property path applied to the elements in the source collection.
        /// </summary>
        public string Path
        {
            get
            {
                return this.collection.Path;
            }

            set
            {
                this.collection.Path = value;
                this.OnPropertyChanged(x => x.Path);
            }
        }

        /// <summary>
        /// Gets or sets the multiproperty value.
        /// </summary>
        /// <remarks>
        /// <para>A multiproperty value is calculated from individual values in <see cref="SourceCollection"/>. The property path <see cref="Path"/> is applied to each element in <see cref="SourceCollection"/>, and if the resulting values are all equivalent, then that is the multiproperty value. If one value is not equivalent (or if there is a path evaluation error), then the multiproperty value is the default value of <typeparamref name="T"/>.</para>
        /// <para>Setting the multiproperty value sets all the individual values in <see cref="SourceCollection"/> identified by <see cref="Path"/>.</para>
        /// </remarks>
        public T Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.ignoreCollectionChanges = true;
                try
                {
                    for (int i = 0; i != this.collection.Count; ++i)
                    {
                        this.collection[i] = value;
                    }
                }
                finally
                {
                    this.ignoreCollectionChanges = false;
                }

                this.value = value;
                this.OnPropertyChanged(x => x.Value);
            }
        }

        /// <summary>
        /// Unsubscribes from all collection and property notifications.
        /// </summary>
        public void Dispose()
        {
            this.collection.Dispose();
        }

        /// <summary>
        /// Updates the value of <see cref="Value"/>, raising <see cref="PropertyChanged"/> if necessary.
        /// </summary>
        /// <param name="newValue">The new value of <see cref="Value"/>.</param>
        private void UpdateValue(T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(this.value, newValue))
            {
                this.value = newValue;
                this.OnPropertyChanged(x => x.Value);
            }
        }

        /// <summary>
        /// Re-evaluates the combined <see cref="Value"/> from the individual values in <see cref="collection"/>.
        /// </summary>
        private void RefreshValue()
        {
            if (this.collection.Count == 0)
            {
                this.UpdateValue(default(T));
            }
            else
            {
                T newValue = this.collection[0];
                for (int i = 1; i != this.collection.Count; ++i)
                {
                    if (!EqualityComparer<T>.Default.Equals(newValue, this.collection[i]))
                    {
                        this.UpdateValue(default(T));
                        return;
                    }
                }

                this.UpdateValue(newValue);
            }
        }

        /// <summary>
        /// Process source collection changes: determine if a recalculation of <see cref="Value"/> is necessary, and perform it if it is.
        /// </summary>
        /// <param name="e">What changed and how.</param>
        private void ProcessSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.ignoreCollectionChanges)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // If new elements are added but there is already a conflict, just ignore the change
                    if (!EqualityComparer<T>.Default.Equals(this.value, default(T)))
                    {
                        this.RefreshValue();
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    // We don't care about the position of the elements, so ignore the change
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // If elements are removed but there wasn't already a conflict, just ignore the change
                    if (EqualityComparer<T>.Default.Equals(this.value, default(T)))
                    {
                        this.RefreshValue();
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    // If elements are both added and removed, then we need to recalculate
                    this.RefreshValue();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.RefreshValue();
                    break;
            }
        }
    }
}
