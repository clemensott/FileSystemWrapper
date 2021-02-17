namespace FileSystemWeb.Models
{
    public class FolderItemPermission : FileItemPermission
    {
#nullable enable
        public bool List { get; set; }
#nullable disable

        public FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission ToFolderItemPermission()
        {
            return new FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission()
            {
                Read = Read,
                List = List,
                Info = Info,
                Hash = Hash,
                Write = Write,
            };
        }

        public static FolderItemPermission New(
            FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission permission)
        {
            return new FolderItemPermission()
            {
                Read = permission.Read,
                List = permission.List,
                Info = permission.Info,
                Hash = permission.Hash,
                Write = permission.Write,
            };
        }
    }
}
