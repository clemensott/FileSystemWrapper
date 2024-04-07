using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using Microsoft.EntityFrameworkCore;
using StdOttStandard.Models.HttpExceptions;

namespace FileSystemWeb.Services.Share
{
    public class ShareFileService
    {
        private readonly AppDbContext dbContext;

        public ShareFileService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<InternalFile> GetFileItem(string virtualPath, string userId)
        {
            string[] parts = ConfigHelper.Public.SplitVirtualPath(virtualPath);
            if (!Guid.TryParse(parts[0], out Guid uuid))
            {
                throw new BadRequestHttpException("Can't parse uuid", code: 8001);
            }

            if (parts.Length == 1)
            {
                ShareFile shareFile = await dbContext.ShareFiles
                    .Include(f => f.Permission)
                    .FirstOrDefaultAsync(f => f.Uuid == uuid);

                if (shareFile == null || (shareFile.UserId != null && shareFile.UserId != userId))
                {
                    throw new NotFoundHttpException("Share file not found.", code: 8002);
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
                throw new NotFoundHttpException("Share file not found.", code: 8003);
            }

            IEnumerable<string> allPhysicalPathParts = new string[] { folder.GetPath() }.Concat(parts[1..]);
            string physicalPath = FileHelper.ToFilePath(allPhysicalPathParts);
            if (!FileHelper.IsPathAllowed(physicalPath))
            {
                throw new BadRequestHttpException("Path is not fully qualified", code: 8004);
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
