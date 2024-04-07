using FileSystemCommon;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Services.Share;
using Microsoft.AspNetCore.Http;
using StdOttStandard.Models.HttpExceptions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FileSystemWeb.Services.File
{
    public class FileService
    {
        private readonly ShareFileService shareFileService;

        public FileService(ShareFileService shareFileService)
        {
            this.shareFileService = shareFileService;
        }

        public async Task<InternalFile> Get(string userId, string virtualPath)
        {
            InternalFile file = await shareFileService.GetFileItem(virtualPath, userId);

            if (!file.Permission.Read) throw new ForbiddenHttpException("No permission to read file", code: 4004);
            if (!System.IO.File.Exists(file.PhysicalPath)) throw new NotFoundHttpException("File not found.", code: 4005);

            return file;
        }

        public async Task<bool> Exists(string userId, string virtualPath)
        {
            InternalFile file;
            try
            {
                file = await shareFileService.GetFileItem(virtualPath, userId);
            }
            catch (HttpException)
            {
                return false;
            }

            if (!file.Permission.Info) throw new ForbiddenHttpException("No permission to get file info", code: 4006);

            return System.IO.File.Exists(file.PhysicalPath);
        }

        public async Task<FileItemInfo> GetInfo(string userId, string virtualPath)
        {
            InternalFile file = await shareFileService.GetFileItem(virtualPath, userId);

            if (!file.Permission.Info) throw new ForbiddenHttpException("No permission to read file", code: 4007);

            try
            {
                FileInfo info = new FileInfo(file.PhysicalPath);
                if (!info.Exists) throw new NotFoundHttpException("File not found.", code: 4008);
                return FileHelper.GetInfo(file, info);
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundHttpException("File not found.", code: 4009);
            }
        }

        public async Task<string> GetHash(string userId, string virtualPath, int partialSize)
        {
            InternalFile file = await shareFileService.GetFileItem(virtualPath, userId);

            if (!file.Permission.Hash) throw new ForbiddenHttpException("No permission to hash file", code: 4010);

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
                throw new NotFoundHttpException("File not found.", code: 4011);
            }
        }

        public async Task Copy(string userId, string virtualSrcPath, string virtualDestPath)
        {
            InternalFile srcFile = await shareFileService.GetFileItem(virtualSrcPath, userId);
            InternalFile destFile = await shareFileService.GetFileItem(virtualDestPath, userId);

            if (!srcFile.Permission.Read) throw new ForbiddenHttpException("No permission to read src file", code: 4012);
            if (!destFile.Permission.Write) throw new ForbiddenHttpException("No permission to write dest file", code: 4013);

            try
            {
                await using Stream source = System.IO.File.OpenRead(srcFile.PhysicalPath);
                await using Stream destination = System.IO.File.Create(destFile.PhysicalPath);
                await source.CopyToAsync(destination);
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundHttpException("File not found.", code: 4014);
            }
        }

        public async Task Move(string userId, string virtualSrcPath, string virtualDestPath)
        {
            InternalFile srcFile = await shareFileService.GetFileItem(virtualSrcPath, userId);
            InternalFile destFile = await shareFileService.GetFileItem(virtualDestPath, userId);

            if (!srcFile.Permission.Write) throw new ForbiddenHttpException("No permission to delete src file", code: 4015);
            if (!destFile.Permission.Write) throw new ForbiddenHttpException("No permission to write dest file", code: 4016);

            try
            {
                await Task.Run(() => System.IO.File.Move(srcFile.PhysicalPath, destFile.PhysicalPath));
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundHttpException("File not found.", code: 4017);
            }
        }

        public async Task Write(string userId, string virtualPath, IFormFile file)
        {
            InternalFile internalFile = await shareFileService.GetFileItem(virtualPath, userId);

            if (!internalFile.Permission.Write) throw new ForbiddenHttpException("No permission to write file", code: 4018);

            string physicalPath = internalFile.PhysicalPath;
            string tmpPath = FileHelper.GenerateUniqueFileName(physicalPath);

            try
            {
                await using (FileStream dest = System.IO.File.Create(tmpPath))
                {
                    await file.CopyToAsync(dest);
                }

                if (tmpPath != physicalPath) System.IO.File.Move(tmpPath, physicalPath, true);
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
        }

        public async Task Delete(string userId, string virtualPath)
        {
            InternalFile file = await shareFileService.GetFileItem(virtualPath, userId);

            if (!file.Permission.Write) throw new ForbiddenHttpException("No permission to delete file", code: 4019);

            try
            {
                System.IO.File.Delete(file.PhysicalPath);
            }
            catch (FileNotFoundException)
            {
                throw new NotFoundHttpException("File not found.", code: 4020);
            }
        }
    }
}
