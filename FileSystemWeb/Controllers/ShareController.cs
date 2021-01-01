using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommon.Models.Share;
using FileSystemWeb.Data;
using FileSystemWeb.Exceptions;
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
            InternalFile file;
            try
            {
                file = await ValidateAddFileShare(body);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
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

        [HttpGet("file/{uuid}")]
        public async Task<ActionResult<ShareItem>> GetShareFile(Guid uuid)
        {
            ShareFile shareFile = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFile == null) return NotFound("Share file not found");

            return shareFile.ToShareItem();
        }

        [HttpPut("file/{uuid}")]
        public async Task<ActionResult<FileItem>> EditShareFile(Guid uuid, [FromBody] EditFileSystemItemShareBody body)
        {
            try
            {
                await ValidateFileSystemItemShare(body);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (await dbContext.ShareFiles.AnyAsync(f =>
                f.Uuid != uuid && f.Name == body.Name && f.UserId == body.UserId))
            {
                return BadRequest("File with this name is already shared");
            }

            ShareFile shareFile = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFile == null) return NotFound("Share file not found");

            shareFile.Name = body.Name;
            shareFile.IsListed = body.IsListed;
            shareFile.UserId = string.IsNullOrWhiteSpace(body.UserId) ? null : body.UserId;

            await dbContext.SaveChangesAsync();

            return shareFile.ToFileItem();
        }

        private async Task<InternalFile> ValidateAddFileShare(AddFileShareBody body)
        {
            await ValidateFileSystemItemShare(body);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(body.Path, dbContext, userId);

            if (file == null) throw (HttpResultException) NotFound("Base not found");
            if (!HasPermission(file.Permission, body.Permission)) throw (HttpResultException) Forbid();
            if (!System.IO.File.Exists(file.PhysicalPath)) throw (HttpResultException) NotFound("File not found");

            return file;
        }

        private async Task ValidateFileSystemItemShare(EditFileSystemItemShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) throw (HttpResultException) BadRequest("Name missing");

            if (!string.IsNullOrWhiteSpace(body.UserId) && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                throw (HttpResultException) BadRequest("User not found");
            }
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

        [HttpDelete("file/{uuid}")]
        public async Task<ActionResult> DeleteShareFile(Guid uuid)
        {
            ShareFile shareFile = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFile == null) return NotFound("Share file not found");

            dbContext.ShareFiles.Remove(shareFile);
            await dbContext.SaveChangesAsync();

            return Ok();
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

            if (await dbContext.ShareFolders.AnyAsync(f => f.Name == body.Name && f.UserId == body.UserId))
            {
                return BadRequest("Folder with this name is already shared");
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
            await dbContext.SaveChangesAsync();

            return shareFolder.ToFolderItem();
        }

        [HttpGet("folder/{uuid}")]
        public async Task<ActionResult<ShareItem>> GetShareFolder(Guid uuid)
        {
            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFolder == null) return NotFound("Share folder not found");

            return shareFolder.ToShareItem();
        }
        
        [HttpPut("folder/{uuid}")]
        public async Task<ActionResult<FolderItem>> EditFolderShare(Guid uuid, [FromBody] EditFileSystemItemShareBody body)
        {
            try
            {
                await ValidateFileSystemItemShare(body);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }
        
            if (await dbContext.ShareFolders.AnyAsync(f =>
                f.Uuid != uuid && f.Name == body.Name && f.UserId == body.UserId))
            {
                return BadRequest("Folder with this name is already shared");
            }
        
            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFolder == null) return NotFound("Share folder not found");
        
            shareFolder.Name = body.Name;
            shareFolder.IsListed = body.IsListed;
            shareFolder.UserId = body.UserId;
        
            await dbContext.SaveChangesAsync();
        
            return shareFolder.ToFolderItem();
        }

        private async Task<InternalFolder> ValidateAddFolderShare(AddFolderShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) throw (HttpResultException) BadRequest("Name missing");

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFolder folder = await ShareFolderHelper.GetFolderItem(body.Path, dbContext, userId);

            if (folder == null) throw (HttpResultException) NotFound("Base not found");
            if (!HasPermission(folder.Permission, body.Permission)) throw (HttpResultException) Forbid();
            if (!string.IsNullOrWhiteSpace(folder.PhysicalPath) && !System.IO.Directory.Exists(folder.PhysicalPath))
            {
                throw (HttpResultException) NotFound("Folder not found");
            }

            if (body.UserId != null && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                throw (HttpResultException) BadRequest("User not found");
            }

            return folder;
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

        [HttpDelete("folder/{uuid}")]
        public async Task<ActionResult> DeleteShareFolder(Guid uuid)
        {
            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFolder == null) return NotFound("Share folder not found");

            dbContext.ShareFolders.Remove(shareFolder);
            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}