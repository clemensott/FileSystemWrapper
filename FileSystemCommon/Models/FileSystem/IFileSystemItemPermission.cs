namespace FileSystemCommon.Models.FileSystem
{
    public interface IFileSystemItemPermission
    {
        bool Read { get; set; }
        
        bool Info { get; set; }
        
        bool Hash { get; set; }
        
        bool Write { get; set; }
    }
}