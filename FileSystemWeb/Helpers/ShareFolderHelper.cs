using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Exceptions;
using FileSystemWeb.Models.Internal;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Helpers
{
    public static class ShareFolderHelper
    {
        public static async Task<InternalFolder> GetFolderItem(string virtualPath, AppDbContext dbContext, string userId)
        {
            string[] parts = ConfigHelper.Public.SplitVirtualPath(virtualPath);
            if (!Guid.TryParse(parts[0], out Guid uuid))
            {
                throw new BadRequestException("Can't parse uuid.", 6001);
            }

            ShareFolder folder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (folder == null 
                || (folder.UserId != null && folder.UserId != userId)
                || (folder.ExpiresAt  != null && folder.ExpiresAt <= DateTime.UtcNow))
            {
                throw new NotFoundException("Share folder not found.", 6002);
            }

            Guid? sharedId = null;
            IEnumerable<string> allPhysicalPathParts = new string[] { folder.GetPath() }.Concat(parts[1..]);
            string physicalPath = FileHelper.ToPhysicalFolderPath(allPhysicalPathParts);
            var permission = folder.Permission.ToFolderItemPermission();

            if (physicalPath.Length == 0)
            {
                permission.Write = false;
                sharedId = folder.Uuid;
            }
            else if (!FileHelper.IsPathAllowed(physicalPath))
            {
                throw new BadRequestException("Path is not fully qualified", 6003);
            }

            return new InternalFolder()
            {
                BaseName = folder.Name,
                Name = parts.Length == 1 ?
                    folder.Name : Path.GetFileName(physicalPath.TrimEnd(ConfigHelper.Public.DirectorySeparatorChar)),
                PhysicalPath = physicalPath,
                VirtualPath = virtualPath,
                SharedId = sharedId,
                Permission = permission,
            };
        }
    }
}
