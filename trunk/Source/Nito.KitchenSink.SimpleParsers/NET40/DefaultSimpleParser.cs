using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nito.KitchenSink.Dynamic;
using System.Diagnostics.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace Nito.KitchenSink.SimpleParsers
{
    /// <summary>
    /// Implements <see cref="ISimpleParser"/> for any type that defines a <c>TryParse</c> method with the standard signature.
    /// </summary>
    /// <typeparam name="T">The type of object that this simple parser can parse.</typeparam>
    public sealed class DefaultSimpleParser<T> : ISimpleParser
    {
        /// <summary>
        /// Provides dynamic access to the <typeparamref name="T"/> static type members.
        /// </summary>
        private static readonly dynamic TType = DynamicStaticTypeMembers.Create<T>();

        /// <summary>
        /// A single, shared instance of this parser type.
        /// </summary>
        public static readonly DefaultSimpleParser<T> Instance = new DefaultSimpleParser<T>();

        Type ISimpleParser.ResultType
        {
            get
            {
                Type ret = typeof(T);
                Contract.Assume(ret != typeof(string));
                Contract.Assume(Nullable.GetUnderlyingType(ret) == null);
                return ret;
            }
        }

        [SuppressMessage("Microsoft.Contracts", "Nonnull-174-0")]
        [SuppressMessage("Microsoft.Contracts", "Nonnull-179-0")]
        object ISimpleParser.TryParse(string value)
        {
            var ret = RefOutArg.Create();
            if (TType.TryParse(value, ret))
            {
                return ret.ValueAsObject;
            }

            return null;
        }
    }
}
