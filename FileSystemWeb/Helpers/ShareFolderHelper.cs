using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Exceptions;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Helpers
{
    public static class ShareFolderHelper
    {
        public static async Task<InternalFolder> GetFolderItem(string virtualPath, AppDbContext dbContext,
            string userId, ControllerBase controller)
        {
            string[] parts = ConfigHelper.Public.SplitVirtualPath(virtualPath);
            if (!Guid.TryParse(parts[0], out Guid uuid))
            {
                throw (HttpResultException)controller.BadRequest("Can't parse uuid");
            }

            ShareFolder folder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (folder == null || (folder.UserId != null && folder.UserId != userId))
            {
                throw (HttpResultException)controller.NotFound("Share folder not found.");
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
                throw (HttpResultException)controller.BadRequest("Path is not fully qualified");
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
