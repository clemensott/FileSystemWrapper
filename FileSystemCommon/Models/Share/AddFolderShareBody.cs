using FileSystemCommon.Models.FileSystem.Folders;

namespace FileSystemCommon.Models.Share
{
    public class AddFolderShareBody : EditFileSystemItemShareBody
    {
        // Virtual Path
        public string Path { get; set; }

        public FolderItemPermission Permission { get; set; }
    }
}