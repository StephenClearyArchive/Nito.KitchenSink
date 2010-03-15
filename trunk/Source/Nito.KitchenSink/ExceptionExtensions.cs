// <copyright file="ExceptionExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Nito.Linq;

    /// <summary>
    /// Provides useful extension methods for the <see cref="Exception"/> class.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Returns a flattened, printable detailed error message for this exception and all inner exceptions.
        /// </summary>
        /// <param name="ex">The exception from which to get the error details.</param>
        /// <param name="includeStackTrace">Whether to include the stack trace in the error message.</param>
        /// <returns>A flattened, printable detailed error message.</returns>
        public static string Dump(this Exception ex, bool includeStackTrace)
        {
            return ex.InnerExceptionsAndSelf().Select(e => e.ErrorMessage(includeStackTrace)).Join("  :  ");
        }
        
        /// <summary>
        /// Returns a flattened, printable detailed error message for this exception.
        /// </summary>
        /// <param name="ex">The exception from which to get the error details.</param>
        /// <param name="includeStackTrace">Whether to include the stack trace in the error message.</param>
        /// <returns>A flattened, printable detailed error message.</returns>
        public static string ErrorMessage(this Exception ex, bool includeStackTrace)
        {
            string ret = "[" + ex.GetType().Name + "] " + ex.Message.Flatten().Printable();

            string data = null;
            if (ex.Data != null && ex.Data.Count != 0)
            {
                var dataStrings = ex.Data.Cast<DictionaryEntry>().Select(x => "\"" + x.Key.ToString().PrintableEscape() + "\" = \"" + x.Value.ToString().PrintableEscape() + "\"");
                data = "{ " + dataStrings.Join(", ") + " }";
            }

            string stackTrace = null;
            if (includeStackTrace && !string.IsNullOrEmpty(ex.StackTrace))
            {
                stackTrace = ex.StackTrace.Flatten().Printable();
            }

            if (data != null)
            {
                ret += "  " + data;
            }

            if (stackTrace != null)
            {
                ret += "  " + stackTrace;
            }

            return ret;
        }

        /// <summary>
        /// Returns a collection of exceptions that contains this exception and all inner exceptions.
        /// </summary>
        /// <param name="ex">The exception to enumerate.</param>
        /// <returns>A sequence of inner exceptions, beginning at the current exception.</returns>
        public static IEnumerable<Exception> InnerExceptionsAndSelf(this Exception ex)
        {
            AggregateException aggregateEx = ex as AggregateException;
            if (aggregateEx != null)
            {
                yield return ex;
                foreach (var innerEx in aggregateEx.InnerExceptions.Select(x => x.InnerExceptionsAndSelf()).Flatten())
                {
                    yield return innerEx;
                }
            }
            else
            {
                Exception e = ex;
                while (e != null)
                {
                    yield return e;
                    e = e.InnerException;
                }
            }
        }
    }
}
