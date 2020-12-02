using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemWeb.Data;
using FileSystemWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Helpers
{
    public static class ShareFileHelper
    {
        public static async Task<InternalFile> GetFileItem(string virtualPath, AppDbContext dbContext, string userId)
        {
            string[] parts = virtualPath.Split('\\');
            if (parts.Length == 1)
            {
                if (!Guid.TryParse(parts[0], out Guid fileUuid)) return null;
                ShareFile shareFile = await dbContext.ShareFiles
                    .Include(f => f.Permission)
                    .FirstOrDefaultAsync(f => f.Uuid == fileUuid);

                return shareFile != null
                    ? new InternalFile()
                    {
                        PhysicalPath = shareFile.Path,
                        VirtualPath = virtualPath,
                        Permission = shareFile.Permission,
                    }
                    : null;
            }

            if (!Guid.TryParse(parts[0], out Guid folderUuid)) return null;
            ShareFolder folder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == folderUuid);

            IEnumerable<string> allPhysicalPathParts = new string[] {folder?.Path}.Concat(parts[1..]);
            return folder != null
                ? new InternalFile()
                {
                    PhysicalPath = FileHelper.ToFilePath(allPhysicalPathParts.ToArray()),
                    VirtualPath = virtualPath,
                    Permission = folder.Permission,
                }
                : null;
        }
    }
}