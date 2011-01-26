// <copyright file="PositionalArgumentAttribute.cs" company="Nito Programs">
//     Copyright (c) 2010-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.OptionParsing
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Specifies that a command-line positional argument sets this property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class PositionalArgumentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PositionalArgumentAttribute"/> class.
        /// </summary>
        /// <param name="index">The index of the positional argument.</param>
        public PositionalArgumentAttribute(int index)
        {
            Contract.Requires(index >= 0);

            this.Index = index;
        }

        /// <summary>
        /// Gets or sets the index of the positional argument.
        /// </summary>
        public int Index { get; set; }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.Index >= 0);
        }
    }
}
