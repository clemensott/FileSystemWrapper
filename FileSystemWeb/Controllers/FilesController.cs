using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.Data;
using FileSystemWeb.Exceptions;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public FilesController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("")]
        [HttpGet("{encodedVirtualPath}")]
        public async Task<ActionResult> Get(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFile file;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!file.Permission.Read) return Forbid();
            if (!System.IO.File.Exists(file.PhysicalPath)) return NotFound();

            string fileName = Utils.ReplaceNonAscii(file.Name);
            Response.Headers.Add(HeaderNames.ContentDisposition, $"inline; filename=\"{fileName}\"");
            string contentType = Utils.GetContentType(Path.GetExtension(file.Name));
            return PhysicalFile(file.PhysicalPath, contentType, true);
        }

        [HttpGet("download")]
        [HttpGet("{encodedVirtualPath}/download")]
        public async Task<ActionResult> Download(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFile file;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!file.Permission.Read) return Forbid();
            if (!System.IO.File.Exists(file.PhysicalPath)) return NotFound();

            string contentType = Utils.GetContentType(Path.GetExtension(file.Name));
            return PhysicalFile(file.PhysicalPath, contentType, file.Name);
        }

        [HttpGet("exists")]
        [HttpGet("{encodedVirtualPath}/exists")]
        public async Task<ActionResult<bool>> Exists(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFile file;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return false;
            }

            if (!file.Permission.Info) return Forbid();

            return System.IO.File.Exists(file.PhysicalPath);
        }

        [HttpGet("info")]
        [HttpGet("{encodedVirtualPath}/info")]
        public async Task<ActionResult<FileItemInfo>> GetInfo(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFile file;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!file.Permission.Info) return Forbid();

            try
            {
                FileInfo info = new FileInfo(file.PhysicalPath);
                if (!info.Exists) return NotFound();
                return FileHelper.GetInfo(file, info);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("hash")]
        [HttpGet("{encodedVirtualPath}/hash")]
        public async Task<ActionResult<string>> GetHash(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFile file;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!file.Permission.Hash) return Forbid();

            try
            {
                using HashAlgorithm hasher = SHA1.Create();
                using FileStream stream = System.IO.File.OpenRead(file.PhysicalPath);
                return Convert.ToBase64String(hasher.ComputeHash(stream));
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("copy")]
        [HttpPost("{encodedVirtualSrcPath}/{encodedVirtualDestPath}/copy")]
        public async Task<ActionResult> Copy(string encodedVirtualSrcPath, string encodedVirtualDestPath, [FromQuery] string srcPath, [FromQuery] string destPath)
        {
            string virtualSrcPath = Utils.DecodePath(encodedVirtualSrcPath ?? srcPath);
            if (virtualSrcPath == null) return BadRequest("Src encoding error");
            string virtualDestPath = Utils.DecodePath(encodedVirtualDestPath ?? destPath);
            if (virtualDestPath == null) return BadRequest("Dest encoding error");
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            InternalFile srcFile, destFile;
            try
            {
                srcFile = await ShareFileHelper.GetFileItem(virtualSrcPath, dbContext, userId, this);
                destFile = await ShareFileHelper.GetFileItem(virtualDestPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!srcFile.Permission.Write) return Forbid();
            if (!destFile.Permission.Write) return Forbid();

            try
            {
                await using Stream source = System.IO.File.OpenRead(srcFile.PhysicalPath);
                await using Stream destination = System.IO.File.Create(destFile.PhysicalPath);
                await source.CopyToAsync(destination);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("move")]
        [HttpPost("{encodedVirtualSrcPath}/{encodedVirtualDestPath}/move")]
        public async Task<ActionResult> Move(string encodedVirtualSrcPath, string encodedVirtualDestPath, [FromQuery] string srcPath, [FromQuery] string destPath)
        {
            string virtualSrcPath = Utils.DecodePath(encodedVirtualSrcPath ?? srcPath);
            if (virtualSrcPath == null) return BadRequest("Src encoding error");
            string virtualDestPath = Utils.DecodePath(encodedVirtualDestPath ?? destPath);
            if (virtualDestPath == null) return BadRequest("Dest encoding error");
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            InternalFile srcFile, destFile;
            try
            {
                srcFile = await ShareFileHelper.GetFileItem(virtualSrcPath, dbContext, userId, this);
                destFile = await ShareFileHelper.GetFileItem(virtualDestPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!srcFile.Permission.Write) return Forbid();
            if (!destFile.Permission.Write) return Forbid();

            try
            {
                await Task.Run(() => System.IO.File.Move(srcFile.PhysicalPath, destFile.PhysicalPath));
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }

        [DisableRequestSizeLimit]
        [HttpPost("")]
        [HttpPost("{encodedVirtualPath}")]
        public async Task<ActionResult> Write(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFile file;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!file.Permission.Write) return Forbid();

            string physicalPath = file.PhysicalPath;
            string tmpPath = FileHelper.GenerateUniqueFileName(physicalPath);

            try
            {
                byte[] buffer = new byte[100000];
                await using (Stream src = Request.Body)
                await using (FileStream dest = System.IO.File.Create(tmpPath))
                {
                    int size;
                    do
                    {
                        size = await src.ReadAsync(buffer, 0, buffer.Length);
                        await dest.WriteAsync(buffer, 0, size);
                    } while (size > 0);
                }

                if (tmpPath != physicalPath)
                {
                    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
                    System.IO.File.Move(tmpPath, physicalPath);
                }
            }
            catch (Exception)
            {
                try
                {
                    if (System.IO.File.Exists(tmpPath)) System.IO.File.Delete(tmpPath);
                }
                catch { }

                throw;
            }

            return Ok();
        }

        [HttpDelete("")]
        [HttpDelete("{encodedVirtualPath}")]
        public async Task<ActionResult> Delete(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            InternalFile file;
            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!file.Permission.Write) return Forbid();

            try
            {
                System.IO.File.Delete(file.PhysicalPath);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
