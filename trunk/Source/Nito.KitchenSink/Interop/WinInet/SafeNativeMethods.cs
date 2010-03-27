// <copyright file="SafeNativeMethods.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Native methods that are safe for any caller.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static partial class SafeNativeMethods
    {
        /// <summary>
        /// Converts a local FILETIME to a Utc FILETIME.
        /// </summary>
        /// <param name="localFileTime">The local FILETIME to convert.</param>
        /// <returns>Utc FILETIME.</returns>
        public static long FtpLocalFileTimeToFileTime(long localFileTime)
        {
            long ret;
            if (!FtpLocalFileTimeToFileTime(ref localFileTime, out ret))
            {
                throw Interop.GetLastWin32Exception();
            }

            return ret;
        }

        [DllImport("Kernel32.dll", EntryPoint = "LocalFileTimeToFileTime", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FtpLocalFileTimeToFileTime(ref long lpLocalFileTime, out long lpFileTime);
    }
}
