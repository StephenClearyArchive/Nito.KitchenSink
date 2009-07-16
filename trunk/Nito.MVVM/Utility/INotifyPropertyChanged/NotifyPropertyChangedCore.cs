// <copyright file="NotifyPropertyChangedCore.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Utility
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    /// <summary>
    /// Implements <see cref="INotifyPropertyChanged"/> on behalf of a container class.
    /// </summary>
    /// <remarks>
    /// <para>Use <see cref="NotifyPropertyChangedBase"/> instead of this class if possible.</para>
    /// </remarks>
    /// <typeparam name="T">The type of the containing class.</typeparam>
    public sealed class NotifyPropertyChangedCore<T>
    {
        /// <summary>
        /// The backing delegate for <see cref="PropertyChanged"/>.
        /// </summary>
        private PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// The object that contains this instance.
        /// </summary>
        private T obj;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedCore{T}"/> class that is contained by <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object that contains this instance.</param>
        public NotifyPropertyChangedCore(T obj)
        {
            this.obj = obj;
        }

        /// <summary>
        /// Provides notification of changes to a property value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                this.propertyChanged += value;
            }

            remove
            {
                this.propertyChanged -= value;
            }
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the given property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="expression">The lambda expression identifying the property that changed.</param>
        public void OnPropertyChanged<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            this.propertyChanged.Raise(this.obj, expression);
        }
    }
}
