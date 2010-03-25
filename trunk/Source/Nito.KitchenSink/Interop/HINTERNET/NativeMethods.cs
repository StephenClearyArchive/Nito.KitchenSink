using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Net;
using System.Net.Sockets;
using Nito.Linq;

namespace Nito.KitchenSink
{
    internal static partial class NativeMethods
    {
        [DllImport("Kernel32.dll", EntryPoint = "LocalFileTimeToFileTime", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FtpLocalFileTimeToFileTime(ref long lpLocalFileTime, out long lpFileTime);

        public static long FtpLocalFileTimeToFileTime(long localFileTime)
        {
            long ret;
            if (!FtpLocalFileTimeToFileTime(ref localFileTime, out ret))
            {
                throw Interop.GetLastWin32Exception();
            }

            return ret;
        }

        [DllImport("Wininet.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InternetCloseHandle(IntPtr hInternet);

        [DllImport("Wininet.dll", EntryPoint = "InternetOpen", SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern SafeInternetHandle DoInternetOpen(string lpszAgent, InternetOpenHandle.AccessType dwAccessType, string lpszProxyName, string lpszProxyBypass, InternetOpenHandle.Flags dwFlags);

        public static SafeInternetHandle InternetOpen(string agent, InternetOpenHandle.AccessType accessType, string proxyName, string proxyBypass, InternetOpenHandle.Flags flags)
        {
            SafeInternetHandle ret = DoInternetOpen(agent, accessType, proxyName, proxyBypass, flags);
            if (ret.IsInvalid)
            {
                throw GetLastInternetException();
            }

            return ret;
        }

        [DllImport("Wininet.dll", EntryPoint = "InternetConnect", SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern SafeInternetHandle DoInternetConnect(SafeInternetHandle hInternet, string lpszServerName, ushort nServerPort, string lpszUserName, string lpszPassword, InternetConnectHandle.Service dwService, InternetConnectHandle.Flags dwFlags, IntPtr dwContext);

        public static SafeInternetHandle InternetConnect(SafeInternetHandle internet, string serverName, ushort serverPort, string username, string password, InternetConnectHandle.Service service, InternetConnectHandle.Flags flags)
        {
            SafeInternetHandle ret = DoInternetConnect(internet, serverName, serverPort, username, password, service, flags, (IntPtr)1);
            if (ret.IsInvalid)
            {
                throw GetLastInternetException();
            }

            return ret;
        }

        [DllImport("Wininet.dll", EntryPoint = "FtpCreateDirectory", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpCreateDirectory(SafeInternetHandle hConnect, string lpszDirectory);

        public static void FtpCreateDirectory(SafeInternetHandle connect, string directory)
        {
            if (!DoFtpCreateDirectory(connect, directory))
            {
                throw GetLastInternetException();
            }
        }

        [DllImport("Wininet.dll", EntryPoint = "FtpGetCurrentDirectory", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpGetCurrentDirectory(SafeInternetHandle hConnect, StringBuilder lpszCurrentDirectory, ref uint lpdwCurrentDirectory);

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

        [DllImport("Wininet.dll", EntryPoint = "FtpDeleteFile", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpDeleteFile(SafeInternetHandle hConnect, string lpszFileName);

        public static void FtpDeleteFile(SafeInternetHandle connect, string fileName)
        {
            if (!DoFtpDeleteFile(connect, fileName))
            {
                throw GetLastInternetException();
            }
        }

        [DllImport("Wininet.dll", EntryPoint = "FtpGetFile", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpGetFile(SafeInternetHandle hConnect, string lpszRemoteFile, string lpszNewFile, [MarshalAs(UnmanagedType.Bool)] bool fFailIfExists, uint dwFlagsAndAttributes, FtpHandle.GetFileFlags dwFlags, IntPtr dwContext);

        public static void FtpGetFile(SafeInternetHandle connect, string remoteFile, string localFile, bool failIfExists, FtpHandle.GetFileFlags flags)
        {
            if (!DoFtpGetFile(connect, remoteFile, localFile, failIfExists, 0, flags, (IntPtr)1))
            {
                throw GetLastInternetException();
            }
        }

        [DllImport("Wininet.dll", EntryPoint = "FtpPutFile", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpPutFile(SafeInternetHandle hConnect, string lpszLocalFile, string lpszNewRemoteFile, FtpHandle.PutFileFlags dwFlags, IntPtr dwContext);

        public static void FtpPutFile(SafeInternetHandle connect, string localFile, string remoteFile, FtpHandle.PutFileFlags flags)
        {
            if (!DoFtpPutFile(connect, localFile, remoteFile, flags, (IntPtr)1))
            {
                throw GetLastInternetException();
            }
        }

        [DllImport("Wininet.dll", EntryPoint = "FtpRemoveDirectory", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpRemoveDirectory(SafeInternetHandle hConnect, string lpszDirectory);

        public static void FtpRemoveDirectory(SafeInternetHandle connect, string directory)
        {
            if (!DoFtpRemoveDirectory(connect, directory))
            {
                throw GetLastInternetException();
            }
        }

        [DllImport("Wininet.dll", EntryPoint = "FtpRenameFile", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpRenameFile(SafeInternetHandle hConnect, string lpszExisting, string lpszNew);

        public static void FtpRenameFile(SafeInternetHandle connect, string oldName, string newName)
        {
            if (!DoFtpRenameFile(connect, oldName, newName))
            {
                throw GetLastInternetException();
            }
        }

        [DllImport("Wininet.dll", EntryPoint = "FtpSetCurrentDirectory", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DoFtpSetCurrentDirectory(SafeInternetHandle hConnect, string lpszDirectory);

        public static void FtpSetCurrentDirectory(SafeInternetHandle connect, string directory)
        {
            if (!DoFtpSetCurrentDirectory(connect, directory))
            {
                throw GetLastInternetException();
            }
        }

        private delegate void InternetStatusCallback(IntPtr hInternet, IntPtr dwContext, InternetCallbackEventArgs.StatusCode dwInternetStatus, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] lpvStatusInformation, uint dwStatusInformationLength);

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
                                Value = BitConverter.ToUInt32(lpvStatusInformation, 0),
                            };
                            break;

                        case InternetCallbackEventArgs.StatusCode.HandleCreated:
                        case InternetCallbackEventArgs.StatusCode.RequestComplete:
                            args = new InternetCallbackEventArgs.AsyncResult
                            {
                                Code = dwInternetStatus,
                                RawData = lpvStatusInformation,
                                Result = IntPtr.Size == 4 ? (IntPtr)BitConverter.ToUInt32(lpvStatusInformation, 0) : (IntPtr)BitConverter.ToUInt64(lpvStatusInformation, 0),
                                Error = IntPtr.Size == 4 ? BitConverter.ToUInt32(lpvStatusInformation, 4) : BitConverter.ToUInt32(lpvStatusInformation, 8),
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

        [DllImport("Wininet.dll", EntryPoint = "InternetSetStatusCallback", SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr DoInternetSetStatusCallback(SafeInternetHandle hInternet, InternetStatusCallback lpfnInternetCallback);

        public static object InternetSetStatusCallback(SafeInternetHandle internet, InternetHandle.InternetCallback callback)
        {
            // Note: this will only work for synchronous callbacks! Asynchronous callbacks are not yet supported.
            InternetStatusCallback ret = CreateInternetStatusCallback(callback);
            DoInternetSetStatusCallback(internet, ret);
            return ret;
        }

        [DllImport("Wininet.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InternetGetLastResponseInfo(out uint lpdwError, StringBuilder lpszBuffer, ref uint lpdwBufferLength);

        private const int MAX_PATH = 260;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int ERROR_INTERNET_EXTENDED_ERROR = 12003;

        private static Exception GetLastInternetException()
        {
            Exception ex = Interop.GetLastWin32Exception();
            if (Marshal.GetLastWin32Error() != ERROR_INTERNET_EXTENDED_ERROR)
            {
                return ex;
            }

            uint code;
            StringBuilder sb = new StringBuilder(32);
            while (true)
            {
                uint length = (uint)sb.Capacity + 1;
                if (InternetGetLastResponseInfo(out code, sb, ref length))
                {
                    return new InternetException(code, sb.ToString());
                }
                else if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
                {
                    return ex;
                }

                sb.Capacity *= 2;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FtpWin32FindData
        {
            public FtpDirectoryEntry.AttributeFlags Attributes;
            public long CreationTime;
            public long LastAccessTime;
            public long LastWriteTime;
            public ulong FileSize;
            public long _;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string FileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string AlternateFileName;
        }
    }
}
