using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Internal;
using Microsoft.EntityFrameworkCore;
using StdOttStandard.Models.HttpExceptions;

namespace FileSystemWeb.Services.Share
{
    public class ShareFolderService
    {
        private readonly AppDbContext dbContext;

        public ShareFolderService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<InternalFolder> GetFolderItem(string virtualPath, string userId)
        {
            string[] parts = ConfigHelper.Public.SplitVirtualPath(virtualPath);
            if (!Guid.TryParse(parts[0], out Guid uuid))
            {
                throw new BadRequestHttpException("Can't parse uuid", code: 8005);
            }

            ShareFolder folder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (folder == null || (folder.UserId != null && folder.UserId != userId))
            {
                throw new NotFoundHttpException("Share folder not found.", code: 8006);
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
                throw new BadRequestHttpException("Path is not fully qualified", code: 8007);
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
