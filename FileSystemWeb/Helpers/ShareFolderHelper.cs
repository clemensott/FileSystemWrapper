using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemWeb.Data;
using FileSystemWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Helpers
{
    public static class ShareFolderHelper
    {
        public static async Task<InternalFolder> GetFolderItem(string virtualPath, AppDbContext dbContext,
            string userId)
        {
            string[] parts = virtualPath.Split('\\');
            if (!Guid.TryParse(parts[0], out Guid uuid)) return null;
            ShareFolder folder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);

            if (folder == null || (folder.UserId != null && folder.UserId != userId)) return null;

            IEnumerable<string> allPhysicalPathParts = new string[] {folder.Path}.Concat(parts[1..]);
            return new InternalFolder()
            {
                BaseName = folder.Name,
                PhysicalPath = FileHelper.ToFolderPath(allPhysicalPathParts.ToArray()),
                VirtualPath = virtualPath,
                Permission = folder.Permission,
            };
        }
    }
}