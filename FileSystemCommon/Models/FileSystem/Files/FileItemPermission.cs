using FileSystemCommon.Models.FileSystem.Folders;

namespace FileSystemCommon.Models.FileSystem.Files
{
    public struct FileItemPermission : IFileSystemItemPermission
    {
        public bool Read { get; set; }

        public bool Info { get; set; }

        public bool Hash { get; set; }

        public bool Write { get; set; }

        public static explicit operator FileItemPermission(FolderItemPermission permission)
        {
            return new FileItemPermission()
            {
                Read = permission.Read,
                Info = permission.Info,
                Hash = permission.Hash,
                Write = permission.Write,
            };
        }
    }
}