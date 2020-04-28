using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemCommon.Model;
using FileSystemWeb.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<object> Get([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is missing");
            }

            if (!System.IO.File.Exists(path)) return NotFound();


            return PhysicalFile(path, Utils.GetContentType(Path.GetExtension(path)), Path.GetFileName(path), true);
        }

        [HttpGet("exists")]
        public bool Exists([FromQuery] string path)
        {
            return System.IO.File.Exists(path);
        }

        [HttpGet("info")]
        public ActionResult<FileItemInfo> GetInfo([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is missing");
            }

            try
            {
                FileInfo info = new FileInfo(path);
                return Utils.GetInfo(info);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("hash")]
        public ActionResult<string> GetHash([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is missing");
            }

            try
            {
                using (HashAlgorithm hasher = SHA1.Create())
                {
                    using (FileStream stream = System.IO.File.OpenRead(path))
                    {
                        return Convert.ToBase64String(hasher.ComputeHash(stream));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("copy")]
        public async Task<ActionResult> Copy([FromQuery] string srcPath, [FromQuery] string destPath)
        {
            if (string.IsNullOrWhiteSpace(srcPath))
            {
                return BadRequest("Source path is missing");
            }

            if (string.IsNullOrWhiteSpace(srcPath))
            {
                return BadRequest("Destination path is missing");
            }

            try
            {
                using (Stream source = System.IO.File.OpenRead(srcPath))
                {
                    using (Stream destination = System.IO.File.Create(destPath))
                    {
                        await source.CopyToAsync(destination);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("move")]
        public async Task<ActionResult> Move([FromQuery] string srcPath, [FromQuery] string destPath)
        {
            if (string.IsNullOrWhiteSpace(srcPath))
            {
                return BadRequest("Source path is missing");
            }

            if (string.IsNullOrWhiteSpace(srcPath))
            {
                return BadRequest("Destination path is missing");
            }

            try
            {
                await Task.Run(() => System.IO.File.Move(srcPath, destPath));
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<ActionResult> Write([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is missing");
            }

            string tmpPath = FileHelper.GenerateUniqueFileName(path);

            try
            {
                byte[] buffer = new byte[100000];
                using (FileStream dest = System.IO.File.Create(tmpPath))
                {
                    int size;
                    Stream src = HttpContext.Request.Body;
                    do
                    {
                        size = await src.ReadAsync(buffer, 0, buffer.Length);
                        await dest.WriteAsync(buffer, 0, size);
                    }
                    while (size > 0);
                }

                if (tmpPath != path)
                {
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    System.IO.File.Move(tmpPath, path);
                }
            }
            catch (Exception e)
            {
                try
                {
                    System.IO.File.Delete(tmpPath);
                }
                catch { }

                throw;
            }
            return Ok();
        }

        [HttpDelete]
        public ActionResult Delete([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is missing");
            }

            try
            {
                System.IO.File.Delete(path);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
