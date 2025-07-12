using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommon.Models.Share;
using FileSystemWeb.Constants;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Exceptions;
using FileSystemWeb.Models.Internal;
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
        [Authorize(Policy = Permissions.Share.PostShareFile)]
        public async Task<ActionResult<FileItem>> AddShareFile([FromBody] AddFileShareBody body)
        {
            InternalFile file = await ValidateAddFileShare(body);

            if (await dbContext.ShareFiles.AnyAsync(f => f.Name == body.Name && f.UserId == body.UserId))
            {
                throw new BadRequestException("File with this name is already shared.", 8001);
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

        [HttpGet("files")]
        [Authorize(Policy = Permissions.Share.GetShareFiles)]
        public async Task<ActionResult<IEnumerable<ShareItem>>> GetShareFiles()
        {
            ShareFile[] shareFiles = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .ToArrayAsync();

            return shareFiles.Select(f => f.ToShareItem(f.GetExists())).ToArray();
        }

        [HttpGet("file/{uuid}")]
        [Authorize(Policy = Permissions.Share.GetShareFile)]
        public async Task<ActionResult<ShareItem>> GetShareFile(Guid uuid)
        {
            ShareFile shareFile = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFile == null) throw new NotFoundException("Share file not found.", 8002);

            return shareFile.ToShareItem(shareFile.GetExists());
        }

        [HttpPut("file/{uuid}")]
        [Authorize(Policy = Permissions.Share.PutShareFile)]
        public async Task<ActionResult<FileItem>> EditShareFile(Guid uuid, [FromBody] EditFileSystemItemShareBody body)
        {
            await ValidateFileSystemItemShare(body);

            if (await dbContext.ShareFiles.AnyAsync(f =>
                f.Uuid != uuid && f.Name == body.Name && f.UserId == body.UserId))
            {
                throw new BadRequestException("File with this name is already shared.", 8003);
            }

            ShareFile shareFile = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFile == null) throw new NotFoundException("Share file not found.", 8004);

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

            if (!HasPermission(file.Permission, body.Permission)) throw new ForbiddenException("Needed permissions are missing.", 8001);
            if (!System.IO.File.Exists(file.PhysicalPath)) throw new NotFoundException("File not found.", 8018);

            return file;
        }

        private async Task ValidateFileSystemItemShare(EditFileSystemItemShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) throw new BadRequestException("Name missing.", 8019);

            if (!string.IsNullOrWhiteSpace(body.UserId) && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                throw new BadRequestException("User not found.", 8020);
            }
        }

        private static bool HasPermission(FileSystemCommon.Models.FileSystem.Files.FileItemPermission hasPermission,
            FileSystemCommon.Models.FileSystem.Files.FileItemPermission givePermission)
        {
            return
                (hasPermission.Info && givePermission.Info) &&
                (hasPermission.Hash || !givePermission.Hash) &&
                (hasPermission.Read || !givePermission.Read) &&
                (hasPermission.Write || !givePermission.Write);
        }

        [HttpDelete("file/{uuid}")]
        [Authorize(Policy = Permissions.Share.DeleteShareFile)]
        public async Task<ActionResult> DeleteShareFile(Guid uuid)
        {
            ShareFile shareFile = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFile == null) throw new NotFoundException("Share file not found.", 8005);

            dbContext.ShareFiles.Remove(shareFile);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("folder")]
        [Authorize(Policy = Permissions.Share.PostShareFolder)]
        public async Task<ActionResult<FolderItem>> AddFolderShare([FromBody] AddFolderShareBody body)
        {
            InternalFolder folder = await ValidateAddFolderShare(body);

            if (!HasPermission(folder.Permission, body.Permission)) throw new ForbiddenException("Needed permissions are missing.", 8006);
            if (!string.IsNullOrWhiteSpace(folder.PhysicalPath) && !System.IO.Directory.Exists(folder.PhysicalPath))
            {
                throw new NotFoundException("Folder not found.", 8007);
            }

            if (body.UserId != null && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                throw new BadRequestException("User not found.", 8008);
            }

            if (await dbContext.ShareFolders.AnyAsync(f => f.Name == body.Name && f.UserId == body.UserId))
            {
                throw new BadRequestException("Folder with this name is already shared.", 8009);
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

        [HttpGet("folders")]
        [Authorize(Policy = Permissions.Share.GetShareFolders)]
        public async Task<ActionResult<IEnumerable<ShareItem>>> GetShareFolders()
        {
            ShareFolder[] shareFolders = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .ToArrayAsync();

            return shareFolders.Select(f => f.ToShareItem(f.GetExists())).ToArray();
        }

        [HttpGet("folder/{uuid}")]
        [Authorize(Policy = Permissions.Share.GetShareFolder)]
        public async Task<ActionResult<ShareItem>> GetShareFolder(Guid uuid)
        {
            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFolder == null) throw new NotFoundException("Share folder not found.", 8010);

            return shareFolder.ToShareItem(shareFolder.GetExists());
        }

        [HttpPut("folder/{uuid}")]
        [Authorize(Policy = Permissions.Share.PutShareFolder)]
        public async Task<ActionResult<FolderItem>> EditFolderShare(Guid uuid,
            [FromBody] EditFileSystemItemShareBody body)
        {
            await ValidateFileSystemItemShare(body);

            if (await dbContext.ShareFolders.AnyAsync(f =>
                f.Uuid != uuid && f.Name == body.Name && f.UserId == body.UserId))
            {
                throw new BadRequestException("Folder with this name is already shared.", 8011);
            }

            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFolder == null) throw new NotFoundException("Share folder not found.", 8012);

            shareFolder.Name = body.Name;
            shareFolder.IsListed = body.IsListed;
            shareFolder.UserId = body.UserId;

            await dbContext.SaveChangesAsync();

            return shareFolder.ToFolderItem();
        }

        private async Task<InternalFolder> ValidateAddFolderShare(AddFolderShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) throw new BadRequestException("Name missing.", 8013);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFolder folder = await ShareFolderHelper.GetFolderItem(body.Path, dbContext, userId);

            if (folder == null) throw new NotFoundException("Base not found.", 8014);
            if (!HasPermission(folder.Permission, body.Permission)) throw new ForbiddenException("Needed permissions are missing.", 8014);
            if (!string.IsNullOrWhiteSpace(folder.PhysicalPath) && !System.IO.Directory.Exists(folder.PhysicalPath))
            {
                throw new NotFoundException("Folder not found.", 8015);
            }

            if (body.UserId != null && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                throw new BadRequestException("User not found.", 8016);
            }

            return folder;
        }

        private static bool HasPermission(FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission hasPermission,
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
        [Authorize(Policy = Permissions.Share.DeleteShareFolder)]
        public async Task<ActionResult> DeleteShareFolder(Guid uuid)
        {
            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid);
            if (shareFolder == null) throw new NotFoundException("Share folder not found.", 8017);

            dbContext.ShareFolders.Remove(shareFolder);
            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
