using System;
using FileSystemCommon.Models.FileSystem.Folders;

namespace FileSystemCommon.Models.Share
{
    public struct ShareItem
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public bool IsListed { get; set; }
        
        public bool IsFile { get; set; }

        public FolderItemPermission Permission { get; set; }
    }
}