using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    [CLSCompliant(false)]
    public class InternetConnectHandle : InternetHandle
    {
        public InternetConnectHandle(InternetHandle parent, string serverName, ushort serverPort, string username, string password, Service service, Flags flags)
            : base(NativeMethods.InternetConnect(parent.SafeInternetHandle, serverName, serverPort, username, password, service, flags))
        {
        }

        public enum Service : uint
        {
            Ftp = 1,
            Gopher = 2,
            Http = 3,
        }

        [Flags]
        public enum Flags : uint
        {
            None = 0x0,

            /// <summary>
            /// Use SSL/PCT for HTTP requests.
            /// </summary>
            Secure = 0x00800000,

            /// <summary>
            /// Use an existing InternetConnectHandle if one exists with the same attributes.
            /// </summary>
            ExistingConnect = 0x20000000,

            /// <summary>
            /// Use passive FTP semantics.
            /// </summary>
            Passive = 0x08000000,
        }
    }
}
