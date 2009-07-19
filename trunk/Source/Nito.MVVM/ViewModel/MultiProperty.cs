// <copyright file="MultiProperty.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using Nito.Utility;

    /// <summary>
    /// Represents a multiproperty.
    /// </summary>
    /// <remarks>
    /// <para>A multiproperty is defined by a <see cref="SourceCollection"/>, a <see cref="Path"/>, and a multi-value <see cref="Converter"/>. Applying the property path to the source collection results in a collection of individual values, which are converted by the converter into a single "multiproperty value".</para>
    /// <para>If the default <see cref="IdentityMultiValueConverter"/> is used, then the <see cref="Value"/> of a multiproperty is equal to one of its individual values, if all of its individual values are equivalent.</para>
    /// </remarks>
    public sealed class MultiProperty : NotifyPropertyChangedBase<MultiProperty>, IDisposable
    {
        /// <summary>
        /// The combined value. Acts as a backing store for <see cref="Value"/>.
        /// </summary>
        private object value;

        /// <summary>
        /// The source collection, projected along a property path.
        /// </summary>
        private ProjectedCollection collection;

        /// <summary>
        /// Whether to ignore collection change notifications.
        /// </summary>
        private bool ignoreCollectionChanges;

        /// <summary>
        /// Backing field for <see cref="Converter"/>.
        /// </summary>
        private IMultiValueConverter converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiProperty"/> class.
        /// </summary>
        public MultiProperty()
        {
            this.collection = new ProjectedCollection();
            this.collection.CollectionChanged += (sender, e) => this.ProcessSourceCollectionChanged(e);
            this.converter = new IdentityMultiValueConverter();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiProperty"/> class using the specified converter.
        /// </summary>
        /// <param name="converter">The multi-value converter to use.</param>
        public MultiProperty(IMultiValueConverter converter)
        {
            this.collection = new ProjectedCollection();
            this.collection.CollectionChanged += (sender, e) => this.ProcessSourceCollectionChanged(e);
            this.converter = converter;
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
        /// <para>A multiproperty value is calculated from individual values in <see cref="SourceCollection"/>. The property path <see cref="Path"/> is applied to each element in <see cref="SourceCollection"/>, and the resulting values are run through <see cref="Converter"/> to get the multiproperty value. When using the default converter, if one value is not equivalent (or if there is a path evaluation error), then the multiproperty value is null.</para>
        /// <para>Setting the multiproperty value will invoke <see cref="IMultiValueConverter.ConvertBack"/> on <see cref="Converter"/>, and use the resulting group of values to set the individual values in <see cref="SourceCollection"/> identified by <see cref="Path"/>. If using the default (identity) converter, this sets all the individual values to the new value.</para>
        /// </remarks>
        public object Value
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
                    if (this.Converter is IdentityMultiValueConverter)
                    {
                        // Optimize the common case
                        for (int i = 0; i != this.collection.Count; ++i)
                        {
                            this.collection[i] = value;
                        }
                    }
                    else
                    {
                        // Longer, more correct code for custom converters
                        Type[] types = new Type[this.collection.Count];
                        for (int i = 0; i != types.Length; ++i)
                        {
                            types[i] = typeof(object);
                        }

                        object[] values = this.Converter.ConvertBack(value, types, null, CultureInfo.CurrentCulture);

                        for (int i = 0; i != this.collection.Count; ++i)
                        {
                            this.collection[i] = values[i];
                        }
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
        /// Gets or sets the multi-value converter used when setting or getting <see cref="Value"/>. This property may not be set to null.
        /// </summary>
        public IMultiValueConverter Converter
        {
            get
            {
                return this.converter;
            }

            set
            {
                this.converter = value;
                this.RefreshValue();
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
        private void UpdateValue(object newValue)
        {
            if (!object.Equals(this.value, newValue))
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
            this.UpdateValue(this.Converter.Convert(this.collection.Cast<object>().ToArray(), typeof(object), null, CultureInfo.CurrentCulture));
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
                    if (this.value != null)
                    {
                        this.RefreshValue();
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    // We don't care about the position of the elements, so ignore the change
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // If elements are removed but there wasn't already a conflict, just ignore the change
                    if (this.value == null)
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
