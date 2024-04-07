using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Services.Share;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StdOttStandard.Models.HttpExceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileSystemWeb.Services.File
{
    public class BigFileService
    {
        private readonly AppDbContext dbContext;
        private readonly ShareFileService shareFileService;

        public BigFileService(AppDbContext dbContext, ShareFileService shareFileService)
        {
            this.dbContext = dbContext;
            this.shareFileService = shareFileService;
        }

        public async Task<Guid> StartUpload(string userId, string virtualPath)
        {
            InternalFile file = await shareFileService.GetFileItem(virtualPath, userId);

            if (!file.Permission.Write) throw new ForbiddenHttpException("No permission to create file", code: 4001);

            BigFileUpload upload = new BigFileUpload()
            {
                DestinationPath = file.PhysicalPath,
                TempPath = FileHelper.GenerateUniqueFileName(file.PhysicalPath),
                UserId = userId,
                LastActivity = DateTime.UtcNow,
            };

            await dbContext.BigFileUploads.AddAsync(upload);
            await dbContext.SaveChangesAsync();

            await System.IO.File.WriteAllBytesAsync(upload.TempPath, Array.Empty<byte>());

            return upload.Uuid;
        }

        private async Task<BigFileUpload> GetValidBigFileUpload(Guid uuid, string userId, bool validateFileExists = true)
        {
            BigFileUpload upload = await dbContext.BigFileUploads
              .FirstOrDefaultAsync(u => u.Uuid == uuid && u.UserId == userId)
              ?? throw new NotFoundHttpException("Upload not found.", code: 4002);

            if (validateFileExists && !System.IO.File.Exists(upload.TempPath))
            {
                throw new NotFoundHttpException("Temporary file not found.", code: 4003);
            }

            return upload;
        }

        public async Task AppendUpload(string userId, Guid uuid, IFormFile file)
        {
            BigFileUpload upload = await GetValidBigFileUpload(uuid, userId);

            await using FileStream dest = System.IO.File.Open(upload.TempPath, FileMode.Append);
            await file.CopyToAsync(dest);

            upload.LastActivity = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        public async Task FinishUpload(string userId, Guid uuid)
        {
            BigFileUpload upload = await GetValidBigFileUpload(uuid, userId);

            if (upload.TempPath != upload.DestinationPath)
                System.IO.File.Move(upload.TempPath, upload.DestinationPath, true);

            dbContext.BigFileUploads.Remove(upload);
            await dbContext.SaveChangesAsync();
        }

        public async Task CancelUpload(string userId, Guid uuid)
        {
            BigFileUpload upload = await GetValidBigFileUpload(uuid, userId, false);

            System.IO.File.Delete(upload.TempPath);

            dbContext.BigFileUploads.Remove(upload);
            await dbContext.SaveChangesAsync();
        }
    }
}
