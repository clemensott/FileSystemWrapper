using FileSystemCommon.Models.FileSystem.Content;
using System;

namespace FileSystemCommon.Models.FileSystem.Folders
{
    public struct FolderSortItem : IFolderItem, IFileSystemSortItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public Guid? SharedId { get; set; }

        public FolderItemPermission Permission { get; set; }

        IFileSystemItemPermission IFileSystemItem.Permission
        {
            get => Permission;
            set => Permission = (FolderItemPermission)value;
        }

        public bool Deletable { get; set; }

        public string[] SortKeys { get; set; }

        public static FolderSortItem FromItem(FolderItem folder, params string[] keys)
        {
            return new FolderSortItem()
            {
                Name = folder.Name,
                Path = folder.Path,
                SharedId = folder.SharedId,
                Permission = folder.Permission,
                Deletable = folder.Deletable,
                SortKeys = keys,
            };
        }
    }
}