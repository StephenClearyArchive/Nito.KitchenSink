using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    [CLSCompliant(false)]
    public sealed class InternetOpenHandle : InternetHandle
    {
        public InternetOpenHandle(string agent, AccessType accessType, string proxyName, string proxyBypass, Flags flags)
            : base(NativeMethods.InternetOpen(agent, accessType, proxyName, proxyBypass, flags))
        {
        }

        public InternetOpenHandle(string agent, Flags flags)
            : this(agent, AccessType.Direct, null, null, flags)
        {
        }

        public InternetOpenHandle(string agent)
            : this(agent, AccessType.Direct, null, null, Flags.None)
        {
        }

        public InternetConnectHandle Connect(string serverName, ushort serverPort, string username, string password, InternetConnectHandle.Service service, InternetConnectHandle.Flags flags)
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

        public FtpHandle ConnectFtp(string serverName, ushort serverPort, string username, string password, InternetConnectHandle.Flags flags)
        {
            return new FtpHandle(this, serverName, serverPort, username, password, flags);
        }

        public FtpHandle ConnectFtp(string serverName, string username, string password, InternetConnectHandle.Flags flags)
        {
            return new FtpHandle(this, serverName, 0, username, password, flags);
        }

        public FtpHandle ConnectFtp(string serverName, string username, string password)
        {
            return new FtpHandle(this, serverName, 0, username, password, InternetConnectHandle.Flags.None);
        }

        public enum AccessType : uint
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

        [Flags]
        public enum Flags : uint
        {
            /// <summary>
            /// No flags specified.
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Forces asynchronous operations.
            /// </summary>
            Async = 0x1,

            /// <summary>
            /// Does not make network requests; all entries are returned from the cache.
            /// </summary>
            Offline = 0x01000000,

            /// <summary>
            /// Does not make network requests; all entries are returned from the cache.
            /// </summary>
            FromCache = Offline,
        }
    }
}
