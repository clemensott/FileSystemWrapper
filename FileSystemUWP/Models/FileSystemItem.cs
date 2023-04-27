using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using StdOttStandard.Linq;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemUWP.Models
{
    public struct FileSystemItem : ISortableFileSystemItem
    {
        public bool IsFile { get; }

        public bool IsFolder { get; }

        public string Name { get; }

        public string Extension { get; }

        public string FullPath { get; }

        public PathPart[] PathParts { get; }

        public IFileSystemItemPermission Permission { get; }

        public IReadOnlyList<string> SortKeys { get; set; }

        public FileSystemItem(bool isFile, string name, string extension, string fullPath,
            PathPart[] pathParts, IFileSystemItemPermission permission, IEnumerable<string> sortKeys) : this()
        {
            IsFile = isFile;
            IsFolder = !isFile;
            Name = name;
            Extension = extension;
            FullPath = fullPath;
            PathParts = pathParts;
            Permission = permission;
            SortKeys = sortKeys.ToNotNull().ToList().AsReadOnly();
        }

        public static FileSystemItem FromFile(FileSortItem file, PathPart[] parentPath)
        {
            PathPart[] pathParts = parentPath.GetChildPathParts(file).ToArray();
            return new FileSystemItem(true, file.Name, file.Extension, file.Path, pathParts, file.Permission, file.SortKeys);
        }

        public static FileSystemItem FromFolder(FolderSortItem folder, PathPart[] parentPath)
        {
            PathPart[] pathParts = parentPath.GetChildPathParts(folder).ToArray();
            return new FileSystemItem(false, folder.Name, null, folder.Path, pathParts, folder.Permission, folder.SortKeys);
        }

        public static FileSystemItem FromFolderContent(FolderContent folder)
        {
            return new FileSystemItem(false, folder.Path.ToName(), null,
                folder.Path.ToPath(), folder.Path, folder.Permission, folder.SortKeys);
        }
    }
}
