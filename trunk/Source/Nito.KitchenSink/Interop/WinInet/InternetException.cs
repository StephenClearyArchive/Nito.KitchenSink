// <copyright file="InternetException.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;

    /// <summary>
    /// An error as reported by a remote server.
    /// </summary>
    public sealed class InternetException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternetException"/> class with the specified attributes.
        /// </summary>
        /// <param name="code">The error code, as reported by the remote server.</param>
        /// <param name="message">The message, as reported by the remote server.</param>
        public InternetException(int code, string message)
            : base("Internet error code " + code + ": " + message)
        {
            this.Code = code;
        }

        /// <summary>
        /// Gets the error code, as reported by the remote server.
        /// </summary>
        public int Code { get; private set; }
    }
}
