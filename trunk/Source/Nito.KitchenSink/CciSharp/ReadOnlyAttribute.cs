// <copyright file="ReadOnlyAttribute.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;

    /// <summary>
    /// When used with CciSharp, makes an auto-property read-only. This can only be applied to non-virtual instance properties. The setter on the property must be private and only called from the constructor.
    /// See http://ccisamples.codeplex.com/wikipage?title=CciSharp.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ReadOnlyAttribute : Attribute
    {
    }
}
