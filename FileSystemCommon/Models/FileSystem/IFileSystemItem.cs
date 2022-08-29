namespace FileSystemCommon.Models.FileSystem
{
    public interface IFileSystemItem : IPathItem
    {
        IFileSystemItemPermission Permission { get; set; }
    }
}
