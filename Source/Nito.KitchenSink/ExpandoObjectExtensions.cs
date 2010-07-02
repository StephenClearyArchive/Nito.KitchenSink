using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Extension methods for the <see cref="ExpandoObject"/> class.
    /// </summary>
    public static class ExpandoObjectExtensions
    {
        /// <summary>
        /// Adds a sequence of values as properties on the <see cref="ExpandoObject"/>. Any existing properties with the same name are overwritten. Returns the same <see cref="ExpandoObject"/> for chaining.
        /// </summary>
        /// <typeparam name="T">The type of values to add.</typeparam>
        /// <param name="this">The object to which to add the properties.</param>
        /// <param name="values">The values to add as properties.</param>
        /// <param name="names">The names to use for the properties. This may be <c>null</c>. If this parameter is <c>null</c> or does not contain enough names for the values, the property name will be of the form "Property<i>n</i>", where <i>n</i> is the index in the value sequence.</param>
        /// <returns>The <see cref="ExpandoObject"/> <paramref name="this"/>.</returns>
        public static ExpandoObject AddProperties<T>(this ExpandoObject @this, IEnumerable<T> values, IEnumerable<string> names = null)
        {
            IDictionary<string, object> obj = @this;
            int index = 0;
            using (var valueEnumerator = values.CreateEnumeratorWrapper())
            using (var nameEnumerator = names == null ? null : names.CreateEnumeratorWrapper())
            {
                while (!valueEnumerator.Done)
                {
                    // Determine the name of the field
                    string name;
                    if (nameEnumerator != null && !nameEnumerator.Done)
                    {
                        name = nameEnumerator.Current;
                        nameEnumerator.MoveNext();
                    }
                    else
                    {
                        name = "Property" + index;
                    }

                    // Save the value of the field
                    if (obj.ContainsKey(name))
                    {
                        obj[name] = valueEnumerator.Current;
                    }
                    else
                    {
                        obj.Add(name, valueEnumerator.Current);
                    }

                    valueEnumerator.MoveNext();
                    ++index;
                }
            }

            return @this;
        }
    }
}
