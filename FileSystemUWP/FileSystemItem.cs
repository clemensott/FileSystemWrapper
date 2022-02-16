using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using System.Linq;

namespace FileSystemUWP
{
    public struct FileSystemItem
    {
        public bool IsFile { get; }

        public bool IsFolder { get; }

        public string Name { get; }

        public string Extension { get; }

        public string FullPath { get; }

        public PathPart[] PathParts { get; }

        public IFileSystemItemPermission Permission { get; }

        public FileSystemItem(bool isFile, string name, string extension, string fullPath,
            PathPart[] pathParts, IFileSystemItemPermission permission) : this()
        {
            IsFile = isFile;
            IsFolder = !isFile;
            Name = name;
            Extension = extension;
            FullPath = fullPath;
            PathParts = pathParts;
            Permission = permission;
        }

        public static FileSystemItem FromFile(FileItem file, PathPart[] parentPath)
        {
            PathPart[] pathParts = parentPath.GetChildPathParts(file).ToArray();
            return new FileSystemItem(true, file.Name, file.Extension, file.Path, pathParts, file.Permission);
        }

        public static FileSystemItem FromFolder(FolderItem folder, PathPart[] parentPath)
        {
            PathPart[] pathParts = parentPath.GetChildPathParts(folder).ToArray();
            return new FileSystemItem(false, folder.Name, null, folder.Path, pathParts, folder.Permission);
        }

        public static FileSystemItem FromFolderContent(FolderContent folder)
        {
            return new FileSystemItem(false, folder.Path.LastOrDefault().Name, null,
                folder.Path.ToPath(), folder.Path, folder.Permission);
        }
    }
}
