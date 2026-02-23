using System;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.Share;

namespace FileSystemWeb.Models
{
    public class ShareFile
    {
#nullable enable
        public int Id { get; set; }

        public Guid Uuid { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public bool IsListed { get; set; }

        public int PermissionId { get; set; }

        public string? UserId { get; set; }

        public DateTime? ExpiresAt { get; set; }
#nullable disable

        public FileItemPermission Permission { get; set; }

        public AppUser User { get; set; }

        public FileItem ToFileItem()
        {
            return new FileItem()
            {
                Name = Name,
                Extension = System.IO.Path.GetExtension(Name),
                Path = Uuid.ToString(),
                SharedId = Uuid,
                Permission = Permission.ToFileItemPermission(),
            };
        }

        public ShareItem ToShareItem(bool exists)
        {
            return new ShareItem()
            {
                Id = Uuid,
                Name = Name,
                IsListed = IsListed,
                Exists = exists,
                UserId = UserId,
                IsFile = true,
                Permission = Permission.ToFolderItemPermission(),
                ExpiresAt = ExpiresAt,
            };
        }
    }
}