using System;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemWeb.Helpers;

namespace FileSystemWeb.Models
{
    public class ShareFolder
    {
#nullable enable
        public int Id { get; set; }

        public Guid Uuid { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string Path { get; set; }

        public bool IsListed { get; set; }

        public int PermissionId { get; set; }

        public string? UserId { get; set; }
#nullable disable

        public FolderItemPermission Permission { get; set; }

        public AppUser User { get; set; }

        public FolderItem ToFolderItem()
        {
            return new FolderItem()
            {
                Name = Name,
                Path = Uuid.ToString(),
                Permission = Permission.ToFolderItemPermission(),
            };
        }
    }
}