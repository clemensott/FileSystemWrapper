using FileSystemCommon.Models.FileSystem.Files;

namespace FileSystemCommon.Models.Share
{
    public class AddFileShareBody : EditFileSystemItemShareBody
    {
        // Virtual Path
        public string Path { get; set; }

        public FileItemPermission Permission { get; set; }
    }
}