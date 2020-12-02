namespace FileSystemCommon.Models.FileSystem.Files
{
    public interface IFileItem : IPathItem
    {
        string Extension { get; }

        FileItemPermission Permission { get; }
    }
}