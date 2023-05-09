using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Exceptions;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class BigFileController : Controller
    {
        private readonly AppDbContext dbContext;

        public BigFileController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("start")]
        [HttpPost("{encodedVirtualPath}/start")]
        public async Task<ActionResult<string>> StartUpload(string encodedVirtualPath, [FromQuery] string path)
        {
            string virtualPath = Utils.DecodePath(encodedVirtualPath ?? path);
            if (virtualPath == null) return BadRequest("Path encoding error");

            string userId;
            InternalFile file;
            try
            {
                userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId, this);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            if (!file.Permission.Write) return Forbid();

            BigFileUpload upload = new BigFileUpload()
            {
                DestinationPath = file.PhysicalPath,
                TempPath = Path.GetTempFileName(),
                UserId = userId,
                LastActivity = DateTime.UtcNow,
            };

            await dbContext.BigFileUploads.AddAsync(upload);
            await dbContext.SaveChangesAsync();

            return upload.Uuid.ToString();
        }

        private async Task<BigFileUpload> GetValidBigFileUpload(Guid uuid, string userId)
        {
            BigFileUpload upload = await dbContext.BigFileUploads
              .FirstOrDefaultAsync(u => u.Uuid == uuid && u.UserId == userId);

            if (upload == null)
            {
                throw (HttpResultException)NotFound("Upload not found.");
            }

            if (!System.IO.File.Exists(upload.TempPath))
            {
                throw (HttpResultException)NotFound("Temporary file not found.");
            }

            return upload;
        }


        [HttpPost("{uuid}/append")]
        public async Task<ActionResult> AppendUpload(Guid uuid, [FromForm] byte[] data)
        {
            BigFileUpload upload;

            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                upload = await GetValidBigFileUpload(uuid, userId);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            using FileStream stream = System.IO.File.OpenWrite(upload.TempPath);
            await stream.WriteAsync(data);

            upload.LastActivity = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpPut("{uuid}/finish")]
        public async Task<ActionResult> FinishUpload(Guid uuid)
        {
            BigFileUpload upload;

            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                upload = await GetValidBigFileUpload(uuid, userId);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            System.IO.File.Move(upload.TempPath, upload.DestinationPath, true);

            dbContext.BigFileUploads.Remove(upload);
            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{uuid}")]
        public async Task<ActionResult> CancelUpload(Guid uuid)
        {
            BigFileUpload upload;

            try
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                upload = await GetValidBigFileUpload(uuid, userId);
            }
            catch (HttpResultException exc)
            {
                return exc.Result;
            }

            System.IO.File.Delete(upload.TempPath);

            dbContext.BigFileUploads.Remove(upload);
            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
