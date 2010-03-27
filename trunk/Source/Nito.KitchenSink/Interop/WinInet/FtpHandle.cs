// <copyright file="FtpHandle.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An FTP connection handle. Normally, the <see cref="FtpConnection"/> class is used instead of this class. Note that this wrapper does NOT support asynchronous operations! Multiple threads may safely call <see cref="Dispose"/>.
    /// </summary>
    public sealed class FtpHandle : InternetConnectHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpHandle"/> class with the specified parameters. Normally, <see cref="InternetOpenHandle.Connect"/> or <see cref="InternetOpenHandle.ConnectFtp"/> is used instead of this constructor.
        /// </summary>
        /// <param name="parent">The parent internet connection.</param>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="flags">The connection flags, such as <see cref="InternetConnectHandle.Flags.Passive"/> for passive FTP.</param>
        public FtpHandle(InternetHandle parent, string serverName, int serverPort, string username, string password, InternetConnectHandle.Flags flags)
            : base(parent, serverName, serverPort, username, password, Service.Ftp, flags)
        {
        }

        /// <summary>
        /// Additional flags for the <see cref="FindFiles"/> operation.
        /// </summary>
        [Flags]
        public enum FindFilesFlags : int
        {
            /// <summary>
            /// No additional flags.
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Forces a reload if there is no Expires time and no LastModified time returned from the server when determining whether to reload the item from the network.
            /// </summary>
            Hyperlink = 0x00000400,

            /// <summary>
            /// Causes a temporary file to be created if the file cannot be cached.
            /// </summary>
            NeedFile = 0x00000010,

            /// <summary>
            /// Does not add the returned entity to the cache.
            /// </summary>
            NoCacheWrite = 0x04000000,

            /// <summary>
            /// Forces a download of the requested file, object, or directory listing from the origin server, not from the cache.
            /// </summary>
            Reload = unchecked((int)0x80000000),

            /// <summary>
            /// Reloads FTP resources.
            /// </summary>
            Resynchronize = 0x00000800,
        }

        /// <summary>
        /// Additional flags for the <see cref="GetFile"/> operation.
        /// </summary>
        [Flags]
        public enum GetFileFlags : int
        {
            /// <summary>
            /// Transfers file as ASCII.
            /// </summary>
            Ascii = 0x1,

            /// <summary>
            /// Transfers file as binary.
            /// </summary>
            Binary = 0x2,

            /// <summary>
            /// Forces a reload if there is no Expires time and no LastModified time returned from the server when determining whether to reload the item from the network.
            /// </summary>
            Hyperlink = 0x00000400,

            /// <summary>
            /// Causes a temporary file to be created if the file cannot be cached.
            /// </summary>
            NeedFile = 0x00000010,

            /// <summary>
            /// Forces a download of the requested file, object, or directory listing from the origin server, not from the cache.
            /// </summary>
            Reload = unchecked((int)0x80000000),

            /// <summary>
            /// Reloads FTP resources.
            /// </summary>
            Resynchronize = 0x00000800,
        }

        /// <summary>
        /// Additional flags for the <see cref="PutFile"/> operation.
        /// </summary>
        [Flags]
        public enum PutFileFlags : int
        {
            /// <summary>
            /// Transfers file as ASCII.
            /// </summary>
            Ascii = 0x1,

            /// <summary>
            /// Transfers file as binary.
            /// </summary>
            Binary = 0x2,
        }

        /// <summary>
        /// Creates the specified directory on the remote FTP server.
        /// </summary>
        /// <param name="directory">The directory to create.</param>
        public void CreateDirectory(string directory)
        {
            UnsafeNativeMethods.FtpCreateDirectory(this.SafeInternetHandle, directory);
        }

        /// <summary>
        /// Retrieves the current working directory on the remote FTP server.
        /// </summary>
        /// <returns>The current working directory on the remote FTP server.</returns>
        public string GetCurrentDirectory()
        {
            return UnsafeNativeMethods.FtpGetCurrentDirectory(this.SafeInternetHandle);
        }

        /// <summary>
        /// Deletes the specified file on the remote FTP server.
        /// </summary>
        /// <param name="fileName">The file to delete.</param>
        public void DeleteFile(string fileName)
        {
            UnsafeNativeMethods.FtpDeleteFile(this.SafeInternetHandle, fileName);
        }

        /// <summary>
        /// Downloads the specified remote file from the FTP server, saving it at a local path and filename.
        /// </summary>
        /// <param name="remoteFile">The remote file to download.</param>
        /// <param name="localFile">The local path and filename to which to save the file.</param>
        /// <param name="failIfExists">Whether to fail if the local file specified by <paramref name="localFile"/> already exists.</param>
        /// <param name="flags">Additional flags for this action. At least <see cref="GetFileFlags.Ascii"/> or <see cref="GetFileFlags.Binary"/> should be specified.</param>
        public void GetFile(string remoteFile, string localFile, bool failIfExists, GetFileFlags flags)
        {
            UnsafeNativeMethods.FtpGetFile(this.SafeInternetHandle, remoteFile, localFile, failIfExists, flags);
        }

        /// <summary>
        /// Uploads the specified local file to the FTP server, saving it at a remote path and filename.
        /// </summary>
        /// <param name="localFile">The local file to upload.</param>
        /// <param name="remoteFile">The remote path and filename to which to save the file.</param>
        /// <param name="flags">Additional flags for this action. At least <see cref="PutFileFlags.Ascii"/> or <see cref="PutFileFlags.Binary"/> should be specified.</param>
        public void PutFile(string localFile, string remoteFile, PutFileFlags flags)
        {
            UnsafeNativeMethods.FtpPutFile(this.SafeInternetHandle, localFile, remoteFile, flags);
        }

        /// <summary>
        /// Removes the specified directory from the remote FTP server.
        /// </summary>
        /// <param name="directory">The directory to remove.</param>
        public void RemoveDirectory(string directory)
        {
            UnsafeNativeMethods.FtpRemoveDirectory(this.SafeInternetHandle, directory);
        }

        /// <summary>
        /// Renames the specified file on the FTP server.
        /// </summary>
        /// <param name="oldName">The old file name.</param>
        /// <param name="newName">The new file name.</param>
        public void RenameFile(string oldName, string newName)
        {
            UnsafeNativeMethods.FtpRenameFile(this.SafeInternetHandle, oldName, newName);
        }

        /// <summary>
        /// Sets the current working directory on the remote FTP server.
        /// </summary>
        /// <param name="directory">The new current working directory.</param>
        public void SetCurrentDirectory(string directory)
        {
            UnsafeNativeMethods.FtpSetCurrentDirectory(this.SafeInternetHandle, directory);
        }

        /// <summary>
        /// Finds matching files on the remote FTP server.
        /// </summary>
        /// <param name="search">The search string, which may include wildcards and/or directory information.</param>
        /// <param name="flags">Additional flags for this action.</param>
        /// <returns>All files matching the query on the remote FTP server.</returns>
        public IList<FtpDirectoryEntry> FindFiles(string search, FindFilesFlags flags)
        {
            List<FtpDirectoryEntry> ret = new List<FtpDirectoryEntry>();
            FtpDirectoryEntry entry;
            Nito.KitchenSink.WinInet.SafeInternetHandle find;
            if (!UnsafeNativeMethods.FtpFindFirstFile(this.SafeInternetHandle, search, flags, out entry, out find))
            {
                return ret;
            }

            using (find)
            {
                ret.Add(entry);
                while (UnsafeNativeMethods.FtpFindNextFile(find, out entry))
                {
                    ret.Add(entry);
                }
            }

            return ret;
        }

        /// <summary>
        /// Finds matching files on the remote FTP server.
        /// </summary>
        /// <param name="search">The search string, which may include wildcards and/or directory information.</param>
        /// <returns>All files matching the query on the remote FTP server.</returns>
        public IList<FtpDirectoryEntry> FindFiles(string search)
        {
            return this.FindFiles(search, FindFilesFlags.None);
        }

        /// <summary>
        /// Retrieves all files from the current working directory on the remote FTP server.
        /// </summary>
        /// <returns>All files in the current working directory on the remote FTP server.</returns>
        public IList<FtpDirectoryEntry> FindFiles()
        {
            return this.FindFiles(string.Empty, FindFilesFlags.None);
        }
    }
}
