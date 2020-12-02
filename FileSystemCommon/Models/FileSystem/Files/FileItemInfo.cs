using System;
using System.IO;

namespace FileSystemCommon.Models.FileSystem.Files
{
    public struct FileItemInfo : IFileItem
    {
        public string Name { get; set; }

        public string Extension { get; set; }

        public string Path { get; set; }

        public FileItemPermission Permission { get; set; }

        public long Size { get; set; }

        public DateTime LastWriteTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime CreationTime { get; set; }

        public FileAttributes Attributes { get; set; }
    }
}