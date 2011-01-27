// <copyright file="PropertyChangedEventHandlerExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.NotifyPropertyChanged
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using Nito.KitchenSink.GetPropertyName;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides extension methods for <see cref="PropertyChangedEventHandler"/> delegates.
    /// </summary>
    public static class PropertyChangedEventHandlerExtensions
    {
        /// <summary>
        /// Raises the delegate for the property identified by a lambda expression.
        /// </summary>
        /// <typeparam name="TObject">The type of object containing the property.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="handler">The delegate to raise. If this parameter is null, then no action is taken.</param>
        /// <param name="sender">The object raising this event.</param>
        /// <param name="expression">The lambda expression identifying the property that changed. May not be <c>null</c>.</param>
        public static void Raise<TObject, TProperty>(
            this PropertyChangedEventHandler handler,
            TObject sender,
            Expression<Func<TObject, TProperty>> expression)
        {
            Contract.Requires(expression != null);

            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(sender.GetPropertyName(expression)));
            }
        }

        /// <summary>
        /// Raises the delegate for the items property (with the name "Items[]").
        /// </summary>
        /// <param name="handler">The delegate to raise. If this parameter is null, then no action is taken.</param>
        /// <param name="sender">The object raising this event.</param>
        public static void RaiseItems(this PropertyChangedEventHandler handler, object sender)
        {
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs("Items[]"));
            }
        }

        /// <summary>
        /// Raises the delegate for all properties (with an empty string).
        /// </summary>
        /// <param name="handler">The delegate to raise. If this parameter is null, then no action is taken.</param>
        /// <param name="sender">The object raising this event.</param>
        public static void Raise(this PropertyChangedEventHandler handler, object sender)
        {
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(string.Empty));
            }
        }
    }
}
