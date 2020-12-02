namespace FileSystemWeb.Models
{
    public class InternalFolder
    {
        public string BaseName { get; set; }

        public string PhysicalPath { get; set; }

        public string VirtualPath { get; set; }

        public FolderItemPermission Permission { get; set; }
    }
}