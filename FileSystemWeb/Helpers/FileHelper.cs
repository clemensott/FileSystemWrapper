using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemWeb.Models;

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
            if (!EndsWithNumberation(ref fileName, out nummeration)) nummeration = 0;

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
        private static bool EndsWithNumberation(ref string fileName, out int number)
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
                Name = info.Name,
                Extension = info.Extension,
                SharedId = file.SharedId,
                Permission = file.Permission.ToFileItemPermission(),
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
            int count;
            long size;
            GetFileCountAndSize(info, out count, out size);

            return new FolderItemInfo()
            {
                Name = info.Name,
                Path = folder.VirtualPath,
                SharedId = folder.SharedId,
                Permission = folder.Permission.ToFolderItemPermission(),
                Deletable = info.FullName != info.Root.FullName,
                FileCount = count,
                Size = size,
                LastAccessTime = info.LastAccessTime,
                LastWriteTime = info.LastWriteTime,
                CreationTime = info.CreationTime,
                Attributes = info.Attributes,
            };
        }

        public static void GetFileCountAndSize(DirectoryInfo dir, out int count, out long size)
        {
            count = 0;
            size = 0;

            try
            {
                foreach (FileInfo file in dir.EnumerateFiles())
                {
                    count++;
                    size += file.Length;
                }

                foreach (DirectoryInfo subDir in dir.EnumerateDirectories())
                {
                    int subCount;
                    long subSize;
                    GetFileCountAndSize(subDir, out subCount, out subSize);

                    count += subCount;
                    size += subSize;
                }
            }
            catch
            {
                count = -1;
                size = 0;
            }
        }

        public static PathPart[] GetPathParts(InternalFolder folder)
        {
            return GetPathParts(folder.VirtualPath, folder.BaseName);
        }

        public static PathPart[] GetPathParts(string virtualPath, string baseName)
        {
            string[] parts = virtualPath.TrimEnd('\\').Split('\\');
            return parts.Select((p, i) => new PathPart()
            {
                Name = i == 0 ? baseName : p,
                Path = string.Join(@"\", parts[..(i + 1)]),
            }).ToArray();
        }

        public static string ToFilePath(IEnumerable<string> paths)
        {
            return ToFilePath(Path.Join(paths.ToArray()));
        }

        public static string ToFilePath(string path)
        {
            return path?.TrimEnd('\\');
        }

        public static string ToFolderPath(IEnumerable<string> paths)
        {
            return ToFolderPath(Path.Join(paths.ToArray()));
        }

        public static string ToFolderPath(string path)
        {
            if (path == null) return null;

            path = path.TrimEnd('\\');
            return path.Length > 0 ? path + @"\\" : path;
        }
    }
}