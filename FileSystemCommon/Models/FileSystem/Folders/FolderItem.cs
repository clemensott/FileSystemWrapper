namespace FileSystemCommon.Models.FileSystem.Folders
{
    public struct FolderItem : IFolderItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public FolderItemPermission Permission { get; set; }
    }
}