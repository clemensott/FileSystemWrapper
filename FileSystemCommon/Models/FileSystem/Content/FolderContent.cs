using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;

namespace FileSystemCommon.Models.FileSystem.Content
{
    public class FolderContent : IFileSystemSortItem
    {
        public PathPart[] Path { get; set; } = new PathPart[0];

        public FolderItemPermission Permission { get; set; }

        public string[] SortKeys { get; set; }

        public FolderSortItem[] Folders { get; set; } = new FolderSortItem[0];

        public FileSortItem[] Files { get; set; } = new FileSortItem[0];
    }
}