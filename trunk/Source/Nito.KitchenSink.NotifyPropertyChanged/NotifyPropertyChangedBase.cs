// <copyright file="NotifyPropertyChangedBase.cs" company="Nito Programs">
//     Copyright (c) 2009-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.NotifyPropertyChanged
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// A base class for classes that need to implement <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    /// <typeparam name="TObject">The type of the derived class.</typeparam>
    public abstract class NotifyPropertyChangedBase<TObject> : INotifyPropertyChanged
        where TObject : NotifyPropertyChangedBase<TObject>
    {
        /// <summary>
        /// The backing delegate for <see cref="PropertyChanged"/>.
        /// </summary>
        private PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// Provides notification of changes to a property value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the Items[] property.
        /// </summary>
        protected void OnItemsPropertyChanged()
        {
            this.propertyChanged.RaiseItems(this);
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the given property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="expression">The lambda expression identifying the property that changed. May not be <c>null</c>.</param>
        protected void OnPropertyChanged<TProperty>(Expression<Func<TObject, TProperty>> expression)
        {
            Contract.Requires(expression != null);

            // The cast of "this" to TObject will always succeed due to the generic constraint on this class
            this.propertyChanged.Raise((TObject)this, expression);
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for all properties.
        /// </summary>
        protected void OnPropertyChanged()
        {
            this.propertyChanged.Raise(this);
        }
    }
}
