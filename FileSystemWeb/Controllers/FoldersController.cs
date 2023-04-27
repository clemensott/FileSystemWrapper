using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemWeb.Data;
using FileSystemWeb.Exceptions;
using FileSystemWeb.Helpers;
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
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFolder folder;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException)
            {
                return false;
            }

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
            if (virtualPath == null) return BadRequest("Path encoding error");
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return await FolderContentHelper.FromRoot(dbContext, userId, sortType, sortDirection);
            }

            InternalFolder folder;
            try
            {
                folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!folder.Permission.List) return Forbid();

            try
            {
                return FolderContentHelper.FromFolder(folder, sortType, sortDirection);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound("Directory not found");
            }
        }

        private static IEnumerable<FolderItem> GetFolders(InternalFolder folder)
        {
            if (string.IsNullOrWhiteSpace(folder.PhysicalPath))
            {
                return DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => new FolderItem()
                {
                    Name = d.Name,
                    Path = Path.Join(folder.VirtualPath, d.Name),
                    SharedId = null,
                    Permission = folder.Permission,
                    Deletable = false,
                });
            }

            return Directory.EnumerateDirectories(folder.PhysicalPath).Select(p => new FolderItem()
            {
                Name = Path.GetFileName(p),
                Path = Path.Join(folder.VirtualPath, Path.GetFileName(p)),
                SharedId = null,
                Permission = folder.Permission,
                Deletable = true,
            });
        }

        private static IEnumerable<FileItem> GetFiles(InternalFolder folder)
        {
            if (string.IsNullOrWhiteSpace(folder.PhysicalPath)) return new FileItem[0];
            return Directory.EnumerateFiles(folder.PhysicalPath).Select(GetFileItem);

            FileItem GetFileItem(string path)
            {
                string name = Path.GetFileName(path);
                return new FileItem()
                {
                    Name = name,
                    Extension = Path.GetExtension(path),
                    Path = Path.Join(folder.VirtualPath, name),
                    SharedId = null,
                    Permission = (FileItemPermission)folder.Permission,
                };
            }
        }

        [HttpGet("info")]
        [HttpGet("{encodedVirtualPath}/info")]
        public async Task<ActionResult<FolderItemInfo>> GetInfo(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFolder folder;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!folder.Permission.Info) return Forbid();

            try
            {
                DirectoryInfo info = null;
                if (!string.IsNullOrWhiteSpace(folder.PhysicalPath))
                {
                    info = new DirectoryInfo(folder.PhysicalPath);
                    if (!info.Exists) return NotFound();
                }

                return FileHelper.GetInfo(folder, info);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound("Directory not found");
            }
        }

        [HttpGet("infoWithSize")]
        [HttpGet("{encodedVirtualPath}/infoWithSize")]
        public async Task<ActionResult<FolderItemInfoWithSize>> GetInfoWithSize(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFolder folder;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!folder.Permission.Info) return Forbid();

            try
            {
                DirectoryInfo info = null;
                if (!string.IsNullOrWhiteSpace(folder.PhysicalPath))
                {
                    info = new DirectoryInfo(folder.PhysicalPath);
                    if (!info.Exists) return NotFound();
                }

                return FileHelper.GetInfoWithSize(folder, info);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound("Directory not found");
            }
        }

        [HttpPost("")]
        [HttpPost("{encodedVirtualPath}")]
        public async Task<ActionResult<FolderItemInfo>> Create(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFolder folder;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!folder.Permission.Write) return Forbid();

            DirectoryInfo info = Directory.CreateDirectory(folder.PhysicalPath);
            return FileHelper.GetInfo(folder, info);
        }

        [HttpDelete("")]
        [HttpDelete("{encodedVirtualPath}")]
        public async Task<ActionResult> Delete(string encodedVirtualPath, [FromQuery] string path, [FromQuery] bool recursive)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFolder folder;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                folder = await ShareFolderHelper.GetFolderItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!folder.Permission.Write) return Forbid();

            try
            {
                await Task.Run(() => Directory.Delete(folder.PhysicalPath, recursive));
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
