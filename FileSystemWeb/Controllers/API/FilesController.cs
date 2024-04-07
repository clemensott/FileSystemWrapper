using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Models.RequestBodies;
using FileSystemWeb.Services.File;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace FileSystemWeb.Controllers.API
{
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FileService fileService;

        public FilesController(FileService fileService)
        {
            this.fileService = fileService;
        }

        [HttpGet("")]
        [HttpGet("{encodedVirtualPath}")]
        public async Task<ActionResult> Get(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            InternalFile file = await fileService.Get(userId, virtualPath);

            string fileName = Utils.ReplaceNonAscii(file.Name);
            Response.Headers.Add(HeaderNames.ContentDisposition, $"inline; filename=\"{fileName}\"");
            string contentType = Utils.GetContentType(Path.GetExtension(file.Name));
            return PhysicalFile(file.PhysicalPath, contentType, true);
        }

        [HttpGet("download")]
        [HttpGet("{encodedVirtualPath}/download")]
        public async Task<ActionResult> Download(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            InternalFile file = await fileService.Get(userId, virtualPath);

            string contentType = Utils.GetContentType(Path.GetExtension(file.Name));
            return PhysicalFile(file.PhysicalPath, contentType, file.Name);
        }

        [HttpGet("exists")]
        [HttpGet("{encodedVirtualPath}/exists")]
        public async Task<ActionResult<bool>> Exists(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            return await fileService.Exists(userId, virtualPath);
        }

        [HttpGet("info")]
        [HttpGet("{encodedVirtualPath}/info")]
        public async Task<ActionResult<FileItemInfo>> GetInfo(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            return await fileService.GetInfo(userId, virtualPath);
        }

        [HttpGet("hash")]
        [HttpGet("{encodedVirtualPath}/hash")]
        public async Task<ActionResult<string>> GetHash(string encodedVirtualPath, [FromQuery] string path, [FromQuery] int partialSize)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            return await fileService.GetHash(userId, virtualPath, partialSize);
        }

        [HttpPost("copy")]
        [HttpPost("{encodedVirtualSrcPath}/{encodedVirtualDestPath}/copy")]
        public async Task<ActionResult> Copy(string encodedVirtualSrcPath, string encodedVirtualDestPath, [FromQuery] string srcPath, [FromQuery] string destPath)
        {
            string userId = HttpContext.GetUserId();
            string virtualSrcPath = PathHelper.DecodeAndValidatePath(encodedVirtualSrcPath ?? srcPath);
            string virtualDestPath = PathHelper.DecodeAndValidatePath(encodedVirtualDestPath ?? destPath);
            await fileService.Copy(userId, virtualSrcPath, virtualDestPath);

            return Ok();
        }

        [HttpPost("move")]
        [HttpPost("{encodedVirtualSrcPath}/{encodedVirtualDestPath}/move")]
        public async Task<ActionResult> Move(string encodedVirtualSrcPath, string encodedVirtualDestPath, [FromQuery] string srcPath, [FromQuery] string destPath)
        {
            string userId = HttpContext.GetUserId();
            string virtualSrcPath = PathHelper.DecodeAndValidatePath(encodedVirtualSrcPath ?? srcPath);
            string virtualDestPath = PathHelper.DecodeAndValidatePath(encodedVirtualDestPath ?? destPath);
            await fileService.Move(userId, virtualSrcPath, virtualDestPath);

            return Ok();
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        [HttpPost("{encodedVirtualPath}")]
        public async Task<ActionResult> Write(string encodedVirtualPath, [FromQuery] string path, [FromForm] WriteFileBody form)
        {
            if (form?.FileContent == null) return BadRequest("Missing file content");
            
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            await fileService.Write(userId, virtualPath, form.FileContent);

            return Ok();
        }

        [HttpDelete("")]
        [HttpDelete("{encodedVirtualPath}")]
        public async Task<ActionResult> Delete(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            await fileService.Delete(userId, virtualPath);

            return Ok();
        }
    }
}
