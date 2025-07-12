using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Exceptions;
using FileSystemWeb.Models.RequestBodies;
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
            if (virtualPath == null) throw new BadRequestException("Path encoding error.", 5006);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            InternalFile file = await ShareFileHelper.GetFileItem(virtualPath, dbContext, userId);

            if (!file.Permission.Write) throw new ForbiddenException("No write permission.", 5005);

            BigFileUpload upload = new BigFileUpload()
            {
                DestinationPath = file.PhysicalPath,
                TempPath = FileHelper.GenerateUniqueFileName(file.PhysicalPath),
                UserId = userId,
                LastActivity = DateTime.UtcNow,
            };

            await dbContext.BigFileUploads.AddAsync(upload);
            await dbContext.SaveChangesAsync();

            await System.IO.File.WriteAllBytesAsync(upload.TempPath, new byte[0]);

            return upload.Uuid.ToString();
        }

        private async Task<BigFileUpload> GetValidBigFileUpload(Guid uuid, string userId, bool validateFileExists = true)
        {
            BigFileUpload upload = await dbContext.BigFileUploads
              .FirstOrDefaultAsync(u => u.Uuid == uuid && u.UserId == userId);

            if (upload == null)
            {
                throw new NotFoundException("Upload not found.", 5001);
            }

            if (validateFileExists && !System.IO.File.Exists(upload.TempPath))
            {
                throw new NotFoundException("Temporary file not found.", 5002);
            }

            return upload;
        }


        [HttpPost("{uuid}/append")]
        public async Task<ActionResult> AppendUpload(Guid uuid, [FromForm] AppendBigFileBody form)
        {
            if (form.PartialFile == null) throw new BadRequestException("No data or file.", 5003);

            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            BigFileUpload upload = await GetValidBigFileUpload(uuid, userId);

            await using FileStream dest = System.IO.File.Open(upload.TempPath, FileMode.Append);
            await form.PartialFile.CopyToAsync(dest);

            upload.LastActivity = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpPut("{uuid}/finish")]
        public async Task<ActionResult> FinishUpload(Guid uuid)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            BigFileUpload upload = await GetValidBigFileUpload(uuid, userId);

            if (upload.TempPath != upload.DestinationPath)
            {
                System.IO.File.Move(upload.TempPath, upload.DestinationPath, true);
            }

            dbContext.BigFileUploads.Remove(upload);
            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{uuid}")]
        public async Task<ActionResult> CancelUpload(Guid uuid)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            BigFileUpload upload = await GetValidBigFileUpload(uuid, userId, false);

            System.IO.File.Delete(upload.TempPath);

            dbContext.BigFileUploads.Remove(upload);
            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
