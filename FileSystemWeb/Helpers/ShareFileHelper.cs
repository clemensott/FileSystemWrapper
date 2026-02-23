using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Helpers
{
    public static class ShareFileHelper
    {
        public static async Task<InternalFile> GetFileItem(string virtualPath, AppDbContext dbContext, string userId)
        {
            string[] parts = ConfigHelper.Public.SplitVirtualPath(virtualPath);
            if (!Guid.TryParse(parts[0], out Guid uuid))
            {
                throw new BadRequestException("Can't parse uuid", 4001);
            }

            if (parts.Length == 1)
            {
                ShareFile shareFile = await dbContext.ShareFiles
                    .Include(f => f.Permission)
                    .FirstOrDefaultAsync(f => f.Uuid == uuid);

                if (shareFile == null
                    || (shareFile.UserId != null && shareFile.UserId != userId)
                    || (shareFile.ExpiresAt  != null && shareFile.ExpiresAt <= DateTime.UtcNow))
                {
                    throw new NotFoundException("Share file not found.", 4002);
                }

                return new InternalFile()
                {
                    PhysicalPath = shareFile.Path,
                    VirtualPath = virtualPath,
                    Name = shareFile.Name,
                    SharedId = shareFile.Uuid,
                    Permission = shareFile.Permission.ToFileItemPermission(),
                };
            }

            ShareFolder folder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);

            if (folder == null 
                || (folder.UserId != null && folder.UserId != userId)
                || (folder.ExpiresAt  != null && folder.ExpiresAt <= DateTime.UtcNow))
            {
                throw new NotFoundException("Share folder not found.", 4003);
            }

            IEnumerable<string> allPhysicalPathParts = new string[] { folder.GetPath() }.Concat(parts[1..]);
            string physicalPath = FileHelper.ToFilePath(allPhysicalPathParts);
            if (!FileHelper.IsPathAllowed(physicalPath))
            {
                throw new BadRequestException("Path is not fully qualified.", 4004);
            }

            return new InternalFile()
            {
                PhysicalPath = physicalPath,
                VirtualPath = virtualPath,
                Name = Path.GetFileName(physicalPath),
                SharedId = null,
                Permission = folder.Permission.ToFileItemPermission(),
            };
        }
    }
}
