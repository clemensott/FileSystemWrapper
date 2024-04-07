using FileSystemCommon.Models.FileSystem.Content;
using FileSystemWeb.Models.Internal;
using System.IO;
using System.Threading.Tasks;
using StdOttStandard.Models.HttpExceptions;
using FileSystemCommon.Models.FileSystem.Folders;
using System;
using FileSystemWeb.Services.Share;
using FileSystemWeb.Helpers;

namespace FileSystemWeb.Services.Folder
{
    public class FolderService
    {
        private readonly FolderContentService folderContentService;
        private readonly ShareFolderService shareFolderService;

        public FolderService(FolderContentService folderContentService, ShareFolderService shareFolderService)
        {
            this.folderContentService = folderContentService;
            this.shareFolderService = shareFolderService;
        }

        public async Task<bool> Exists(string userId, string virtualPath)
        {
            InternalFolder folder;
            try
            {
                folder = await shareFolderService.GetFolderItem(virtualPath, userId);
            }
            catch (HttpException)
            {
                return false;
            }

            return folder.Permission.Read &&
                   (string.IsNullOrWhiteSpace(folder.PhysicalPath) || Directory.Exists(folder.PhysicalPath));
        }

        public async Task<FolderContent> GetContent(string userId, string virtualPath, FileSystemItemSortType sortType, FileSystemItemSortDirection sortDirection)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return await folderContentService.FromRoot(userId, sortType, sortDirection);
            }

            InternalFolder folder = await shareFolderService.GetFolderItem(virtualPath, userId);
            if (!folder.Permission.List) throw new ForbiddenHttpException("No permission to list content", code: 3002);

            try
            {
                return FolderContentService.FromFolder(folder, sortType, sortDirection);
            }
            catch (DirectoryNotFoundException)
            {
                throw new NotFoundHttpException("Directory not found", code: 3003);
            }
        }

        public Task<FolderItemInfo> GetInfo(string userId, string virtualPath)
        {
            return GetInfo(userId, virtualPath, FileHelper.GetInfo);
        }

        public Task<FolderItemInfoWithSize> GetInfoWithSize(string userId, string virtualPath)
        {
            return GetInfo(userId, virtualPath, FileHelper.GetInfoWithSize);
        }

        public async Task<T> GetInfo<T>(string userId, string virtualPath, Func<InternalFolder, DirectoryInfo, T> transformer)
        {
            InternalFolder folder = await shareFolderService.GetFolderItem(virtualPath, userId);

            if (!folder.Permission.Info) throw new ForbiddenHttpException("No permission to get info of folder", code: 3006);

            try
            {
                DirectoryInfo info = null;
                if (!string.IsNullOrWhiteSpace(folder.PhysicalPath))
                {
                    info = new DirectoryInfo(folder.PhysicalPath);
                    if (!info.Exists) throw new NotFoundHttpException("Directory not found", code: 3007);
                }

                return transformer(folder, info);
            }
            catch (DirectoryNotFoundException)
            {
                throw new NotFoundHttpException("Directory not found", code: 3008);
            }
        }

        public async Task<FolderItemInfo> Create(string userId, string virtualPath)
        {
            InternalFolder folder = await shareFolderService.GetFolderItem(virtualPath, userId);

            if (!folder.Permission.Write) throw new ForbiddenHttpException("No permission to create folder", code: 3010);

            DirectoryInfo info = Directory.CreateDirectory(folder.PhysicalPath);
            return FileHelper.GetInfo(folder, info);
        }

        public async Task Delete(string userId, string virtualPath, bool recursive)
        {
            InternalFolder folder = await shareFolderService.GetFolderItem(virtualPath, userId);

            if (!folder.Permission.Write) throw new ForbiddenHttpException("No permission to delete folder", code: 3012);

            try
            {
                await Task.Run(() => Directory.Delete(folder.PhysicalPath, recursive));
            }
            catch (DirectoryNotFoundException)
            {
                throw new NotFoundHttpException("Directory not found", code: 3013);
            }
        }
    }
}
