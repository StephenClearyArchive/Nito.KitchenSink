using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    public sealed class FtpDirectoryEntry
    {
        public AttributeFlags Attributes { get; set; }

        public ulong Size { get; set; }

        public string Name { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        public bool IsDirectory
        {
            get { return (this.Attributes & AttributeFlags.Directory) == AttributeFlags.Directory; }
        }

        [Flags]
        public enum AttributeFlags : uint
        {
            ReadOnly = 0x1,
            Hidden = 0x2,
            System = 0x4,
            Directory = 0x10,
        }
    }
}
