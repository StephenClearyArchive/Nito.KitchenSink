// <copyright file="FtpDirectoryEntry.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;

    /// <summary>
    /// An entry in an FTP directory listing.
    /// </summary>
    public sealed class FtpDirectoryEntry
    {
        /// <summary>
        /// The common values for <see cref="Attributes"/>; note that other values than these may be present.
        /// </summary>
        [Flags]
        public enum AttributeFlags : int
        {
            /// <summary>
            /// The file or subdirectory is read-only.
            /// </summary>
            ReadOnly = 0x1,

            /// <summary>
            /// The file or subdirectory is hidden.
            /// </summary>
            Hidden = 0x2,

            /// <summary>
            /// The file or subdirectory is a system file.
            /// </summary>
            System = 0x4,

            /// <summary>
            /// The entry is a subdirectory.
            /// </summary>
            Directory = 0x10,
        }

        /// <summary>
        /// Gets or sets the basic attributes of the file or subdirectory.
        /// </summary>
        public AttributeFlags Attributes { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the name of the file or subdirectory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the time that the file or subdirectory was created, if known.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the time that the file or subdirectory was last accessed, if known.
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Gets or sets the time that the file or subdirectory was last written, if known.
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// Gets a value indicating whether this entry is a subdirectory.
        /// </summary>
        public bool IsDirectory
        {
            get { return (this.Attributes & AttributeFlags.Directory) == AttributeFlags.Directory; }
        }
    }
}
