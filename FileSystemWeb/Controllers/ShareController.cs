using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommon.Models.Share;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ShareController : Controller
    {
        private readonly AppDbContext dbContext;

        public ShareController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("file")]
        public async Task<ActionResult<FileItem>> AddShareFile([FromBody] AddFileShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) return BadRequest("Name missing");

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(body.Path, dbContext, userId);

            if (file == null) return NotFound("Base not found");
            if (!HasPermission(file.Permission, body.Permission)) return Forbid();
            if (!System.IO.File.Exists(file.PhysicalPath)) return NotFound("File not found");

            if (!string.IsNullOrWhiteSpace(body.UserId) && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                return BadRequest("User not found");
            }

            if (await dbContext.ShareFiles.AnyAsync(f => f.Name == body.Name && f.UserId == body.UserId))
            {
                return BadRequest("File with this name is already shared");
            }

            ShareFile shareFile = new ShareFile()
            {
                Name = body.Name,
                Path = file.PhysicalPath,
                IsListed = body.IsListed,
                UserId = string.IsNullOrWhiteSpace(body.UserId) ? null : body.UserId,
                Permission = Models.FileItemPermission.New(body.Permission),
            };

            await dbContext.ShareFiles.AddAsync(shareFile);
            await dbContext.SaveChangesAsync();

            return shareFile.ToFileItem();
        }

        private static bool HasPermission(Models.FileItemPermission hasPermission,
            FileSystemCommon.Models.FileSystem.Files.FileItemPermission givePermission)
        {
            return
                (hasPermission.Info && givePermission.Info) &&
                (hasPermission.Hash || !givePermission.Hash) &&
                (hasPermission.Read || !givePermission.Read) &&
                (hasPermission.Write || !givePermission.Write);
        }

        [HttpPost("folder")]
        public async Task<ActionResult<FolderItem>> AddFolderShare([FromBody] AddFolderShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) return BadRequest("Name missing");

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFolder folder = await ShareFolderHelper.GetFolderItem(body.Path, dbContext, userId);

            if (folder == null) return NotFound("Base not found");
            if (!HasPermission(folder.Permission, body.Permission)) return Forbid();
            if (!string.IsNullOrWhiteSpace(folder.PhysicalPath) && !System.IO.Directory.Exists(folder.PhysicalPath))
            {
                return NotFound("Folder not found");
            }

            if (body.UserId != null && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                return BadRequest("User not found");
            }

            if (await dbContext.ShareFiles.AnyAsync(f => f.Name == body.Name && f.UserId == body.UserId))
            {
                return BadRequest("File with this name is already shared");
            }

            ShareFolder shareFolder = new ShareFolder()
            {
                Name = body.Name,
                Path = folder.PhysicalPath,
                IsListed = body.IsListed,
                UserId = body.UserId,
                Permission = Models.FolderItemPermission.New(body.Permission),
            };

            await dbContext.ShareFolders.AddAsync(shareFolder);
            return shareFolder.ToFolderItem();
        }

        private static bool HasPermission(Models.FolderItemPermission hasPermission,
            FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission givePermission)
        {
            return
                (hasPermission.Info && givePermission.Info) &&
                (hasPermission.List || !givePermission.List) &&
                (hasPermission.Hash || !givePermission.Hash) &&
                (hasPermission.Read || !givePermission.Read) &&
                (hasPermission.Write || !givePermission.Write);
        }

        [HttpPost("firstFolder")]
        public async Task<ActionResult<FolderItem>> AddFirstFolderShare([FromBody] AddFolderShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) return BadRequest("Name missing");

            if (body.UserId != null && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                return BadRequest("User not found");
            }

            if (await dbContext.ShareFiles.AnyAsync(f => f.Name == body.Name && f.UserId == body.UserId))
            {
                return BadRequest("File with this name is already shared");
            }

            ShareFolder shareFolder = new ShareFolder()
            {
                Name = body.Name,
                Path = body.Path,
                IsListed = body.IsListed,
                UserId = body.UserId,
                Permission = Models.FolderItemPermission.New(body.Permission),
            };

            await dbContext.ShareFolders.AddAsync(shareFolder);
            await dbContext.SaveChangesAsync();
            return shareFolder.ToFolderItem();
        }
    }
}