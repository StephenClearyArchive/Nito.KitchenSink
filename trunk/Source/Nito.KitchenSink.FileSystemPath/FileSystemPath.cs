// <copyright file="FileSystemPath.cs" company="Nito Programs">
//     Copyright (c) 2009-2011 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.FileSystemPaths
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// A string that is a file system path. All members of this type (with the exception of <see cref="Absolute"/>) do not interact with the actual file system; they are only string manipulation members. To interact with the file system, call <see cref="ToFileInfo"/> or <see cref="ToDirectoryInfo"/>.
    /// </summary>
    public sealed class FileSystemPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemPath"/> class with the specified actual path string.
        /// </summary>
        /// <param name="path">The actual path string. May not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
        public FileSystemPath(string path)
        {
            Contract.Requires(path != null);
            this.path = path;
        }

        /// <summary>
        /// Gets the file system path that is the current working directory for the process.
        /// </summary>
        public static FileSystemPath CurrentDirectory
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);
                return new FileSystemPath(System.IO.Directory.GetCurrentDirectory());
            }
        }

        /// <summary>
        /// Enumerates the paths representing the logical drives in the system.
        /// </summary>
        public static IEnumerable<FileSystemPath> LogicalDrives
        {
            [SuppressMessage("Microsoft.Contracts", "Ensures-55-99")]
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<FileSystemPath>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<FileSystemPath>>(), x => x != null));
                return System.IO.Directory.GetLogicalDrives().Select(x => new FileSystemPath(x));
            }
        }

        /// <summary>
        /// Gets random name that can be used as a folder or file name.
        /// </summary>
        public static FileSystemPath RandomFileName
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);
                return new FileSystemPath(System.IO.Path.GetRandomFileName());
            }
        }

        /// <summary>
        /// Gets the path of a temporary file that has been created.
        /// </summary>
        public static FileSystemPath TempFile
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);
                return new FileSystemPath(System.IO.Path.GetTempFileName());
            }
        }

        /// <summary>
        /// Gets the path of the temporary directory.
        /// </summary>
        public static FileSystemPath TempPath
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);
                return new FileSystemPath(System.IO.Path.GetTempPath());
            }
        }

        private readonly string path;
        /// <summary>
        /// Gets the actual path string. This is never <c>null</c>, but may be empty.
        /// </summary>
        public string Path
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return this.path;
            }
        }

        /// <summary>
        /// Gets the root portion of this path. Returns an empty path if this path does not contain a root portion.
        /// </summary>
        public FileSystemPath Root
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);
                return new FileSystemPath(System.IO.Path.GetPathRoot(this));
            }
        }

        /// <summary>
        /// Gets the directory portion of this path. Returns an empty path if this path does not contain a directory portion.
        /// </summary>
        public FileSystemPath DirectoryName
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);

                // Path.GetDirectoryName will return null if this path is a root directory
                return new FileSystemPath(System.IO.Path.GetDirectoryName(this) ?? string.Empty);
            }
        }

        /// <summary>
        /// Gets the file name portion of this path. Returns an empty path if this path does not contain a file name portion.
        /// </summary>
        public FileSystemPath FileName
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);
                return new FileSystemPath(System.IO.Path.GetFileName(this));
            }
        }

        /// <summary>
        /// Gets the file name portion of this path, stripping the extension. Returns an empty path if this path does not contain a file name portion.
        /// </summary>
        public FileSystemPath FileNameWithoutExtension
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);
                return new FileSystemPath(System.IO.Path.GetFileNameWithoutExtension(this));
            }
        }

        /// <summary>
        /// Gets the absolute path for this path, using the current directory if this is not already an absolute path. This property does interact with the file system.
        /// </summary>
        public FileSystemPath Absolute
        {
            get
            {
                Contract.Ensures(Contract.Result<FileSystemPath>() != null);
                if (this.Path == string.Empty)
                {
                    return CurrentDirectory;
                }

                return new FileSystemPath(System.IO.Path.GetFullPath(this));
            }
        }

        /// <summary>
        /// Gets the extension of the file name portion of this path, including the ".". Returns an empty string if this path does not contain a file name portion or the file name does not have an extension.
        /// </summary>
        public string Extension
        {
            get
            {
                return System.IO.Path.GetExtension(this);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this path has an extension.
        /// </summary>
        public bool HasExtension
        {
            get
            {
                return System.IO.Path.HasExtension(this);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this path is absolute.
        /// </summary>
        public bool IsAbsolute
        {
            get
            {
                return System.IO.Path.IsPathRooted(this);
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.path != null);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="FileSystemPath"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="source">The source path. May not be <c>null</c>.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(FileSystemPath source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<string>() != null);
            return source.Path;
        }

        /// <summary>
        /// Changes the extension of the file name portion of this path. Returns an empty string if this path does not contain a file name portion.
        /// </summary>
        /// <param name="extension">The new extension, with or without the ".". May be <c>null</c> to remove an existing extension.</param>
        /// <returns>A path with a changed extension.</returns>
        public FileSystemPath ChangeExtension(string extension)
        {
            Contract.Ensures(Contract.Result<FileSystemPath>() != null);
            return new FileSystemPath(System.IO.Path.ChangeExtension(this.Path, extension));
        }

        /// <summary>
        /// Combines two or more paths. Absolute paths remove previous path information. The resulting path may have nested ".." designations; these may be resolved by reading the <see cref="Absolute"/> property.
        /// </summary>
        /// <param name="others">The other paths to combine with this one. None of these may be <c>null</c>.</param>
        /// <returns>The combined path.</returns>
        public FileSystemPath Combine(params FileSystemPath[] others)
        {
            Contract.Requires(others != null);
            Contract.Requires(Contract.ForAll(others, x => x != null));
            Contract.Ensures(Contract.Result<FileSystemPath>() != null);

            var otherStrings = others.Select(x => x.Path).ToArray();
            Contract.Assume(Contract.ForAll(otherStrings, x => x != null));
            return this.Combine(otherStrings);
        }

        /// <summary>
        /// Combines two or more paths. Absolute paths remove previous path information. The resulting path may have nested ".." designations; these may be resolved by reading the <see cref="Absolute"/> property.
        /// </summary>
        /// <param name="others">The other paths to combine with this one. None of these may be <c>null</c>.</param>
        /// <returns>The combined path.</returns>
        public FileSystemPath Combine(params string[] others)
        {
            Contract.Requires(others != null);
            Contract.Requires(Contract.ForAll(others, x => x != null));
            Contract.Ensures(Contract.Result<FileSystemPath>() != null);

            string ret = this;
            foreach (var other in others)
            {
                ret = System.IO.Path.Combine(ret, other);
            }

            return new FileSystemPath(ret);
        }

        /// <summary>
        /// Converts this path into a directory object, which may enumerate its children or perform directory-related operations.
        /// </summary>
        /// <returns>A directory object.</returns>
        public System.IO.DirectoryInfo ToDirectoryInfo()
        {
            Contract.Ensures(Contract.Result<System.IO.DirectoryInfo>() != null);
            return new System.IO.DirectoryInfo(this.Path);
        }

        /// <summary>
        /// Converts this path into a file object, which may open the file or perform file-related operations.
        /// </summary>
        /// <returns>A file object.</returns>
        public System.IO.FileInfo ToFileInfo()
        {
            Contract.Ensures(Contract.Result<System.IO.FileInfo>() != null);
            return new System.IO.FileInfo(this.Path);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return this.Path;
        }
    }
}
