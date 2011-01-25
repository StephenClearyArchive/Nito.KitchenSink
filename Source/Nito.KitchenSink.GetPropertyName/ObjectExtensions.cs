// <copyright file="ObjectExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.GetPropertyName
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides extension methods applicable to all objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Retrieves the name of a property referenced by a lambda expression.
        /// </summary>
        /// <typeparam name="TObject">The type of object containing the property.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="this">The object containing the property.</param>
        /// <param name="expression">A lambda expression selecting the property from the containing object. May not be <c>null</c>.</param>
        /// <returns>The name of the property referenced by <paramref name="expression"/>.</returns>
        public static string GetPropertyName<TObject, TProperty>(this TObject @this, Expression<Func<TObject, TProperty>> expression)
        {
            Contract.Requires(expression != null);

            // For more information on the technique used here, see these blog posts:
            //   http://themechanicalbride.blogspot.com/2007/03/symbols-on-steroids-in-c.html
            //   http://michaelsync.net/2009/04/09/silverlightwpf-implementing-propertychanged-with-expression-tree
            //   http://joshsmithonwpf.wordpress.com/2009/07/11/one-way-to-avoid-messy-propertychanged-event-handling/
            // Note that the following blog post:
            //   http://www.ingebrigtsen.info/post/2008/12/11/INotifyPropertyChanged-revisited.aspx
            // uses a similar technique, but must also account for implicit casts to object by checking for UnaryExpression.
            // Our solution uses generics, so this additional test is not necessary.
            return ((MemberExpression)expression.Body).Member.Name;
        }
    }
}

