namespace FileSystemWeb.Models
{
    public class InternalFile
    {
        public string PhysicalPath { get; set; }

        public string VirtualPath { get; set; }

        public FileItemPermission Permission { get; set; }
    }
}