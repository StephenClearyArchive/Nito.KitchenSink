// <copyright file="FileSystemPathExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;

    /// <summary>
    /// Provides extensions to expose <see cref="FileSystemPath"/> with other system types.
    /// </summary>
    public static class FileSystemPathExtensions
    {
        /// <summary>
        /// Treats a string as a file system path.
        /// </summary>
        /// <param name="source">The source path.</param>
        /// <returns>The file system path.</returns>
        public static FileSystemPath AsFileSystemPath(this string source)
        {
            return source;
        }

        /// <summary>
        /// Treats a special folder as a file system path. The resulting path is empty if the special folder does not exist or is virtual.
        /// </summary>
        /// <param name="specialFolder">The special folder.</param>
        /// <returns>The file system path of the special folder.</returns>
        public static FileSystemPath AsFileSystemPath(this Environment.SpecialFolder specialFolder)
        {
            return Environment.GetFolderPath(specialFolder);
        }
    }
}
