using FileSystemCommon.Models.Share;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Internal;
using Microsoft.EntityFrameworkCore;
using StdOttStandard.Models.HttpExceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWeb.Services.Share
{
    public class ShareService
    {
        private readonly AppDbContext dbContext;
        private readonly ShareFileService shareFileService;
        private readonly ShareFolderService shareFolderService;

        public ShareService(AppDbContext dbContext, ShareFileService shareFileService, ShareFolderService shareFolderService)
        {
            this.dbContext = dbContext;
            this.shareFileService = shareFileService;
            this.shareFolderService = shareFolderService;
        }

        private async Task ValidateFileSystemItemShare(EditFileSystemItemShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) throw new BadRequestHttpException("Name missing", code: 8008);

            if (!string.IsNullOrWhiteSpace(body.UserId) && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                throw new BadRequestHttpException("User not found", code: 8009);
            }
        }

        private static bool HasPermission(FileSystemCommon.Models.FileSystem.Files.FileItemPermission hasPermission,
            FileSystemCommon.Models.FileSystem.Files.FileItemPermission givePermission)
        {
            return
                hasPermission.Info && givePermission.Info &&
                (hasPermission.Hash || !givePermission.Hash) &&
                (hasPermission.Read || !givePermission.Read) &&
                (hasPermission.Write || !givePermission.Write);
        }

        private async Task<InternalFile> ValidateAddFileShare(string userId, AddFileShareBody body)
        {
            await ValidateFileSystemItemShare(body);

            InternalFile file = await shareFileService.GetFileItem(body.Path, userId);

            if (!HasPermission(file.Permission, body.Permission))
            {
                throw new ForbiddenHttpException("Can't give more permissions.", code: 8010);
            }
            if (!System.IO.File.Exists(file.PhysicalPath))
            {
                throw new NotFoundHttpException("File not found", code: 8011);
            }

            return file;
        }

        public async Task<ShareFile> AddShareFile(string userId, AddFileShareBody body)
        {
            InternalFile file = await ValidateAddFileShare(userId, body);

            if (await dbContext.ShareFiles.AnyAsync(f => f.Name == body.Name && f.UserId == body.UserId))
            {
                throw new BadRequestHttpException("File with this name is already shared", code: 8012);
            }

            ShareFile shareFile = new ShareFile()
            {
                Name = body.Name,
                Path = file.PhysicalPath,
                IsListed = body.IsListed,
                UserId = string.IsNullOrWhiteSpace(body.UserId) ? null : body.UserId,
                Permission = FileItemPermission.New(body.Permission),
            };

            await dbContext.ShareFiles.AddAsync(shareFile);
            await dbContext.SaveChangesAsync();

            return shareFile;
        }

        public async Task<ShareItem[]> GetShareFiles()
        {
            ShareFile[] shareFiles = await dbContext.ShareFiles
              .Include(f => f.Permission)
              .ToArrayAsync();

            return shareFiles.Select(f => f.ToShareItem(f.GetExists())).ToArray();
        }

        public async Task<ShareItem> GetShareFile(Guid uuid)
        {
            ShareFile shareFile = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid)
                ?? throw new NotFoundHttpException("Share file not found", code: 8013);

            return shareFile.ToShareItem(shareFile.GetExists());
        }

        public async Task<ShareFile> EditShareFile(Guid uuid, EditFileSystemItemShareBody body)
        {
            await ValidateFileSystemItemShare(body);

            if (await dbContext.ShareFiles.AnyAsync(f =>
                f.Uuid != uuid && f.Name == body.Name && f.UserId == body.UserId))
            {
                throw new BadRequestHttpException("File with this name is already shared", code: 8014);
            }

            ShareFile shareFile = await dbContext.ShareFiles
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid)
                ?? throw new NotFoundHttpException("Share file not found", code: 8015);

            shareFile.Name = body.Name;
            shareFile.IsListed = body.IsListed;
            shareFile.UserId = string.IsNullOrWhiteSpace(body.UserId) ? null : body.UserId;

            await dbContext.SaveChangesAsync();

            return shareFile;
        }

        public async Task DeleteShareFile(Guid uuid)
        {
            ShareFile shareFile = await dbContext.ShareFiles
              .Include(f => f.Permission)
              .FirstOrDefaultAsync(f => f.Uuid == uuid)
              ?? throw new NotFoundHttpException("Share file not found", code: 8016);

            dbContext.ShareFiles.Remove(shareFile);
            await dbContext.SaveChangesAsync();
        }

        private static bool HasPermission(FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission hasPermission,
            FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission givePermission)
        {
            return
                hasPermission.Info && givePermission.Info &&
                (hasPermission.List || !givePermission.List) &&
                (hasPermission.Hash || !givePermission.Hash) &&
                (hasPermission.Read || !givePermission.Read) &&
                (hasPermission.Write || !givePermission.Write);
        }

        private async Task<InternalFolder> ValidateAddFolderShare(string userId, AddFolderShareBody body)
        {
            if (string.IsNullOrWhiteSpace(body.Name)) throw new BadRequestHttpException("Name missing", code: 8017);

            InternalFolder folder = await shareFolderService.GetFolderItem(body.Path, userId)
                ?? throw new NotFoundHttpException("Base not found", code: 8018);

            if (!HasPermission(folder.Permission, body.Permission))
            {
                throw new ForbiddenHttpException("Can't give more permissions.", code: 8019);
            }
            if (!string.IsNullOrWhiteSpace(folder.PhysicalPath) && !System.IO.Directory.Exists(folder.PhysicalPath))
            {
                throw new NotFoundHttpException("Folder not found", code: 8020);
            }
            if (body.UserId != null && !await dbContext.Users.AnyAsync(u => u.Id == body.UserId))
            {
                throw new BadRequestHttpException("User not found", code: 8021);
            }

            return folder;
        }

        public async Task<ShareFolder> AddFolderShare(string userId, AddFolderShareBody body)
        {
            InternalFolder folder = await ValidateAddFolderShare(userId, body);

            if (await dbContext.ShareFolders.AnyAsync(f => f.Name == body.Name && f.UserId == body.UserId))
            {
                throw new BadRequestHttpException("Folder with this name is already shared", code: 8022);
            }

            ShareFolder shareFolder = new ShareFolder()
            {
                Name = body.Name,
                Path = folder.PhysicalPath,
                IsListed = body.IsListed,
                UserId = body.UserId,
                Permission = FolderItemPermission.New(body.Permission),
            };

            await dbContext.ShareFolders.AddAsync(shareFolder);
            await dbContext.SaveChangesAsync();

            return shareFolder;
        }

        public async Task<ShareItem[]> GetShareFolders()
        {
            ShareFolder[] shareFolders = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .ToArrayAsync();

            return shareFolders.Select(f => f.ToShareItem(f.GetExists())).ToArray();
        }

        public async Task<ShareItem> GetShareFolder(Guid uuid)
        {
            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid)
                ?? throw new NotFoundHttpException("Share folder not found", code: 8023);

            return shareFolder.ToShareItem(shareFolder.GetExists());
        }

        public async Task<ShareFolder> EditFolderShare(Guid uuid, EditFileSystemItemShareBody body)
        {
            await ValidateFileSystemItemShare(body);

            if (await dbContext.ShareFolders.AnyAsync(f =>
                f.Uuid != uuid && f.Name == body.Name && f.UserId == body.UserId))
            {
                throw new BadRequestHttpException("Folder with this name is already shared", code: 8024);
            }

            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid)
                ?? throw new NotFoundHttpException("Share folder not found", code: 8025);

            shareFolder.Name = body.Name;
            shareFolder.IsListed = body.IsListed;
            shareFolder.UserId = body.UserId;

            await dbContext.SaveChangesAsync();

            return shareFolder;
        }

        public async Task DeleteShareFolder(Guid uuid)
        {
            ShareFolder shareFolder = await dbContext.ShareFolders
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Uuid == uuid)
                ?? throw new NotFoundHttpException("Share folder not found", code: 8026);

            dbContext.ShareFolders.Remove(shareFolder);
            await dbContext.SaveChangesAsync();
        }
    }
}
