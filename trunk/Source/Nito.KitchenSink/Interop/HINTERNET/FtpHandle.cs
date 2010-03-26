using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    [CLSCompliant(false)]
    public sealed class FtpHandle : InternetConnectHandle
    {
        public FtpHandle(InternetHandle parent, string serverName, ushort serverPort, string username, string password, InternetConnectHandle.Flags flags)
            : base(parent, serverName, serverPort, username, password, Service.Ftp, flags)
        {
        }

        public void CreateDirectory(string directory)
        {
            NativeMethods.FtpCreateDirectory(this.SafeInternetHandle, directory);
        }

        public string GetCurrentDirectory()
        {
            return NativeMethods.FtpGetCurrentDirectory(this.SafeInternetHandle);
        }

        public void DeleteFile(string fileName)
        {
            NativeMethods.FtpDeleteFile(this.SafeInternetHandle, fileName);
        }

        public void GetFile(string remoteFile, string localFile, bool failIfExists, GetFileFlags flags)
        {
            NativeMethods.FtpGetFile(this.SafeInternetHandle, remoteFile, localFile, failIfExists, flags);
        }

        public void PutFile(string localFile, string remoteFile, PutFileFlags flags)
        {
            NativeMethods.FtpPutFile(this.SafeInternetHandle, localFile, remoteFile, flags);
        }

        public void RemoveDirectory(string directory)
        {
            NativeMethods.FtpRemoveDirectory(this.SafeInternetHandle, directory);
        }

        public void RenameFile(string oldName, string newName)
        {
            NativeMethods.FtpRenameFile(this.SafeInternetHandle, oldName, newName);
        }

        public void SetCurrentDirectory(string directory)
        {
            NativeMethods.FtpSetCurrentDirectory(this.SafeInternetHandle, directory);
        }

        public IList<FtpDirectoryEntry> FindFiles(string search, FindFilesFlags flags)
        {
            List<FtpDirectoryEntry> ret = new List<FtpDirectoryEntry>();
            FtpDirectoryEntry entry;
            Nito.KitchenSink.SafeInternetHandle find;
            if (!NativeMethods.FtpFindFirstFile(this.SafeInternetHandle, search, flags, out entry, out find))
            {
                return ret;
            }

            using (find)
            {
                ret.Add(entry);
                while (NativeMethods.FtpFindNextFile(find, out entry))
                {
                    ret.Add(entry);
                }
            }

            return ret;
        }

        public IList<FtpDirectoryEntry> FindFiles(string search)
        {
            return this.FindFiles(search, FindFilesFlags.None);
        }

        public IList<FtpDirectoryEntry> FindFiles()
        {
            return this.FindFiles(string.Empty, FindFilesFlags.None);
        }

        [Flags]
        public enum FindFilesFlags : uint
        {
            None = 0x0,
            Hyperlink = 0x00000400,
            NeedFile = 0x00000010,
            NoCacheWrite = 0x04000000,
            Reload = 0x80000000,
            Resynchronize = 0x00000800,
        }

        [Flags]
        public enum GetFileFlags : uint
        {
            Ascii = 0x1,
            Binary = 0x2,
            Hyperlink = 0x00000400,
            NeedFile = 0x00000010,
            Reload = 0x80000000,
            Resynchronize = 0x00000800,
        }

        [Flags]
        public enum PutFileFlags : uint
        {
            Ascii = 0x1,
            Binary = 0x2,
        }
    }
}
