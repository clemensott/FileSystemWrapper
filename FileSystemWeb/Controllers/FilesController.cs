using System;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Files.Many;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Exceptions;
using FileSystemWeb.Models.RequestBodies;
using Microsoft.AspNetCore.Http;
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
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 9001);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId);

            if (!file.Permission.Read) throw new ForbiddenException("No read permission.", 9002);
            if (!System.IO.File.Exists(file.PhysicalPath)) throw new NotFoundException("File not found.", 9003);

            string fileName = Utils.ReplaceNonAscii(file.Name);
            Response.Headers.Append(HeaderNames.ContentDisposition, $"inline; filename=\"{fileName}\"");
            string contentType = Utils.GetContentType(Path.GetExtension(file.Name));
            return PhysicalFile(file.PhysicalPath, contentType, true);
        }

        [HttpGet("download")]
        [HttpGet("{encodedVirtualPath}/download")]
        public async Task<ActionResult> Download(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 9004);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId);

            if (!file.Permission.Read) throw new ForbiddenException("No read permission.", 9005);
            if (!System.IO.File.Exists(file.PhysicalPath)) throw new NotFoundException("File not found.", 9006);

            string contentType = Utils.GetContentType(Path.GetExtension(file.Name));
            return PhysicalFile(file.PhysicalPath, contentType, file.Name);
        }

        [HttpGet("exists")]
        [HttpGet("{encodedVirtualPath}/exists")]
        public async Task<ActionResult<bool>> Exists(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 9007);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId);

            if (!file.Permission.Info) throw new ForbiddenException("No info permission.", 9008);

            return System.IO.File.Exists(file.PhysicalPath);
        }

        [HttpPost("existsMany")]
        public async Task ExistMany([FromBody] FilesExistsManyBody body)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (body.Paths.Length == 0) throw new BadRequestException("No encodedVirtualPaths.", 9034);

            foreach (string path in body.Paths)
            {
                bool? exists = null;
                HttpStatusCode statusCode;
                string errorMessage = null;
                int? errorCode = null;

                try
                {
                    if (path == null) throw new BadRequestException("Path encoding error.", 9013);
                    InternalFile file = await ShareFileHelper.GetFileItem(path, dbContext, userId);

                    if (!file.Permission.Info) throw new ForbiddenException("No hash permission.", 9035);

                    exists = System.IO.File.Exists(file.PhysicalPath);
                    statusCode = HttpStatusCode.OK;
                }
                catch (FileNotFoundException)
                {
                    statusCode = HttpStatusCode.NotFound;
                    errorMessage = "File not found.";
                    errorCode = 9015;
                }
                catch (HttpException exc)
                {
                    statusCode = exc.Status;
                    errorMessage = exc.Message;
                    errorCode = exc.Code;
                }
                catch (Exception exc)
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    errorMessage = exc.Message;
                    errorCode = 9034;
                }

                if (!HttpContext.Response.HasStarted) HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                FileExistsManyItem response = new FileExistsManyItem(path, exists, statusCode, errorMessage, errorCode);
                await HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

        [HttpGet("info")]
        [HttpGet("{encodedVirtualPath}/info")]
        public async Task<ActionResult<FileItemInfo>> GetInfo(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 9009);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId);

            if (!file.Permission.Info) throw new ForbiddenException("No info permission.", 9010);

            try
            {
                FileInfo info = new FileInfo(file.PhysicalPath);
                if (!info.Exists) throw new NotFoundException("File not found.", 9011);
                return FileHelper.GetInfo(file, info);
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundException("File not found.", 9012);
            }
        }

        [HttpPost("infoMany")]
        public async Task GetInfo([FromBody] FilesInfoManyBody body)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (body.Paths.Length == 0) throw new BadRequestException("No encodedVirtualPaths.", 9034);

            foreach (string path in body.Paths)
            {
                FileItemInfo? fileItemInfo = null;
                HttpStatusCode statusCode;
                string errorMessage = null;
                int? errorCode = null;

                try
                {
                    if (path == null) throw new BadRequestException("Path encoding error.", 9013);
                    InternalFile file = await ShareFileHelper.GetFileItem(path, dbContext, userId);

                    if (!file.Permission.Info) throw new ForbiddenException("No hash permission.", 9035);

                    FileInfo info = new FileInfo(file.PhysicalPath);
                    if (!info.Exists) throw new NotFoundException("File not found.", 9011);
                    fileItemInfo = FileHelper.GetInfo(file, info);
                    statusCode = HttpStatusCode.OK;
                }
                catch (FileNotFoundException)
                {
                    statusCode = HttpStatusCode.NotFound;
                    errorMessage = "File not found.";
                    errorCode = 9015;
                }
                catch (HttpException exc)
                {
                    statusCode = exc.Status;
                    errorMessage = exc.Message;
                    errorCode = exc.Code;
                }
                catch (Exception exc)
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    errorMessage = exc.Message;
                    errorCode = 9034;
                }

                if (!HttpContext.Response.HasStarted) HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                FileInfoManyItem response = new FileInfoManyItem(path, fileItemInfo, statusCode, errorMessage, errorCode);
                await HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

        [HttpGet("hash")]
        [HttpGet("{encodedVirtualPath}/hash")]
        public async Task<ActionResult<string>> GetHash(string encodedVirtualPath, [FromQuery] string path, [FromQuery] int partialSize)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 9016);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId);

            if (!file.Permission.Hash) throw new ForbiddenException("No hash permission.", 9017);

            try
            {
                using HashAlgorithm hasher = SHA1.Create();
                using FileStream stream = System.IO.File.OpenRead(file.PhysicalPath);

                byte[] hashBytes;
                if (partialSize > 0)
                {
                    byte[] partialData = await Utils.GetPartialBinary(stream, partialSize);
                    hashBytes = hasher.ComputeHash(partialData);
                }
                else hashBytes = await hasher.ComputeHashAsync(stream);

                return Convert.ToBase64String(hashBytes);
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundException("File not found.", 9018);
            }
        }

        [HttpPost("hashMany")]
        public async Task GetHashes([FromBody] FilesHashManyBody body)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (body.Paths.Length == 0) throw new BadRequestException("No encodedVirtualPaths.", 9033);

            foreach (string path in body.Paths)
            {
                string hash = null;
                HttpStatusCode statusCode;
                string errorMessage = null;
                int? errorCode = null;

                try
                {
                    InternalFile file = await ShareFileHelper.GetFileItem(path, dbContext, userId);

                    if (!file.Permission.Hash) throw new ForbiddenException("No hash permission.", 9014);

                    using HashAlgorithm hasher = SHA1.Create();
                    using FileStream stream = System.IO.File.OpenRead(file.PhysicalPath);

                    byte[] hashBytes;
                    if (body.PartialSize > 0)
                    {
                        byte[] partialData = await Utils.GetPartialBinary(stream, body.PartialSize.Value);
                        hashBytes = hasher.ComputeHash(partialData);
                    }
                    else hashBytes = await hasher.ComputeHashAsync(stream);

                    hash = Convert.ToBase64String(hashBytes);
                    statusCode = HttpStatusCode.OK;
                }
                catch (FileNotFoundException)
                {
                    statusCode = HttpStatusCode.NotFound;
                    errorMessage = "File not found.";
                    errorCode = 9015;
                }
                catch (HttpException exc)
                {
                    statusCode = exc.Status;
                    errorMessage = exc.Message;
                    errorCode = exc.Code;
                }
                catch (Exception exc)
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    errorMessage = exc.Message;
                    errorCode = 9034;
                }

                if (!HttpContext.Response.HasStarted) HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                FileHashManyItem response = new FileHashManyItem(path, hash, statusCode, errorMessage, errorCode);
                await HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

        [HttpPost("copy")]
        [HttpPost("{encodedVirtualSrcPath}/{encodedVirtualDestPath}/copy")]
        public async Task<ActionResult> Copy(string encodedVirtualSrcPath, string encodedVirtualDestPath, [FromQuery] string srcPath, [FromQuery] string destPath)
        {
            string virtualSrcPath = Utils.DecodePath(encodedVirtualSrcPath ?? srcPath);
            if (virtualSrcPath == null) throw new BadRequestException("Src encoding error.", 9019);
            string virtualDestPath = Utils.DecodePath(encodedVirtualDestPath ?? destPath);
            if (virtualDestPath == null) throw new BadRequestException("Dest encoding error.", 9020);
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            InternalFile srcFile = await ShareFileHelper.GetFileItem(virtualSrcPath, dbContext, userId);
            InternalFile destFile = await ShareFileHelper.GetFileItem(virtualDestPath, dbContext, userId);

            if (!srcFile.Permission.Read) throw new ForbiddenException("No read permission for src file.", 9021);
            if (!destFile.Permission.Write) throw new ForbiddenException("No write permission for dest file.", 9022);

            try
            {
                await using Stream source = System.IO.File.OpenRead(srcFile.PhysicalPath);
                await using Stream destination = System.IO.File.Create(destFile.PhysicalPath);
                await source.CopyToAsync(destination);
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundException("File not found.", 9023);
            }

            return Ok();
        }

        [HttpPost("move")]
        [HttpPost("{encodedVirtualSrcPath}/{encodedVirtualDestPath}/move")]
        public async Task<ActionResult> Move(string encodedVirtualSrcPath, string encodedVirtualDestPath, [FromQuery] string srcPath, [FromQuery] string destPath)
        {
            string virtualSrcPath = Utils.DecodePath(encodedVirtualSrcPath ?? srcPath);
            if (virtualSrcPath == null) throw new BadRequestException("Src encoding error.", 9023);
            string virtualDestPath = Utils.DecodePath(encodedVirtualDestPath ?? destPath);
            if (virtualDestPath == null) throw new BadRequestException("Dest encoding error.", 9024);
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            InternalFile srcFile = await ShareFileHelper.GetFileItem(virtualSrcPath, dbContext, userId);
            InternalFile destFile = await ShareFileHelper.GetFileItem(virtualDestPath, dbContext, userId);

            if (!srcFile.Permission.Write) throw new ForbiddenException("No write permission for src file.", 9025);
            if (!destFile.Permission.Write) throw new ForbiddenException("No write permission for dest file.", 9026);

            try
            {
                await Task.Run(() => System.IO.File.Move(srcFile.PhysicalPath, destFile.PhysicalPath));
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundException("File not found.", 9027);
            }

            return Ok();
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        [HttpPost("{encodedVirtualPath}")]
        public async Task<ActionResult> Write(string encodedVirtualPath, [FromQuery] string path, [FromForm] WriteFileBody form)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 9027);
            if (form?.FileContent == null) throw new BadRequestException("Missing file content.", 9028);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId);

            if (!file.Permission.Write) throw new ForbiddenException("No write permission.", 9029);

            string physicalPath = file.PhysicalPath;
            string tmpPath = FileHelper.GenerateUniqueFileName(physicalPath);

            try
            {
                await using (FileStream dest = System.IO.File.Create(tmpPath))
                {
                    await form.FileContent.CopyToAsync(dest);
                }

                if (tmpPath != physicalPath)
                {
                    System.IO.File.Move(tmpPath, physicalPath, true);
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
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 9030);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId);

            if (!file.Permission.Write) throw new ForbiddenException("No write permission.", 9031);

            try
            {
                System.IO.File.Delete(file.PhysicalPath);
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundException("File not found.", 9032);
            }

            return Ok();
        }
    }
}
