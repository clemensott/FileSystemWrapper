namespace FileSystemCommon.Models.FileSystem.Folders
{
    public interface IFolderItem : IPathItem
    {
        FolderItemPermission Permission { get; }
        
        bool Deletable { get; }
    }
}