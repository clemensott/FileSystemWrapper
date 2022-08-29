using System;

namespace FileSystemCommon.Models.FileSystem.Files
{
    public struct FileItem : IFileItem
    {
        public string Name { get; set; }

        public string Extension { get; set; }

        public string Path { get; set; }

        public Guid? SharedId { get; set; }

        public FileItemPermission Permission { get; set; }

        IFileSystemItemPermission IFileSystemItem.Permission
        {
            get => Permission;
            set => Permission = (FileItemPermission)value;
        }
    }
}
