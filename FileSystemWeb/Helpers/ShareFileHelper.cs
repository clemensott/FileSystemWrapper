using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Exceptions;
using FileSystemWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Helpers
{
    public static class ShareFileHelper
    {
        public static async Task<InternalFile> GetFileItem(string virtualPath, AppDbContext dbContext, string userId,
            ControllerBase controller)
        {
            string[] parts = ConfigHelper.Public.SplitVirtualPath(virtualPath);
            if (!Guid.TryParse(parts[0], out Guid uuid))
            {
                throw (HttpResultException)controller.BadRequest("Can't parse uuid");
            }

            if (parts.Length == 1)
            {
                ShareFile shareFile = await dbContext.ShareFiles
                    .Include(f => f.Permission)
                    .FirstOrDefaultAsync(f => f.Uuid == uuid);

                if (shareFile == null || (shareFile.UserId != null && shareFile.UserId != userId))
                {
                    throw (HttpResultException)controller.NotFound("Share file not found.");
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

            if (folder == null || (folder.UserId != null && folder.UserId != userId))
            {
                throw (HttpResultException)controller.NotFound("Share folder not found.");
            }

            IEnumerable<string> allPhysicalPathParts = new string[] { folder.GetPath() }.Concat(parts[1..]);
            string physicalPath = FileHelper.ToFilePath(allPhysicalPathParts);
            if (!FileHelper.IsPathAllowed(physicalPath))
            {
                throw (HttpResultException)controller.BadRequest("Path is not fully qualified");
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
