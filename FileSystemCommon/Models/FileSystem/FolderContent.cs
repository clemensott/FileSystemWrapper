using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;

namespace FileSystemCommon.Models.FileSystem
{
    public class FolderContent
    {
        public PathPart[] Path { get; set; } = new PathPart[0];

        public FolderItemPermission Permission { get; set; }

        public FolderItem[] Folders { get; set; } = new FolderItem[0];

        public FileItem[] Files { get; set; } = new FileItem[0];
    }
}