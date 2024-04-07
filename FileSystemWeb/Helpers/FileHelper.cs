using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Internal;
using StdOttStandard.Linq;

namespace FileSystemWeb.Helpers
{
    static class FileHelper
    {
        public static string GenerateUniqueFileName(string path)
        {
            if (!File.Exists(path)) return path;

            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            int nummeration;
            if (!EndsWithNumeration(ref fileName, out nummeration)) nummeration = 0;

            string newPath;
            do
            {
                string newFileName = $"{fileName} ({nummeration++}){extension}";
                newPath = Path.Combine(directoryName, newFileName);
            } while (File.Exists(newPath));

            return newPath;
        }

        /// <summary>
        /// Finds Numeration. Example "FileName (23)" would find 23.
        /// </summary>
        /// <returns>Returns true if find Numeration</returns>
        private static bool EndsWithNumeration(ref string fileName, out int number)
        {
            if (fileName.Length < 3 ||
                fileName[fileName.Length - 1] != ')' ||
                char.IsNumber(fileName[fileName.Length - 2]))
            {
                number = -1;
                return false;
            }

            string numberText = string.Empty;
            for (int i = fileName.Length - 2; i > 0; i--)
            {
                if (!char.IsNumber(fileName[i]) && (char.IsNumber(fileName[i - 1]) || fileName[i - 1] == '('))
                {
                    number = -1;
                    return false;
                }

                numberText = fileName[i] + numberText;
            }

            number = int.Parse(numberText);
            fileName = fileName.Remove(fileName.Length - numberText.Length);
            return true;
        }

        public static FileItemInfo GetInfo(InternalFile file, FileInfo info)
        {
            return new FileItemInfo()
            {
                Name = file.Name,
                Extension = info.Extension,
                SharedId = file.SharedId,
                Permission = file.Permission,
                Size = info.Length,
                Path = file.VirtualPath,
                LastAccessTime = info.LastAccessTime,
                LastWriteTime = info.LastWriteTime,
                CreationTime = info.CreationTime,
                Attributes = info.Attributes,
            };
        }

        public static FolderItemInfo GetInfo(InternalFolder folder, DirectoryInfo info)
        {
            return new FolderItemInfo()
            {
                Name = folder.Name,
                Path = folder.VirtualPath,
                SharedId = folder.SharedId,
                Permission = folder.Permission,
                Deletable = info != null && info.FullName != info.Root.FullName,
                LastAccessTime = info?.LastAccessTime ?? DateTime.MinValue,
                LastWriteTime = info?.LastWriteTime ?? DateTime.MinValue,
                CreationTime = info?.CreationTime ?? DateTime.MinValue,
                Attributes = info?.Attributes ?? FileAttributes.Directory,
            };
        }

        public static FolderItemInfoWithSize GetInfoWithSize(InternalFolder folder, DirectoryInfo info)
        {
            int count;
            long size;
            if (info == null) GetFileCountAndSize(out count, out size);
            else GetFileCountAndSize(info, out count, out size);

            return new FolderItemInfoWithSize()
            {
                Name = folder.Name,
                Path = folder.VirtualPath,
                SharedId = folder.SharedId,
                Permission = folder.Permission,
                Deletable = info != null && info.FullName != info.Root.FullName,
                FileCount = count,
                Size = size,
                LastAccessTime = info?.LastAccessTime ?? DateTime.MinValue,
                LastWriteTime = info?.LastWriteTime ?? DateTime.MinValue,
                CreationTime = info?.CreationTime ?? DateTime.MinValue,
                Attributes = info?.Attributes ?? FileAttributes.Directory,
            };
        }

        private static void GetFileCountAndSize(out int count, out long size)
        {
            GetFileCountAndSize(DriveInfo.GetDrives().Select(d => d.RootDirectory), out count, out size);
        }

        private static void GetFileCountAndSize(DirectoryInfo dir, out int count, out long size)
        {
            GetFileCountAndSize(new DirectoryInfo[] { dir }, out count, out size);
        }

        private static void GetFileCountAndSize(IEnumerable<DirectoryInfo> dirs, out int count, out long size)
        {
            count = 0;
            size = 0;

            Queue<DirectoryInfo> queue = new Queue<DirectoryInfo>(dirs);
            while (queue.Count > 0)
            {
                DirectoryInfo dir = queue.Dequeue();
                try
                {
                    foreach (FileInfo file in dir.EnumerateFiles())
                    {
                        count++;
                        size += file.Length;
                    }

                    foreach (DirectoryInfo subDir in dir.EnumerateDirectories())
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch { }
            }
        }

        public static PathPart[] GetPathParts(InternalFolder folder)
        {
            return GetPathParts(folder.VirtualPath, folder.BaseName);
        }

        public static PathPart[] GetPathParts(string virtualPath, string baseName)
        {
            char directorySeparatorChar = ConfigHelper.Public.DirectorySeparatorChar;
            string[] parts = virtualPath.TrimEnd(directorySeparatorChar).Split(directorySeparatorChar);
            return parts.Select((p, i) => new PathPart()
            {
                Name = i == 0 ? baseName : p,
                Path = string.Join(Path.DirectorySeparatorChar, parts[..(i + 1)]),
            }).ToArray();
        }

        public static string ToFilePath(IEnumerable<string> paths)
        {
            return ToFilePath(Path.Join(paths.ToArray()));
        }

        public static string ToFilePath(string path)
        {
            return path?.TrimEnd(ConfigHelper.Public.DirectorySeparatorChar);
        }

        public static string ToPhysicalFolderPath(IEnumerable<string> paths)
        {
            return ToPhysicalFolderPath(Path.Join(paths.ToArray()));
        }

        public static string ToPhysicalFolderPath(string path)
        {
            if (path == null) return null;
            if (path.Length == 0) return string.Empty;

            path = path.TrimEnd(ConfigHelper.Public.DirectorySeparatorChar);
            return path + ConfigHelper.Public.DirectorySeparatorChar;
        }

        public static bool IsPathAllowed(string path)
        {
            return Path.IsPathFullyQualified(path) &&
                   !path.Contains($"{ConfigHelper.Public.DirectorySeparatorChar}..{ConfigHelper.Public.DirectorySeparatorChar}");
        }

        public static string GetPath(this ShareFolder folder)
        {
            return string.IsNullOrWhiteSpace(folder.Path) ? ConfigHelper.RootPath : folder.Path;
        }

        public static bool GetExists(this ShareFolder folder)
        {
            string path = folder.GetPath();
            return string.IsNullOrWhiteSpace(path) || Directory.Exists(path);
        }

        public static bool GetExists(this ShareFile file)
        {
            string path = file.Path;
            return string.IsNullOrWhiteSpace(path) || File.Exists(path);
        }

        public static (IFileItem previous, IFileItem next) GetFileSiblings(FileSortItem[] files, IFileItem file)
        {
            if (files.Length <= 1) return (null, null);

            int index = files.IndexOf(f => f.Path == file.Path);
            IFileItem previous = files[(index - 1 + files.Length) % files.Length];
            IFileItem next = files[(index + 1) % files.Length];

            return (previous, next);
        }
    }
}
