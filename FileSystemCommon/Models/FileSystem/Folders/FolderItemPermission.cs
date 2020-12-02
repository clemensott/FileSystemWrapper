namespace FileSystemCommon.Models.FileSystem.Folders
{
    public struct FolderItemPermission : IFileSystemItemPermission
    {
        public bool Read { get; set; }

        public bool List { get; set; }

        public bool Info { get; set; }

        public bool Hash { get; set; }

        public bool Write { get; set; }
    }
}