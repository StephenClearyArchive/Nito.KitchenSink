using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    /// <summary>
    /// When used with CciSharp, makes an auto-property read-only. This can only be applied to non-virtual instance properties. The setter on the property must be private and only called from the constructor.
    /// See http://ccisamples.codeplex.com/wikipage?title=CciSharp.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ReadOnlyAttribute : Attribute
    {
    }
}
