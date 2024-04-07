using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Helpers;
using FileSystemWeb.Services.Folder;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers.API
{
    [Route("api/[controller]")]
    public class FoldersController : ControllerBase
    {
        private readonly FolderService folderService;

        public FoldersController(FolderService folderService)
        {
            this.folderService = folderService;
        }

        [HttpGet("exists")]
        [HttpGet("{encodedVirtualPath}/exists")]
        public async Task<ActionResult<bool>> Exists(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            return await folderService.Exists(userId, virtualPath);
        }

        [HttpGet("content")]
        [HttpGet("content/{encodedVirtualPath}")]
        public async Task<ActionResult<FolderContent>> ListFolders(string encodedVirtualPath, [FromQuery] string path,
            [FromQuery] FileSystemItemSortType sortType = FileSystemItemSortType.Name,
            [FromQuery] FileSystemItemSortDirection sortDirection = FileSystemItemSortDirection.ASC)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            return await folderService.GetContent(userId, virtualPath, sortType, sortDirection);
        }

        [HttpGet("info")]
        [HttpGet("{encodedVirtualPath}/info")]
        public async Task<ActionResult<FolderItemInfo>> GetInfo(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            return await folderService.GetInfo(userId, virtualPath);
        }

        [HttpGet("infoWithSize")]
        [HttpGet("{encodedVirtualPath}/infoWithSize")]
        public async Task<ActionResult<FolderItemInfoWithSize>> GetInfoWithSize(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            return await folderService.GetInfoWithSize(userId, virtualPath);
        }

        [HttpPost("")]
        [HttpPost("{encodedVirtualPath}")]
        public async Task<ActionResult<FolderItemInfo>> Create(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            return await folderService.Create(userId, virtualPath);
        }

        [HttpDelete("")]
        [HttpDelete("{encodedVirtualPath}")]
        public async Task<ActionResult> Delete(string encodedVirtualPath, [FromQuery] string path, [FromQuery] bool recursive)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            await folderService.Delete(userId, virtualPath, recursive);

            return Ok();
        }
    }
}
