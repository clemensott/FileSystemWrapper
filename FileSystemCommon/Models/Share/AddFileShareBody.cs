using FileSystemCommon.Models.FileSystem.Files;

namespace FileSystemCommon.Models.Share
{
    public class AddFileShareBody
    {
        public string UserId { get; set; }
        
        // Virtual Path
        public string Path { get; set; }
        
        public string Name { get; set; }
        
        public bool IsListed { get; set; }
        
        public FileItemPermission Permission { get; set; }
    }
}