namespace FileSystemCommon.Models.FileSystem.Files
{
    public interface IFileItem : IFileSystemItem
    {
        string Extension { get; }
        
        new FileItemPermission Permission { get; }
    }
}