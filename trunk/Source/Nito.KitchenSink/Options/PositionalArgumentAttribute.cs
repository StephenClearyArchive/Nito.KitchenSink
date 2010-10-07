using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Options
{
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
            this.Index = index;
        }

        /// <summary>
        /// Gets or sets the index of the positional argument.
        /// </summary>
        public int Index { get; set; }
    }
}
