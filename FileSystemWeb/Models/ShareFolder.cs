using System;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommon.Models.Share;

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
                SharedId = Uuid,
                Permission = Permission.ToFolderItemPermission(),
                Deletable = false,
            };
        }

        public ShareItem ToShareItem()
        {
            return new ShareItem()
            {
                Id = Uuid,
                Name = Name,
                IsListed = IsListed,
                UserId = UserId,
                IsFile = false,
                Permission = Permission.ToFolderItemPermission(),
            };
        }
    }
}