namespace FileSystemWeb.Models
{
    public class FileItemPermission
    {
#nullable enable
        public int Id { get; set; }
        
        public bool Read { get; set; }
        
        public bool Info { get; set; }
        
        public bool Hash { get; set; }
        
        public bool Write { get; set; }
#nullable disable

        public FileSystemCommon.Models.FileSystem.Files.FileItemPermission ToFileItemPermission()
        {
            return new FileSystemCommon.Models.FileSystem.Files.FileItemPermission()
            {
                Read = Read,
                Info = Info,
                Hash = Hash,
                Write = Write,
            };
        }

        public FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission ToFolderItemPermission()
        {
            return new FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission()
            {
                Read = Read,
                List = false,
                Info = Info,
                Hash = Hash,
                Write = Write,
            };
        }

        public static FileItemPermission New(
            FileSystemCommon.Models.FileSystem.Files.FileItemPermission permission)
        {
            return new FolderItemPermission()
            {
                Read = permission.Read,
                Info = permission.Info,
                Hash = permission.Hash,
                Write = permission.Write,
            };
        }
    }
}