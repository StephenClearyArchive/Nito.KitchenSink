// <copyright file="UnsafeNativeMethods.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Nito.Linq;

    /// <summary>
    /// Native methods that require a security check for use.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static partial class UnsafeNativeMethods
    {
        /// <summary>
        /// Maximum path length.
        /// </summary>
        private const int MAX_PATH = 260;

        /// <summary>
        /// Win32 error code for an insufficient buffer.
        /// </summary>
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        /// <summary>
        /// WinInet error code indicating that the server has sent its own error message.
        /// </summary>
        private const int ERROR_INTERNET_EXTENDED_ERROR = 12003;

        /// <summary>
        /// Win32 error code indicating that there are no more files.
        /// </summary>
        private const int ERROR_NO_MORE_FILES = 18;

        /// <summary>
        /// An invalid internet status callback.
        /// </summary>
        private static readonly IntPtr INTERNET_INVALID_STATUS_CALLBACK = (IntPtr)(-1);

        /// <summary>
        /// The delegate type of the internet status callback wrapper, passed to <c>InternetSetStatusCallback</c>.
        /// </summary>
        /// <param name="hInternet">The internet handle. This parameter is ignored.</param>
        /// <param name="dwContext">The context passed to the internet operation. This parameter is ignored.</param>
        /// <param name="dwInternetStatus">The type of notification.</param>
        /// <param name="lpvStatusInformation">Extra data associated with the notification.</param>
        /// <param name="dwStatusInformationLength">The length of the extra data in <paramref name="lpvStatusInformation"/>.</param>
        private delegate void InternetStatusCallback(IntPtr hInternet, IntPtr dwContext, InternetCallbackEventArgs.StatusCode dwInternetStatus, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] lpvStatusInformation, uint dwStatusInformationLength);

        [DllImport("Wininet.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InternetCloseHandle(IntPtr hInternet);

        /// <summary>
        /// Invokes <c>InternetOpen</c>, handling error conditions.
        /// </summary>
        /// <param name="agent">The user agent or process using WinInet. This is sent as the HTTP user agent and WinInet logs.</param>
        /// <param name="accessType">The type of the proxy used, if any.</param>
        /// <param name="proxyName">The name of the proxy server if <paramref name="accessType"/> is <see cref="InternetOpenHandle.AccessType.Proxy"/>.</param>
        /// <param name="proxyBypass">The list of host names or IP addresses that are not routed through the proxy when <paramref name="accessType"/> is <see cref="InternetOpenHandle.AccessType.Proxy"/>. This list may contain wildcards or be equal to the string <c>"&lt;local&gt;"</c>, but should not be an empty string.</param>
        /// <param name="flags">The flags to use for this internet handle.</param>
        /// <returns>The opened internet handle.</returns>
        public static SafeInternetHandle InternetOpen(string agent, InternetOpenHandle.AccessType accessType, string proxyName, string proxyBypass, InternetOpenHandle.Flags flags)
        {
            SafeInternetHandle ret = DoInternetOpen(agent, accessType, proxyName, proxyBypass, flags);
            if (ret.IsInvalid)
            {
                throw GetLastInternetException();
            }

            return ret;
        }

        /// <summary>
        /// Invokes <c>InternetConnect</c>, handling error conditions.
        /// </summary>
        /// <param name="internet">The parent opened internet handle.</param>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="service">The service type to which to connect.</param>
        /// <param name="flags">The connection flags.</param>
        /// <returns>The connected internet handle.</returns>
        public static SafeInternetHandle InternetConnect(SafeInternetHandle internet, string serverName, ushort serverPort, string username, string password, InternetConnectHandle.Service service, InternetConnectHandle.Flags flags)
        {
            SafeInternetHandle ret = DoInternetConnect(internet, serverName, serverPort, username, password, service, flags, (IntPtr)1);
            if (ret.IsInvalid)
            {
                throw GetLastInternetException();
            }

            return ret;
        }

        /// <summary>
        /// Invokes <c>FtpCreateDirectory</c>, handling error conditions.
        /// </summary>
        /// <param name="connect">The connected internet handle.</param>
        /// <param name="directory">The directory to create.</param>
        public static void FtpCreateDirectory(SafeInternetHandle connect, string directory)
        {
            if (!DoFtpCreateDirectory(connect, directory))
            {
                throw GetLastInternetException();
            }
        }

        /// <summary>
        /// Invokes <c>FtpGetCurrentDirectory</c>, handling error conditions.
        /// </summary>
        /// <param name="connect">The connected internet handle.</param>
        /// <returns>The current working directory on the remote FTP server.</returns>
        public static string FtpGetCurrentDirectory(SafeInternetHandle connect)
        {
            StringBuilder ret = new StringBuilder(MAX_PATH);
            uint length = (uint)ret.Capacity + 1;
            if (!DoFtpGetCurrentDirectory(connect, ret, ref length))
            {
                throw GetLastInternetException();
            }

            return ret.ToString();
        }

        /// <summary>
        /// Invokes <c>FtpDeleteFile</c>, handling error conditions.
        /// </summary>
        /// <param name="connect">The connected internet handle.</param>
        /// <param name="fileName">The file to delete.</param>
        public static void FtpDeleteFile(SafeInternetHandle connect, string fileName)
        {
            if (!DoFtpDeleteFile(connect, fileName))
            {
                throw GetLastInternetException();
            }
        }

        /// <summary>
        /// Invokes <c>FtpGetFile</c>, handling error conditions.
        /// </summary>
        /// <param name="connect">The connected internet handle.</param>
        /// <param name="remoteFile">The remote file to download.</param>
        /// <param name="localFile">The local path and filename to which to save the file.</param>
        /// <param name="failIfExists">Whether to fail if the local file specified by <paramref name="localFile"/> already exists.</param>
        /// <param name="flags">Additional flags for this action. At least <see cref="FtpHandle.GetFileFlags.Ascii"/> or <see cref="FtpHandle.GetFileFlags.Binary"/> should be specified.</param>
        public static void FtpGetFile(SafeInternetHandle connect, string remoteFile, string localFile, bool failIfExists, FtpHandle.GetFileFlags flags)
        {
            if (!DoFtpGetFile(connect, remoteFile, localFile, failIfExists, 0, flags, (IntPtr)1))
            {
                throw GetLastInternetException();
            }
        }

        /// <summary>
        /// Invokes <c>FtpPutFile</c>, handling error conditions.
        /// </summary>
        /// <param name="connect">The connected internet handle.</param>
        /// <param name="localFile">The local file to upload.</param>
        /// <param name="remoteFile">The remote path and filename to which to save the file.</param>
        /// <param name="flags">Additional flags for this action. At least <see cref="FtpHandle.PutFileFlags.Ascii"/> or <see cref="FtpHandle.PutFileFlags.Binary"/> should be specified.</param>
        public static void FtpPutFile(SafeInternetHandle connect, string localFile, string remoteFile, FtpHandle.PutFileFlags flags)
        {
            if (!DoFtpPutFile(connect, localFile, remoteFile, flags, (IntPtr)1))
            {
                throw GetLastInternetException();
            }
        }

        /// <summary>
        /// Invokes <c>FtpRemoveDirectory</c>, handling error conditions.
        /// </summary>
        /// <param name="connect">The connected internet handle.</param>
        /// <param name="directory">The directory to remove.</param>
        public static void FtpRemoveDirectory(SafeInternetHandle connect, string directory)
        {
            if (!DoFtpRemoveDirectory(connect, directory))
            {
                throw GetLastInternetException();
            }
        }

        /// <summary>
        /// Invokes <c>FtpRenameFile</c>, handling error conditions.
        /// </summary>
        /// <param name="connect">The connected internet handle.</param>
        /// <param name="oldName">The old file name.</param>
        /// <param name="newName">The new file name.</param>
        public static void FtpRenameFile(SafeInternetHandle connect, string oldName, string newName)
        {
            if (!DoFtpRenameFile(connect, oldName, newName))
            {
                throw GetLastInternetException();
            }
        }

        /// <summary>
        /// Invokes <c>FtpRenameFile</c>, handling error conditions.
        /// </summary>
        /// <param name="connect">The connected internet handle.</param>
        /// <param name="directory">The new current working directory.</param>
        public static void FtpSetCurrentDirectory(SafeInternetHandle connect, string directory)
        {
            if (!DoFtpSetCurrentDirectory(connect, directory))
            {
                throw GetLastInternetException();
            }
        }

        /// <summary>
        /// Invokes <c>InternetSetStatusCallback</c>, handling error conditions. Returns the wrapper for the delegate, which is actually passed to <c>InternetSetStatusCallback</c>.
        /// </summary>
        /// <param name="internet">The internet handle.</param>
        /// <param name="callback">The internet status callback delegate, used to report progress.</param>
        /// <returns>The wrapper created for <paramref name="callback"/>, which is actually passed to the unmanaged <c>InternetSetStatusCallback</c> function.</returns>
        public static object InternetSetStatusCallback(SafeInternetHandle internet, InternetHandle.InternetCallback callback)
        {
            // Note: this will only work for synchronous callbacks! Asynchronous callbacks are not yet supported.
            InternetStatusCallback ret = CreateInternetStatusCallback(callback);
            if (DoInternetSetStatusCallback(internet, ret) == INTERNET_INVALID_STATUS_CALLBACK)
            {
                throw GetLastInternetException();
            }

            return ret;
        }

        /// <summary>
        /// Invokes <c>FtpFindFirstFile</c>, handling error conditions. Returns <c>false</c> if there are no matching files.
        /// </summary>
        /// <param name="connect">The internet connection handle.</param>
        /// <param name="search">The search string, which may include wildcards and/or directory information.</param>
        /// <param name="flags">Additional flags for this action.</param>
        /// <param name="first">On return, the details for the first matching remote file/directory.</param>
        /// <param name="find">On return, the find handle.</param>
        /// <returns><c>true</c> if there is at least one matching file; <c>false</c> otherwise.</returns>
        public static bool FtpFindFirstFile(SafeInternetHandle connect, string search, FtpHandle.FindFilesFlags flags, out FtpDirectoryEntry first, out SafeInternetHandle find)
        {
            FtpWin32FindData data;
            find = DoFtpFindFirstFile(connect, search, out data, flags, (IntPtr)1);
            if (find.IsInvalid)
            {
                if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_FILES)
                {
                    first = new FtpDirectoryEntry();
                    return false;
                }

                throw GetLastInternetException();
            }

            first = data.ToFtpDirectoryEntry();
            return true;
        }

        /// <summary>
        /// Invokes <c>FtpFindFirstFile</c>, handling error conditions. Returns <c>false</c> if there are no more matching files.
        /// </summary>
        /// <param name="find">The find handle.</param>
        /// <param name="next">On return, the details for the first matching remote file/directory.</param>
        /// <returns><c>true</c> if another file was found; <c>false</c> if there are no more matching files.</returns>
        public static bool FtpFindNextFile(SafeInternetHandle find, out FtpDirectoryEntry next)
        {
            next = new FtpDirectoryEntry();
            FtpWin32FindData data;
            if (!DoFtpFindNextFile(find, out data))
            {
                if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_FILES)
                {
                    return false;
                }

                throw GetLastInternetException();
            }

            next = data.ToFtpDirectoryEntry();
            return true;
        }

        [DllImport("Wininet.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InternetGetLastResponseInfo(out uint lpdwError, StringBuilder lpszBuffer, ref uint lpdwBufferLength);

        [DllImport("Wininet.dll", EntryPoint = "InternetOpen", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern SafeInternetHandle DoInternetOpen(string lpszAgent, InternetOpenHandle.AccessType dwAccessType, string lpszProxyName, string lpszProxyBypass, InternetOpenHandle.Flags dwFlags);

        [DllImport("Wininet.dll", EntryPoint = "InternetConnect", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern SafeInternetHandle DoInternetConnect(SafeInternetHandle hInternet, string lpszServerName, ushort nServerPort, string lpszUserName, string lpszPassword, InternetConnectHandle.Service dwService, InternetConnectHandle.Flags dwFlags, IntPtr dwContext);

        [DllImport("Wininet.dll", EntryPoint = "FtpCreateDirectory", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpCreateDirectory(SafeInternetHandle hConnect, string lpszDirectory);

        [DllImport("Wininet.dll", EntryPoint = "FtpGetCurrentDirectory", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpGetCurrentDirectory(SafeInternetHandle hConnect, StringBuilder lpszCurrentDirectory, ref uint lpdwCurrentDirectory);

        [DllImport("Wininet.dll", EntryPoint = "FtpDeleteFile", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpDeleteFile(SafeInternetHandle hConnect, string lpszFileName);

        [DllImport("Wininet.dll", EntryPoint = "FtpGetFile", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpGetFile(SafeInternetHandle hConnect, string lpszRemoteFile, string lpszNewFile, [MarshalAs(UnmanagedType.Bool)] bool fFailIfExists, uint dwFlagsAndAttributes, FtpHandle.GetFileFlags dwFlags, IntPtr dwContext);

        [DllImport("Wininet.dll", EntryPoint = "FtpPutFile", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpPutFile(SafeInternetHandle hConnect, string lpszLocalFile, string lpszNewRemoteFile, FtpHandle.PutFileFlags dwFlags, IntPtr dwContext);

        [DllImport("Wininet.dll", EntryPoint = "FtpRemoveDirectory", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpRemoveDirectory(SafeInternetHandle hConnect, string lpszDirectory);

        [DllImport("Wininet.dll", EntryPoint = "FtpRenameFile", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpRenameFile(SafeInternetHandle hConnect, string lpszExisting, string lpszNew);

        [DllImport("Wininet.dll", EntryPoint = "FtpSetCurrentDirectory", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpSetCurrentDirectory(SafeInternetHandle hConnect, string lpszDirectory);

        [DllImport("Wininet.dll", EntryPoint = "FtpFindFirstFile", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern SafeInternetHandle DoFtpFindFirstFile(SafeInternetHandle hConnect, string lpszSearchFile, out FtpWin32FindData lpFindFileData, FtpHandle.FindFilesFlags dwFlags, IntPtr dwContext);

        [DllImport("Wininet.dll", EntryPoint = "InternetFindNextFile", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpFindNextFile(SafeInternetHandle hFind, out FtpWin32FindData lpvFindData);

        [DllImport("Wininet.dll", EntryPoint = "InternetSetStatusCallback", SetLastError = true)]
        private static extern IntPtr DoInternetSetStatusCallback(SafeInternetHandle hInternet, InternetStatusCallback lpfnInternetCallback);

        /// <summary>
        /// Creates a wrapper for the provided internet status callback delegate.
        /// </summary>
        /// <param name="callback">The user-provided internet status callback delegate.</param>
        /// <returns>The wrapped internet status callback delegate.</returns>
        private static InternetStatusCallback CreateInternetStatusCallback(InternetHandle.InternetCallback callback)
        {
            if (callback == null)
            {
                return null;
            }

            return (hInternet, dwContext, dwInternetStatus, lpvStatusInformation, dwStatusInformationLength) =>
            {
                InternetCallbackEventArgs args;
                switch (dwInternetStatus)
                {
                    default:
                    case InternetCallbackEventArgs.StatusCode.ClosingConnection:
                    case InternetCallbackEventArgs.StatusCode.ConnectionClosed:
                    case InternetCallbackEventArgs.StatusCode.DetectingProxy:
                    case InternetCallbackEventArgs.StatusCode.HandleClosing:
                    case InternetCallbackEventArgs.StatusCode.IntermediateResponse:
                    case InternetCallbackEventArgs.StatusCode.P3PHeader:
                    case InternetCallbackEventArgs.StatusCode.ReceivingResponse:
                    case InternetCallbackEventArgs.StatusCode.SendingRequest:
                        args = new InternetCallbackEventArgs
                        {
                            Code = dwInternetStatus,
                            RawData = lpvStatusInformation,
                        };
                        break;

                    case InternetCallbackEventArgs.StatusCode.ConnectedToServer:
                    case InternetCallbackEventArgs.StatusCode.ConnectingToServer:
                        SocketAddress socketAddress = new SocketAddress((AddressFamily)BitConverter.ToUInt16(lpvStatusInformation, 0), 16);
                        lpvStatusInformation.Select((x, i) => socketAddress[i] = x).Run();
                        args = new InternetCallbackEventArgs.Socket
                        {
                            Code = dwInternetStatus,
                            RawData = lpvStatusInformation,
                            EndPoint = new IPEndPoint(0, 0).Create(socketAddress),
                        };
                        break;

                    case InternetCallbackEventArgs.StatusCode.CookieHistory:
                        args = new InternetCallbackEventArgs.CookieHistory
                        {
                            Code = dwInternetStatus,
                            RawData = lpvStatusInformation,
                            Accepted = BitConverter.ToUInt32(lpvStatusInformation, 0) != 0,
                            Leashed = BitConverter.ToUInt32(lpvStatusInformation, 4) != 0,
                            Downgraded = BitConverter.ToUInt32(lpvStatusInformation, 8) != 0,
                            Rejected = BitConverter.ToUInt32(lpvStatusInformation, 12) != 0,
                        };
                        break;

                    case InternetCallbackEventArgs.StatusCode.CookieReceived:
                    case InternetCallbackEventArgs.StatusCode.CookieSent:
                    case InternetCallbackEventArgs.StatusCode.RequestSent:
                    case InternetCallbackEventArgs.StatusCode.ResponseReceived:
                    case InternetCallbackEventArgs.StatusCode.StateChange:
                        args = new InternetCallbackEventArgs.Number
                        {
                            Code = dwInternetStatus,
                            RawData = lpvStatusInformation,
                            Value = BitConverter.ToInt32(lpvStatusInformation, 0),
                        };
                        break;

                    case InternetCallbackEventArgs.StatusCode.HandleCreated:
                    case InternetCallbackEventArgs.StatusCode.RequestComplete:
                        IntPtr ptr = IntPtr.Size == 4 ? (IntPtr)BitConverter.ToUInt32(lpvStatusInformation, 0) : (IntPtr)BitConverter.ToUInt64(lpvStatusInformation, 0);
                        args = new InternetCallbackEventArgs.AsyncResult
                        {
                            Code = dwInternetStatus,
                            RawData = lpvStatusInformation,
                            Result = IntPtr.Size == 4 ? (IntPtr)Marshal.ReadInt32(ptr) : (IntPtr)Marshal.ReadInt64(ptr),
                            Error = IntPtr.Size == 4 ? Marshal.ReadInt32(ptr, 4) : Marshal.ReadInt32(ptr, 8),
                        };
                        break;

                    case InternetCallbackEventArgs.StatusCode.ResolvingName:
                    case InternetCallbackEventArgs.StatusCode.NameResolved:
                    case InternetCallbackEventArgs.StatusCode.Redirect:
                        args = new InternetCallbackEventArgs.String
                        {
                            Code = dwInternetStatus,
                            RawData = lpvStatusInformation,
                            Value = Encoding.Unicode.GetString(lpvStatusInformation),
                        };
                        break;
                }

                callback(args);
            };
        }

        /// <summary>
        /// Constructs an <see cref="Exception"/> instance for the last Win32 error, using server-reported error message if possible; otherwise WinInet.dll error messages; otherwise system error messages.
        /// </summary>
        /// <returns>An <see cref="Exception"/> instance for the last Win32 error.</returns>
        private static Exception GetLastInternetException()
        {
            int win32Code = Marshal.GetLastWin32Error();

            if (win32Code != ERROR_INTERNET_EXTENDED_ERROR)
            {
                string inetMessage = Interop.TryFormatMessageFromDll("Wininet.dll", win32Code);
                if (inetMessage != null)
                {
                    return new Win32Exception(win32Code, "WinInet Error 0x" + win32Code.ToString("X") + ": " + inetMessage);
                }

                return new Win32Exception(win32Code);
            }

            uint code;
            StringBuilder sb = new StringBuilder(32);
            while (true)
            {
                uint length = (uint)sb.Capacity + 1;
                if (InternetGetLastResponseInfo(out code, sb, ref length))
                {
                    return new InternetException((int)code, sb.ToString());
                }
                else if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
                {
                    string inetMessage = Interop.TryFormatMessageFromDll("Wininet.dll", win32Code);
                    if (inetMessage != null)
                    {
                        return new Win32Exception(win32Code, "WinInet Error 0x" + win32Code.ToString("X") + ": " + inetMessage);
                    }

                    return new Win32Exception(win32Code);
                }

                sb.Capacity *= 2;
            }
        }

        /// <summary>
        /// Marshal structure for FTP find file details. Almost identical to <c>WIN32_FILE_DATA</c>, except that the FILETIME fields are local time.
        /// </summary>
        /// <remarks>
        /// <para>Normally, the Pack is correct at 8; however, for ease of use, we've redefined a couple of uint+uint pairs to be a single long,
        /// so we need to adjust the packing so that padding isn't inserted after "Attributes"</para>
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public struct FtpWin32FindData
        {
            /// <summary>
            /// The basic attributes of the file or subdirectory.
            /// </summary>
            public FtpDirectoryEntry.AttributeFlags Attributes;

            /// <summary>
            /// The time that the file or subdirectory was created, if known.
            /// </summary>
            public long CreationTime;

            /// <summary>
            /// The time that the file or subdirectory was last accessed, if known.
            /// </summary>
            public long LastAccessTime;

            /// <summary>
            /// The time that the file or subdirectory was last accessed, if known.
            /// </summary>
            public long LastWriteTime;

            /// <summary>
            /// The high 32 bits of the size of the file.
            /// </summary>
            public uint FileSizeHigh;

            /// <summary>
            /// The low 32 bits of the size of the file.
            /// </summary>
            public uint FileSizeLow;

            /// <summary>
            /// Reserved for future expansionB.
            /// </summary>
            public ulong _;

            /// <summary>
            /// The name of the file/directory.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string FileName;

            /// <summary>
            /// The 8.3 shortened name of the file/directory.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string AlternateFileName;

            /// <summary>
            /// Converts from <see cref="FtpWin32FindData"/> to <see cref="FtpDirectoryEntry"/>.
            /// </summary>
            /// <returns>An <see cref="FtpDirectoryEntry"/> instance</returns>
            public FtpDirectoryEntry ToFtpDirectoryEntry()
            {
                return new FtpDirectoryEntry()
                {
                    Attributes = this.Attributes,
                    Size = (long)(((ulong)this.FileSizeHigh << 32) | this.FileSizeLow),
                    Name = this.FileName,
                    CreationTime = DateTime.FromFileTime(SafeNativeMethods.FtpLocalFileTimeToFileTime(this.CreationTime)),
                    LastAccessTime = DateTime.FromFileTime(SafeNativeMethods.FtpLocalFileTimeToFileTime(this.LastAccessTime)),
                    LastWriteTime = DateTime.FromFileTime(SafeNativeMethods.FtpLocalFileTimeToFileTime(this.LastWriteTime)),
                };
            }
        }
    }
}
