using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models.Exceptions;
using FileSystemWeb.Models.Internal;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    public class FoldersController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public FoldersController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("exists")]
        [HttpGet("{encodedVirtualPath}/exists")]
        public async Task<ActionResult<bool>> Exists(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 7001);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFolder folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId);

            return folder.Permission.Read &&
                   (string.IsNullOrWhiteSpace(folder.PhysicalPath) || Directory.Exists(folder.PhysicalPath));
        }

        [HttpGet("content")]
        [HttpGet("content/{encodedVirtualPath}")]
        public async Task<ActionResult<FolderContent>> ListFolders(string encodedVirtualPath, [FromQuery] string path,
            [FromQuery] FileSystemItemSortType sortType = FileSystemItemSortType.Name,
            [FromQuery] FileSystemItemSortDirection sortDirection = FileSystemItemSortDirection.ASC)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path ?? string.Empty);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 7002);
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return await FolderContentHelper.FromRoot(dbContext, userId, sortType, sortDirection);
            }

            InternalFolder folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId);

            if (!folder.Permission.List) throw new ForbiddenException("No list permission.", 7003);

            try
            {
                return FolderContentHelper.FromFolder(folder, sortType, sortDirection);
            }
            catch (DirectoryNotFoundException)
            {
                throw new NotFoundException("Directory not found.", 7004);
            }
        }

        [HttpGet("info")]
        [HttpGet("{encodedVirtualPath}/info")]
        public async Task<ActionResult<FolderItemInfo>> GetInfo(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 7005);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFolder folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId);

            if (!folder.Permission.Info) throw new ForbiddenException("No info permission", 7006);

            try
            {
                DirectoryInfo info = null;
                if (!string.IsNullOrWhiteSpace(folder.PhysicalPath))
                {
                    info = new DirectoryInfo(folder.PhysicalPath);
                    if (!info.Exists) throw new NotFoundException("Directory not found.", 7007);
                }

                return FileHelper.GetInfo(folder, info);
            }
            catch (DirectoryNotFoundException)
            {
                throw new NotFoundException("Directory not found.", 7008);
            }
        }

        [HttpGet("infoWithSize")]
        [HttpGet("{encodedVirtualPath}/infoWithSize")]
        public async Task<ActionResult<FolderItemInfoWithSize>> GetInfoWithSize(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 7009);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFolder folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId);

            if (!folder.Permission.Info) throw new ForbiddenException("No info permission", 7010);

            try
            {
                DirectoryInfo info = null;
                if (!string.IsNullOrWhiteSpace(folder.PhysicalPath))
                {
                    info = new DirectoryInfo(folder.PhysicalPath);
                    if (!info.Exists) throw new NotFoundException("Directory not found.", 7011);
                }

                return FileHelper.GetInfoWithSize(folder, info);
            }
            catch (DirectoryNotFoundException)
            {
                throw new NotFoundException("Directory not found.", 7012);
            }
        }

        [HttpPost("")]
        [HttpPost("{encodedVirtualPath}")]
        public async Task<ActionResult<FolderItemInfo>> Create(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 7013);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFolder folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId);


            if (!folder.Permission.Write) throw new ForbiddenException("No write permission", 7014);

            DirectoryInfo info = Directory.CreateDirectory(folder.PhysicalPath);
            return FileHelper.GetInfo(folder, info);
        }

        [HttpDelete("")]
        [HttpDelete("{encodedVirtualPath}")]
        public async Task<ActionResult> Delete(string encodedVirtualPath, [FromQuery] string path, [FromQuery] bool recursive)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 7015);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFolder folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId);

            if (!folder.Permission.Write) throw new ForbiddenException("No write permission.", 7016);

            try
            {
                await Task.Run(() => Directory.Delete(folder.PhysicalPath, recursive));
            }
            catch (DirectoryNotFoundException)
            {
                throw new NotFoundException("Directory not found.", 7017);
            }

            return Ok();
        }
    }
}
