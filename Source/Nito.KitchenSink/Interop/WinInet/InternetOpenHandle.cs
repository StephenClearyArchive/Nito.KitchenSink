// <copyright file="InternetOpenHandle.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;

    /// <summary>
    /// Represents a base internet handle that has been opened by calling the <c>InternetOpen</c> function. Note that this wrapper does NOT support asynchronous operations! Multiple threads may safely call <see cref="Dispose"/>.
    /// </summary>
    public sealed class InternetOpenHandle : InternetHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternetOpenHandle"/> class with the specified parameters.
        /// </summary>
        /// <param name="agent">The user agent or process using WinInet. This is sent as the HTTP user agent and WinInet logs.</param>
        /// <param name="accessType">The type of the proxy used, if any.</param>
        /// <param name="proxyName">The name of the proxy server if <paramref name="accessType"/> is <see cref="AccessType.Proxy"/>.</param>
        /// <param name="proxyBypass">The list of host names or IP addresses that are not routed through the proxy when <paramref name="accessType"/> is <see cref="AccessType.Proxy"/>. This list may contain wildcards or be equal to the string <c>"&lt;local&gt;"</c>, but should not be an empty string.</param>
        /// <param name="flags">The flags to use for this internet handle.</param>
        public InternetOpenHandle(string agent, AccessType accessType, string proxyName, string proxyBypass, Flags flags)
            : base(UnsafeNativeMethods.InternetOpen(agent, accessType, proxyName, proxyBypass, flags))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetOpenHandle"/> class with direct (no proxy) access.
        /// </summary>
        /// <param name="agent">The user agent or process using WinInet. This is sent as the HTTP user agent and WinInet logs.</param>
        /// <param name="flags">The flags to use for this internet handle.</param>
        public InternetOpenHandle(string agent, Flags flags)
            : this(agent, AccessType.Direct, null, null, flags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetOpenHandle"/> class with direct (no proxy) access and no special flags.
        /// </summary>
        /// <param name="agent">The user agent or process using WinInet. This is sent as the HTTP user agent and WinInet logs.</param>
        public InternetOpenHandle(string agent)
            : this(agent, AccessType.Direct, null, null, Flags.None)
        {
        }

        /// <summary>
        /// The type of proxy used by this internet handle to access resources.
        /// </summary>
        public enum AccessType : int
        {
            /// <summary>
            /// Resolves all host names locally.
            /// </summary>
            Direct = 1,

            /// <summary>
            /// Retrieves the proxy or direct configuration from the registry.
            /// </summary>
            Preconfig = 0,

            /// <summary>
            /// Retrieves the proxy or direct configuration from the registry, but prevents the use of a startup script or setup file.
            /// </summary>
            PreconfigWithNoAutoproxy = 4,

            /// <summary>
            /// Passes requests to the proxy unless the request matches the proxy bypass list.
            /// </summary>
            Proxy = 3,
        }

        /// <summary>
        /// The flags to use for this internet handle.
        /// </summary>
        [Flags]
        public enum Flags : int
        {
            /// <summary>
            /// No flags specified.
            /// </summary>
            None = 0x0,

            //// <summary>
            //// Forces asynchronous operations.
            //// </summary>
            ////Async = 0x1,

            /// <summary>
            /// Does not make network requests; all entries are returned from the cache.
            /// </summary>
            Offline = 0x01000000,

            /// <summary>
            /// Does not make network requests; all entries are returned from the cache.
            /// </summary>
            FromCache = Offline,
        }

        /// <summary>
        /// Establishes a connection to the specified server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="service">The service type to which to connect.</param>
        /// <param name="flags">The connection flags.</param>
        /// <returns>An established internet connection to the specified server.</returns>
        public InternetConnectHandle Connect(string serverName, int serverPort, string username, string password, InternetConnectHandle.Service service, InternetConnectHandle.Flags flags)
        {
            switch (service)
            {
                case InternetConnectHandle.Service.Ftp:
                    return new FtpHandle(this, serverName, serverPort, username, password, flags);
                case InternetConnectHandle.Service.Gopher:
                case InternetConnectHandle.Service.Http:
                default:
                    return new InternetConnectHandle(this, serverName, serverPort, username, password, service, flags);
            }
        }

        /// <summary>
        /// Establishes an FTP connection to the specified server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="flags">The connection flags.</param>
        /// <returns>An established FTP connection to the specified server.</returns>
        public FtpHandle ConnectFtp(string serverName, int serverPort, string username, string password, InternetConnectHandle.Flags flags)
        {
            return new FtpHandle(this, serverName, serverPort, username, password, flags);
        }

        /// <summary>
        /// Establishes an FTP connection to the specified server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="flags">The connection flags.</param>
        /// <returns>An established FTP connection to the specified server.</returns>
        public FtpHandle ConnectFtp(string serverName, string username, string password, InternetConnectHandle.Flags flags)
        {
            return new FtpHandle(this, serverName, 0, username, password, flags);
        }

        /// <summary>
        /// Establishes an FTP connection to the specified server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <returns>An established FTP connection to the specified server.</returns>
        public FtpHandle ConnectFtp(string serverName, string username, string password)
        {
            return new FtpHandle(this, serverName, 0, username, password, InternetConnectHandle.Flags.None);
        }

        /// <summary>
        /// Establishes an FTP connection to the specified server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="flags">The connection flags.</param>
        /// <returns>An established FTP connection to the specified server.</returns>
        public FtpHandle ConnectFtp(string serverName, InternetConnectHandle.Flags flags)
        {
            return new FtpHandle(this, serverName, 0, null, null, flags);
        }

        /// <summary>
        /// Establishes an FTP connection to the specified server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <returns>An established FTP connection to the specified server.</returns>
        public FtpHandle ConnectFtp(string serverName)
        {
            return new FtpHandle(this, serverName, 0, null, null, InternetConnectHandle.Flags.None);
        }
    }
}
