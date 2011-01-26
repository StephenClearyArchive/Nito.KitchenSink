// <copyright file="PositionalArgumentsAttribute.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    using System;

    /// <summary>
    /// Specifies that the command-line sets this property to the remaining positional arguments. The property type must be a collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class PositionalArgumentsAttribute : Attribute
    {
    }
}
