namespace FileSystemCommon.Models.FileSystem.Folders
{
    public interface IFolderItem : IFileSystemItem
    {
        new FolderItemPermission Permission { get; }

        bool Deletable { get; }
    }
}
