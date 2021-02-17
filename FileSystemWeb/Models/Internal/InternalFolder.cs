using System;

namespace FileSystemWeb.Models.Internal
{
    public class InternalFolder
    {
        public string BaseName { get; set; }

        public string Name { get; set; }

        public string PhysicalPath { get; set; }

        public string VirtualPath { get; set; }

        public Guid? SharedId { get; set; }

        public FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission Permission { get; set; }
    }
}
