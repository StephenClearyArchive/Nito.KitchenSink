// <copyright file="FileSystemPath.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

// TODO: P/Invoke for GetShortPathName / GetLongPathName

namespace Nito.KitchenSink
{
    using System;

    /// <summary>
    /// A string that is a file system path.
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
            if (path == null)
            {
                throw new ArgumentNullException("value", "The path used to construct a FileSystemPath may not be null.");
            }

            this.Path = path;
        }

        /// <summary>
        /// Gets random name that can be used as a folder or file name.
        /// </summary>
        public static FileSystemPath RandomFileName
        {
            get
            {
                return System.IO.Path.GetRandomFileName();
            }
        }

        /// <summary>
        /// Gets the path of a temporary file that has been created.
        /// </summary>
        public static FileSystemPath TempFile
        {
            get
            {
                return System.IO.Path.GetTempFileName();
            }
        }

        /// <summary>
        /// Gets the path of the temporary directory.
        /// </summary>
        public static FileSystemPath TempPath
        {
            get
            {
                return System.IO.Path.GetTempPath();
            }
        }

        /// <summary>
        /// Gets the actual path string. This is never <c>null</c>, but may be empty.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the root portion of this path. Returns an empty path if this path does not contain a root portion.
        /// </summary>
        public FileSystemPath Root
        {
            get
            {
                return System.IO.Path.GetPathRoot(this);
            }
        }

        /// <summary>
        /// Gets the directory portion of this path. Returns an empty path if this path does not contain a directory portion.
        /// </summary>
        public FileSystemPath Directory
        {
            get
            {
                // Path.GetDirectoryName will return null if this path is a root directory
                string ret = System.IO.Path.GetDirectoryName(this);
                if (ret == null)
                {
                    return string.Empty;
                }

                return ret;
            }
        }

        /// <summary>
        /// Gets the file name portion of this path. Returns an empty path if this path does not contain a file name portion.
        /// </summary>
        public FileSystemPath FileName
        {
            get
            {
                return System.IO.Path.GetFileName(this);
            }
        }

        /// <summary>
        /// Gets the file name portion of this path, stripping the extension. Returns an empty path if this path does not contain a file name portion.
        /// </summary>
        public FileSystemPath FileNameWithoutExtension
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(this);
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

        /// <summary>
        /// Performs an implicit conversion from <see cref="FileSystemPath"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="source">The source path.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(FileSystemPath source)
        {
            return source.Path;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="FileSystemPath"/>.
        /// </summary>
        /// <param name="source">The source path.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator FileSystemPath(string source)
        {
            return new FileSystemPath(source);
        }

        /// <summary>
        /// Changes the extension of the file name portion of this path. Returns an empty string if this path does not contain a file name portion.
        /// </summary>
        /// <param name="extension">The new extension, with or without the ".". May be <c>null</c> to remove an existing extension.</param>
        /// <returns>A path with a changed extension.</returns>
        public FileSystemPath ChangeExtension(string extension)
        {
            return System.IO.Path.ChangeExtension(this.Path, extension);
        }

        /// <summary>
        /// Combines two or more paths. Absolute paths remove previous path information.
        /// </summary>
        /// <param name="others">The other paths to combine with this one.</param>
        /// <returns>The combined path.</returns>
        public FileSystemPath Combine(params FileSystemPath[] others)
        {
            string ret = this;
            foreach (var other in others)
            {
                ret = System.IO.Path.Combine(ret, other);
            }

            return ret;
        }

        /// <summary>
        /// Converts this path into an absolute path, using the current directory if necessary. This path may not be an empty path.
        /// </summary>
        /// <returns>An absolute path.</returns>
        public FileSystemPath ToAbsolute()
        {
            return System.IO.Path.GetFullPath(this);
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
