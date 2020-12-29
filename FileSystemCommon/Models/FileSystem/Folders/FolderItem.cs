using System;

namespace FileSystemCommon.Models.FileSystem.Folders
{
    public struct FolderItem : IFolderItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public Guid? SharedId { get; set; }

        public FolderItemPermission Permission { get; set; }
    }
}