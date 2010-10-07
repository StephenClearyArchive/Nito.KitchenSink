using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Options
{
    /// <summary>
    /// Specifies that the command-line sets this property to the remaining positional arguments. The property type must be a collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class PositionalArgumentsAttribute : Attribute
    {
    }
}
