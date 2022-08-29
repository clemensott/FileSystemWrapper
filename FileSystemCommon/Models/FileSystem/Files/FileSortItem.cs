using FileSystemCommon.Models.FileSystem.Content;
using System;

namespace FileSystemCommon.Models.FileSystem.Files
{
    public struct FileSortItem : IFileItem, IFileSystemSortItem
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

        public string[] SortKeys { get; set; }

        public static FileSortItem FromItem(FileItem file, params string[] keys)
        {
            return new FileSortItem()
            {
                Name = file.Name,
                Extension = file.Extension,
                Path = file.Path,
                SharedId = file.SharedId,
                Permission = file.Permission,
                SortKeys = keys,
            };
        }
    }
}
