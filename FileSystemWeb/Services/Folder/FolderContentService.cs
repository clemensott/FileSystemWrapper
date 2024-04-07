using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemWeb.Data;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWeb.Services.Folder
{
    public class FolderContentService
    {
        private readonly AppDbContext dbContext;

        public FolderContentService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<FolderContent> FromRoot(string userId, FileSystemItemSortType sortType, FileSystemItemSortDirection sortDirection)
        {
            ShareFolder[] shareFolders = await dbContext.ShareFolders
                .Where(f => f.IsListed && (f.UserId == null || f.UserId == userId))
                .Include(f => f.Permission)
                .ToArrayAsync();
            IEnumerable<FolderSortItem> folders = GetFolderItems(shareFolders, sortType);

            ShareFile[] shareFiles = await dbContext.ShareFiles
                .Where(f => f.IsListed && (f.UserId == null || f.UserId == userId))
                .Include(f => f.Permission)
                .ToArrayAsync();
            IEnumerable<FileSortItem> files = GetFileItems(shareFiles, sortType);


            return new FolderContent()
            {
                Path = new PathPart[0],
                Permission = new FileSystemCommon.Models.FileSystem.Folders.FolderItemPermission()
                {
                    Read = false,
                    List = true,
                    Info = false,
                    Hash = false,
                    Write = false,
                },
                SortKeys = new string[0],
                Folders = FileSystemSortItemComparer.Current.Sort(folders, sortDirection).ToArray(),
                Files = FileSystemSortItemComparer.Current.Sort(files, sortDirection).ToArray(),
            };
        }

        private static IEnumerable<FolderSortItem> GetFolderItems(IEnumerable<ShareFolder> shareFolders,
            FileSystemItemSortType sortType)
        {
            if (sortType == FileSystemItemSortType.Name)
            {
                return shareFolders
                    .Where(f => f.GetExists())
                    .Select(f => f.ToFolderItem())
                    .Select(f => FolderSortItem.FromItem(f, f.Name));
            }

            return shareFolders
                 .Select(f =>
                 {
                     string path = f.GetPath();
                     if (string.IsNullOrWhiteSpace(path))
                     {
                         return FolderSortItem.FromItem(f.ToFolderItem(), "0");
                     }

                     DirectoryInfo dir = new DirectoryInfo(path);
                     if (!dir.Exists) return (FolderSortItem?)null;

                     FolderItem folder = f.ToFolderItem();
                     return FolderSortItem.FromItem(folder, GetDirectorySortKey(dir, sortType));
                 })
                 .Where(f => f.HasValue)
                 .Cast<FolderSortItem>();
        }

        private static IEnumerable<FileSortItem> GetFileItems(IEnumerable<ShareFile> shareFiles,
            FileSystemItemSortType sortType)
        {
            if (sortType == FileSystemItemSortType.Name)
            {
                return shareFiles
                    .Where(f => System.IO.File.Exists(f.Path))
                    .Select(f => f.ToFileItem())
                    .Select(f => FileSortItem.FromItem(f, f.Name));
            }

            return shareFiles
                 .Select(f =>
                 {
                     FileInfo file = new FileInfo(f.Path);
                     if (!file.Exists) return (FileSortItem?)null;

                     FileItem fileItem = f.ToFileItem();
                     return FileSortItem.FromItem(fileItem, GetFileSortKey(file, sortType));
                 })
                 .Where(f => f.HasValue)
                 .Cast<FileSortItem>();
        }

        public static FolderContent FromFolder(InternalFolder folder,
              FileSystemItemSortType sortType, FileSystemItemSortDirection sortDirection)
        {
            IEnumerable<FolderSortItem> folders = GetFolders(folder, sortType);
            IEnumerable<FileSortItem> files = GetFiles(folder, sortType);

            return new FolderContent()
            {
                Path = FileHelper.GetPathParts(folder),
                Permission = folder.Permission,
                SortKeys = new string[] {
                    GetDirectorySortKey(folder, sortType),
                },
                Folders = FileSystemSortItemComparer.Current.Sort(folders, sortDirection).ToArray(),
                Files = FileSystemSortItemComparer.Current.Sort(files, sortDirection).ToArray(),
            };
        }

        private static IEnumerable<FolderSortItem> GetFolders(InternalFolder folder,
              FileSystemItemSortType sortType)
        {
            char directorySeparatorChar = ConfigHelper.Public.DirectorySeparatorChar;
            if (string.IsNullOrWhiteSpace(folder.PhysicalPath))
            {
                return DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => new FolderSortItem()
                    {
                        Name = d.Name,
                        Path = $"{folder.VirtualPath}{directorySeparatorChar}{d.Name}",
                        SharedId = null,
                        Permission = folder.Permission,
                        Deletable = false,
                        SortKeys = new string[] {
                            GetDirectorySortKey(d.Name, sortType),
                        },
                    });
            }

            if (sortType == FileSystemItemSortType.Name)
            {
                return Directory.EnumerateDirectories(folder.PhysicalPath).Select(p =>
                {
                    string name = Path.GetFileName(p);
                    return new FolderSortItem()
                    {
                        Name = name,
                        Path = $"{folder.VirtualPath}{directorySeparatorChar}{name}",
                        SharedId = null,
                        Permission = folder.Permission,
                        Deletable = true,
                        SortKeys = new string[] { name },
                    };
                });
            }

            return new DirectoryInfo(folder.PhysicalPath).GetDirectories().Select(d => new FolderSortItem()
            {
                Name = d.Name,
                Path = Path.Join(folder.VirtualPath, d.Name),
                SharedId = null,
                Permission = folder.Permission,
                Deletable = true,
                SortKeys = new string[] {
                    GetDirectorySortKey(d, sortType),
                },
            });
        }

        private static IEnumerable<FileSortItem> GetFiles(InternalFolder folder, FileSystemItemSortType sortType)
        {
            if (string.IsNullOrWhiteSpace(folder.PhysicalPath)) return new FileSortItem[0];

            if (sortType == FileSystemItemSortType.Name)
            {
                return Directory.EnumerateFiles(folder.PhysicalPath).Select(path =>
                {
                    string name = Path.GetFileName(path);
                    return new FileSortItem()
                    {
                        Name = name,
                        Extension = Path.GetExtension(path),
                        Path = Path.Join(folder.VirtualPath, name),
                        SharedId = null,
                        Permission = (FileSystemCommon.Models.FileSystem.Files.FileItemPermission)folder.Permission,
                        SortKeys = new string[] { name },
                    };
                });
            }

            return new DirectoryInfo(folder.PhysicalPath).GetFiles().Select(f => new FileSortItem()
            {
                Name = f.Name,
                Extension = f.Extension,
                Path = Path.Join(folder.VirtualPath, f.Name),
                SharedId = null,
                Permission = (FileSystemCommon.Models.FileSystem.Files.FileItemPermission)folder.Permission,
                SortKeys = new string[] { GetFileSortKey(f, sortType) },
            });
        }

        private static string GetDirectorySortKey(InternalFolder folder, FileSystemItemSortType sortType)
        {
            if (string.IsNullOrWhiteSpace(folder.PhysicalPath))
            {
                return sortType == FileSystemItemSortType.Name ? folder.Name : "0";
            }

            return GetDirectorySortKey(folder.PhysicalPath, sortType);
        }

        private static string GetDirectorySortKey(string path, FileSystemItemSortType sortType)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            return GetDirectorySortKey(directory, sortType);
        }

        private static string GetDirectorySortKey(DirectoryInfo directory, FileSystemItemSortType sortType)
        {
            switch (sortType)
            {
                case FileSystemItemSortType.Name:
                    return directory.Name;

                case FileSystemItemSortType.Size:
                    return "0";

                case FileSystemItemSortType.LastWriteTime:
                    return directory.LastWriteTimeUtc.ToString("o", CultureInfo.InvariantCulture);

                case FileSystemItemSortType.LastAccessTime:
                    return directory.LastAccessTimeUtc.ToString("o", CultureInfo.InvariantCulture);

                case FileSystemItemSortType.CreationTime:
                    return directory.CreationTimeUtc.ToString("o", CultureInfo.InvariantCulture);

                default:
                    throw new ArgumentException("Not implemented", nameof(sortType));
            }
        }

        private static string GetFileSortKey(FileInfo file, FileSystemItemSortType sortType)
        {
            switch (sortType)
            {
                case FileSystemItemSortType.Name:
                    return file.Name;

                case FileSystemItemSortType.Size:
                    return Utils.FormatSizeSortable(file.Length);

                case FileSystemItemSortType.LastWriteTime:
                    return file.LastWriteTimeUtc.ToString("o", CultureInfo.InvariantCulture);

                case FileSystemItemSortType.LastAccessTime:
                    return file.LastAccessTimeUtc.ToString("o", CultureInfo.InvariantCulture);

                case FileSystemItemSortType.CreationTime:
                    return file.CreationTimeUtc.ToString("o", CultureInfo.InvariantCulture);

                default:
                    throw new ArgumentException("Not implemented", nameof(sortType));
            }
        }
    }
}
