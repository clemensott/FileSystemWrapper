using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class FoldersController : ControllerBase
    {
        [HttpGet("exists")]
        public bool Exists([FromQuery] string path)
        {
            return Directory.Exists(path);
        }

        [HttpGet("listfiles")]
        public ActionResult<IEnumerable<string>> ListFiles([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new string[0];
            }

            try
            {
                return Directory.GetFiles(path);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("listfolders")]
        public ActionResult<IEnumerable<string>> ListFolders([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return DriveInfo.GetDrives().Select(d => d.Name).ToArray();
            }

            try
            {
                return Directory.GetDirectories(path);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("info")]
        public ActionResult<FolderItemInfo> GetInfo([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is missing");
            }

            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                return Utils.GetInfo(info);
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult<FolderItemInfo> Create([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is missing");
            }

            DirectoryInfo info = Directory.CreateDirectory(path);
            return Utils.GetInfo(info);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromQuery] string path, [FromQuery] bool recrusive)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is missing");
            }

            try
            {
                await Task.Run(() => Directory.Delete(path, recrusive));
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
