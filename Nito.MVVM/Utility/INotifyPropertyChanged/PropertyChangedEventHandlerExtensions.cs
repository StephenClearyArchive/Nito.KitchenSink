// <copyright file="PropertyChangedEventHandlerExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Utility
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

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
        /// <param name="expression">The lambda expression identifying the property that changed.</param>
        public static void Raise<TObject, TProperty>(
            this PropertyChangedEventHandler handler,
            TObject sender,
            Expression<Func<TObject, TProperty>> expression)
        {
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(sender.GetPropertyName(expression)));
            }
        }
    }
}
