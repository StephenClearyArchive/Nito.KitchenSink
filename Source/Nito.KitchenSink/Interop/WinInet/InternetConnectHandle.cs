// <copyright file="InternetConnectHandle.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;

    /// <summary>
    /// An internet connection handle (one that has been created using the <c>InternetConnect</c> function). Normally, a derived class such as <see cref="FtpHandle"/> is used instead of this class. Note that this wrapper does NOT support asynchronous operations! Multiple threads may safely call <see cref="InternetHandle.Dispose"/>.
    /// </summary>
    public class InternetConnectHandle : InternetHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternetConnectHandle"/> class with the specified parameters. Normally, <see cref="InternetOpenHandle.Connect"/> is used instead of this constructor.
        /// </summary>
        /// <param name="parent">The parent opened internet connection.</param>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="service">The service type to which to connect.</param>
        /// <param name="flags">The connection flags.</param>
        public InternetConnectHandle(InternetHandle parent, string serverName, int serverPort, string username, string password, Service service, Flags flags)
            : base(UnsafeNativeMethods.InternetConnect(parent.SafeInternetHandle, serverName, (ushort)(short)serverPort, username, password, service, flags))
        {
        }

        /// <summary>
        /// The types of services that an <see cref="InternetConnectHandle"/> may connect to.
        /// </summary>
        public enum Service : int
        {
            /// <summary>
            /// FTP server.
            /// </summary>
            Ftp = 1,

            /// <summary>
            /// Gopher server.
            /// </summary>
            Gopher = 2,

            /// <summary>
            /// HTTP server.
            /// </summary>
            Http = 3,
        }

        /// <summary>
        /// Flags for the internet connection.
        /// </summary>
        [Flags]
        public enum Flags : int
        {
            /// <summary>
            /// No connection flags.
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Use SSL/PCT for HTTP requests.
            /// </summary>
            Secure = 0x00800000,

            /// <summary>
            /// Use an existing internet connection handle if one exists with the same attributes.
            /// </summary>
            ExistingConnect = 0x20000000,

            /// <summary>
            /// Use passive FTP semantics.
            /// </summary>
            Passive = 0x08000000,
        }
    }
}
