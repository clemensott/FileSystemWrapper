using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommon.Models.Share;
using FileSystemWeb.Constants;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Models;
using FileSystemWeb.Services.Share;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers.API
{
    [Route("api/[controller]")]
    [Authorize]
    public class ShareController : Controller
    {
        private readonly ShareService shareService;

        public ShareController(ShareService shareService)
        {
            this.shareService = shareService;
        }

        [HttpPost("file")]
        [Authorize(Policy = Permissions.Share.PostShareFile)]
        public async Task<ActionResult<FileItem>> AddShareFile([FromBody] AddFileShareBody body)
        {
            string userId = HttpContext.GetUserId();
            ShareFile shareFile = await shareService.AddShareFile(userId, body);

            return shareFile.ToFileItem();
        }

        [HttpGet("files")]
        [Authorize(Policy = Permissions.Share.GetShareFiles)]
        public async Task<ActionResult<IEnumerable<ShareItem>>> GetShareFiles()
        {
            return await shareService.GetShareFiles();
        }

        [HttpGet("file/{uuid}")]
        [Authorize(Policy = Permissions.Share.GetShareFile)]
        public async Task<ActionResult<ShareItem>> GetShareFile(Guid uuid)
        {
            return await shareService.GetShareFile(uuid);
        }

        [HttpPut("file/{uuid}")]
        [Authorize(Policy = Permissions.Share.PutShareFile)]
        public async Task<ActionResult<FileItem>> EditShareFile(Guid uuid, [FromBody] EditFileSystemItemShareBody body)
        {
            ShareFile shareFile = await shareService.EditShareFile(uuid, body);

            return shareFile.ToFileItem();
        }

        [HttpDelete("file/{uuid}")]
        [Authorize(Policy = Permissions.Share.DeleteShareFile)]
        public async Task<ActionResult> DeleteShareFile(Guid uuid)
        {
            await shareService.DeleteShareFile(uuid);

            return Ok();
        }

        [HttpPost("folder")]
        [Authorize(Policy = Permissions.Share.PostShareFolder)]
        public async Task<ActionResult<FolderItem>> AddFolderShare([FromBody] AddFolderShareBody body)
        {
            string userId = HttpContext.GetUserId();
            ShareFolder shareFolder = await shareService.AddFolderShare(userId, body);

            return shareFolder.ToFolderItem();
        }

        [HttpGet("folders")]
        [Authorize(Policy = Permissions.Share.GetShareFolders)]
        public async Task<ActionResult<IEnumerable<ShareItem>>> GetShareFolders()
        {
            return await shareService.GetShareFolders();
        }

        [HttpGet("folder/{uuid}")]
        [Authorize(Policy = Permissions.Share.GetShareFolder)]
        public async Task<ActionResult<ShareItem>> GetShareFolder(Guid uuid)
        {
            return await shareService.GetShareFolder(uuid);
        }

        [HttpPut("folder/{uuid}")]
        [Authorize(Policy = Permissions.Share.PutShareFolder)]
        public async Task<ActionResult<FolderItem>> EditFolderShare(Guid uuid,
            [FromBody] EditFileSystemItemShareBody body)
        {
            ShareFolder shareFolder = await shareService.EditFolderShare(uuid, body);

            return shareFolder.ToFolderItem();
        }

        [HttpDelete("folder/{uuid}")]
        [Authorize(Policy = Permissions.Share.DeleteShareFolder)]
        public async Task<ActionResult> DeleteShareFolder(Guid uuid)
        {
            await shareService.DeleteShareFolder(uuid);

            return Ok();
        }
    }
}
