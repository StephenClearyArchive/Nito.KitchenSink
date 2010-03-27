// <copyright file="DependencyPropertyAttribute.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;

    /// <summary>
    /// When used with CciSharp, turns an auto-property into a property that supports DependencyProperty. This can only be applied to non-virtual instance properties.
    /// See http://ccisamples.codeplex.com/wikipage?title=CciSharp.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class DependencyPropertyAttribute : Attribute
    {
    }
}
