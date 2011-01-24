// <copyright file="FileSystemPathExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.FileSystemPaths
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Runtime.InteropServices;
    using System.Text;
    using Nito.KitchenSink.PInvokeInterop;

    /// <summary>
    /// Provides extensions to expose <see cref="FileSystemPath"/> with other system types.
    /// </summary>
    public static class FileSystemPathExtensions
    {
        /// <summary>
        /// Treats a string as a file system path.
        /// </summary>
        /// <param name="source">The source path. May not be <c>null</c>.</param>
        /// <returns>The file system path.</returns>
        public static FileSystemPath AsFileSystemPath(this string source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<FileSystemPath>() != null);
            return new FileSystemPath(source);
        }

        /// <summary>
        /// Treats a special folder as a file system path. The resulting path is empty if the special folder does not exist or is virtual.
        /// </summary>
        /// <param name="specialFolder">The special folder.</param>
        /// <returns>The file system path of the special folder.</returns>
        public static FileSystemPath AsFileSystemPath(this Environment.SpecialFolder specialFolder)
        {
            Contract.Ensures(Contract.Result<FileSystemPath>() != null);
            return new FileSystemPath(Environment.GetFolderPath(specialFolder));
        }

        /// <summary>
        /// Gets the file system path corresponding to a file or directory object.
        /// </summary>
        /// <param name="fileSystemInfo">The file or directory object. May not be <c>null</c>.</param>
        /// <returns>The file system path.</returns>
        public static FileSystemPath GetFileSystemPath(this System.IO.FileSystemInfo fileSystemInfo)
        {
            Contract.Requires(fileSystemInfo != null);
            Contract.Ensures(Contract.Result<FileSystemPath>() != null);
            return new FileSystemPath(fileSystemInfo.FullName);
        }

        /// <summary>
        /// Gets the "long path" form of the path corresponding to a file or directory object, if the file system supports long paths. If this path does not have a long path, then the short path form is returned. The file system object must exist.
        /// </summary>
        /// <param name="fileSystemInfo">The file or directory object. May not be <c>null</c>.</param>
        /// <returns>The file system path.</returns>
        public static FileSystemPath GetLongFileSystemPath(this System.IO.FileSystemInfo fileSystemInfo)
        {
            Contract.Requires(fileSystemInfo != null);
            Contract.Ensures(Contract.Result<FileSystemPath>() != null);
            return new FileSystemPath(SafeNativeMethods.GetLongPathName(fileSystemInfo.FullName));
        }

        /// <summary>
        /// Gets the "short path" form of the path corresponding to a file or directory object, if the file system supports short paths. If this path does not have a short path, then the long path form is returned. The file system object must exist.
        /// </summary>
        /// <param name="fileSystemInfo">The file or directory object. May not be <c>null</c>.</param>
        /// <returns>The file system path.</returns>
        public static FileSystemPath GetShortFileSystemPath(this System.IO.FileSystemInfo fileSystemInfo)
        {
            Contract.Requires(fileSystemInfo != null);
            Contract.Ensures(Contract.Result<FileSystemPath>() != null);
            return new FileSystemPath(SafeNativeMethods.GetShortPathName(fileSystemInfo.FullName));
        }

        private static class SafeNativeMethods
        {
            // Currently, we do not handle long paths - but neither does the .NET BCL.
            public const uint MAX_PATH = 260;

            [DllImport("Kernel32.dll", EntryPoint = "GetShortPathName", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            private static extern uint GetShortPathNameNative(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

            public static string GetShortPathName(string path)
            {
                Contract.Requires(path != null);
                Contract.Ensures(Contract.Result<string>() != null);

                uint bufferSize = MAX_PATH + 1;
                var ret = new StringBuilder((int)bufferSize);
                int retLength = (int)GetShortPathNameNative(path, ret, bufferSize);
                if (retLength == 0)
                {
                    throw Interop.GetLastWin32Exception("GetShortPathName");
                }

                Contract.Assume(retLength <= ret.Length);
                return ret.ToString(0, retLength);
            }

            [DllImport("Kernel32.dll", EntryPoint = "GetLongPathName", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            private static extern uint GetLongPathNameNative(string lpszShortPath, StringBuilder lpszLongPath, uint cchBuffer);

            public static string GetLongPathName(string path)
            {
                Contract.Requires(path != null);
                Contract.Ensures(Contract.Result<string>() != null);

                uint bufferSize = MAX_PATH + 1;
                var ret = new StringBuilder((int)bufferSize);
                int retLength = (int)GetLongPathNameNative(path, ret, bufferSize);
                if (retLength == 0)
                {
                    throw Interop.GetLastWin32Exception("GetLongPathName");
                }

                Contract.Assume(retLength <= ret.Length);
                return ret.ToString(0, retLength);
            }
        }
    }
}
