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
