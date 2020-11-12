using System;
using System.IO;

namespace FileSystemCommon.Models.FileSystem
{
    public struct FolderItemInfo
    {
        public string Name { get; set; }

        public string FullPath { get; set; }

        public int FileCount { get; set; }

        public long Size { get; set; }

        public DateTime LastWriteTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime CreationTime { get; set; }

        public FileAttributes Attributes { get; set; }
    }
}
