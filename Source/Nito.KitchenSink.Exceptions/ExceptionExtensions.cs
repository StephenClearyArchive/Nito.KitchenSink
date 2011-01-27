// <copyright file="ExceptionExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.Exceptions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    /// <summary>
    /// Extension methods for exceptions.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Retrieves a nearly-complete dump of all the exception detail into XML.
        /// </summary>
        /// <param name="exception">The exception to dump. May not be <c>null</c>.</param>
        /// <returns>The exception information, as XML.</returns>
        public static XElement ToXml(this Exception exception)
        {
            Contract.Requires(exception != null);
            Contract.Ensures(Contract.Result<XElement>() != null);

            //<Exception>
            //    <ExceptionType>The assembly-qualified type of the exception.</ExceptionType>
            //    <Message>The exception message.</Message>
            //    <StackTrace> (optional) The exception stack trace, if present.</StackTrace>
            //    <NativeErrorCode> (optional) The Win32 error in hex.</NativeErrorCode>
            //    <DataItems> (optional)
            //        <DataItem> (1..N)
            //            <Key>The key of the Data item.</Key>
            //            <Value>The value of the Data item.</Value>
            //        </DataItem>
            //    </DataItems>
            //    <InnerExceptions> (optional)
            //        <Exception> (1..N) Same as the top-level Exception element.</Exception>
            //    </InnerExceptions>
            //</Exception>

            var win32Exception = exception as Win32Exception;
            var aggregateException = exception as AggregateException;
            Contract.Assume(aggregateException == null || aggregateException.InnerExceptions != null);
            return new XElement("Exception",
                new XElement("ExceptionType", exception.GetType().AssemblyQualifiedName),
                new XElement("Message", exception.Message),
                (exception.StackTrace == null) ? null : new XElement("StackTrace", exception.StackTrace),
                (win32Exception == null) ? null : new XElement("NativeErrorCode", win32Exception.NativeErrorCode.ToString("X", CultureInfo.InvariantCulture)),
                (exception.Data == null || exception.Data.Count == 0) ? null : new XElement(
                    "DataItems",
                    exception.Data.Cast<DictionaryEntry>().Select(x => new XElement("DataItem", new XElement("Key", x.Key.ToString()), new XElement("Value", x.Value.ToString())))),
                (aggregateException == null) ?
                    ((exception.InnerException == null) ? null : new XElement("InnerExceptions", exception.InnerException.ToXml()))
                    : new XElement("InnerExceptions", aggregateException.InnerExceptions.Select(x => x.ToXml())));
        }

        /// <summary>
        /// Retrieves a somewhat user-friendly error message of the form "[TypeOfException] ExceptionMessage", unwrapping any aggregate exceptions before creating the error message.
        /// </summary>
        /// <param name="exception">The exception for which to retrieve the error message. May not be <c>null</c>.</param>
        /// <returns>The somewhat user-friendly error message.</returns>
        public static string SomewhatUserFriendlyMessage(this Exception exception)
        {
            Contract.Requires(exception != null);
            Contract.Ensures(Contract.Result<string>() != null);

            exception = exception.UnwrapAggregateExceptions();
            return "[" + exception.GetType() + "] " + exception.Message;
        }

        /// <summary>
        /// Unwraps any aggregate exceptions to the first non-aggregate exception (depth-first). The returned exception is not an aggregate exception, but it may have an inner exception.
        /// </summary>
        /// <param name="exception">The exception to unwrap. May not be <c>null</c>.</param>
        /// <returns>The unwrapped exception. The returned exception is not an aggregate exception, but it may have an inner exception.</returns>
        public static Exception UnwrapAggregateExceptions(this Exception exception)
        {
            Contract.Requires(exception != null);
            Contract.Ensures(Contract.Result<Exception>() != null);

            var ret = exception.InnerExceptionsAndSelf().Where(x => !(x is AggregateException)).First();
            Contract.Assume(ret != null);
            return ret;
        }

        /// <summary>
        /// Returns a sequence of exceptions that contains this exception and all inner exceptions, depth-first.
        /// </summary>
        /// <param name="exception">The exception to enumerate. May not be <c>null</c>.</param>
        /// <returns>A sequence of inner exceptions, beginning at the current exception.</returns>
        public static IEnumerable<Exception> InnerExceptionsAndSelf(this Exception exception)
        {
            Contract.Requires(exception != null);
            Contract.Ensures(Contract.Result<IEnumerable<Exception>>() != null);

            do
            {
                yield return exception;

                var aggregateException = exception as AggregateException;
                if (aggregateException != null)
                {
                    foreach (var innerException in aggregateException.InnerExceptions.SelectMany(x => x.InnerExceptionsAndSelf()))
                    {
                        yield return innerException;
                    }

                    yield break;
                }

                exception = exception.InnerException;
            } while (exception != null);
        }

        private static readonly MethodInfo prepForRemoting = typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Re-throws the exception, preserving the original stack trace. This method does not return normally, since it throws the exception.
        /// </summary>
        /// <param name="exception">The exception to re-throw. May not be <c>null</c>.</param>
        public static void ReThrow(this Exception exception)
        {
            Contract.Requires(exception != null);

            prepForRemoting.Invoke(exception, new object[0]);
            throw exception;
        }
    }
}
