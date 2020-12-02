using FileSystemCommon.Models.FileSystem.Folders;

namespace FileSystemCommon.Models.FileSystem.Files
{
    public struct FileItem : IFileItem
    {
        public string Name { get; set; }

        public string Extension { get; set; }

        public string Path { get; set; }

        public FileItemPermission Permission { get; set; }
    }
}