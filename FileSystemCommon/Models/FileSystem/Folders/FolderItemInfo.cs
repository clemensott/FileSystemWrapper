using System;
using System.IO;

namespace FileSystemCommon.Models.FileSystem.Folders
{
    public struct FolderItemInfo : IFolderItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public Guid? SharedId { get; set; }

        public FolderItemPermission Permission { get; set; }

        public bool Deletable { get; set; }

        public int FileCount { get; set; }

        public long Size { get; set; }

        public DateTime LastWriteTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime CreationTime { get; set; }

        public FileAttributes Attributes { get; set; }
    }
}