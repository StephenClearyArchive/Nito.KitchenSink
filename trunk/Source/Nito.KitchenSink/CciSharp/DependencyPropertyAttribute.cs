using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    /// <summary>
    /// When used with CciSharp, turns an auto-property into a property that supports DependencyProperty. This can only be applied to non-virtual instance properties.
    /// See http://ccisamples.codeplex.com/wikipage?title=CciSharp.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class DependencyPropertyAttribute : Attribute
    {
    }
}
