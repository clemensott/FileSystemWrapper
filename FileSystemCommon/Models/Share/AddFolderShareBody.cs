using FileSystemCommon.Models.FileSystem.Folders;

namespace FileSystemCommon.Models.Share
{
    public class AddFolderShareBody
    {
        public string UserId { get; set; }
        
        // Virtual Path
        public string Path { get; set; }
        
        public string Name { get; set; }
        
        public bool IsListed { get; set; }
        
        public FolderItemPermission Permission { get; set; }
    }
}