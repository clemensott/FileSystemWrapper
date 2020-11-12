using System;
using FileSystemCommon.Models.FileSystem;
using System.IO;
using System.Text;

namespace FileSystemCommon
{
    public static class Utils
    {
        /// <summary>
        /// Encodes a text with Unicode to Base64 but replaces '/' and '='. Is used for file oder folder paths to replace URI encoding.
        /// This has the advantage of URLs with no whitespaces and other special characters that need escacping.
        /// </summary>
        /// <param name="path">The path or text to encode.</param>
        /// <returns>The encoded text.</returns>
        public static string EncodePath(string path)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(path))
                .Replace('/', '_').Replace('=','!');
        }

        public static string DecodePath(string customBase64)
        {
            string base64 = customBase64.Replace('_', '/').Replace('!', '=');
            return Encoding.Unicode.GetString(Convert.FromBase64String(base64));
        }

        public static string GetContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".js":
                    return "text/javascript";
                case ".json":
                    return "text/json";
                case ".xml":
                    return "text/xml";
                case ".css":
                    return "text/css";
                case ".htm":
                case ".html":
                    return "text/html";
                case ".txt":
                case ".log":
                case ".ini":
                    return "text/plain";
                case ".rtx":
                    return "text/richtext";
                case ".rtf":
                    return "text/rtf";
                case ".tsv":
                    return "text/tab-separated-values";
                case ".csv":
                    return "text/csv";
                case ".zip":
                    return "application/zip";
                case ".pdf":
                    return "application/pdf";
                case ".aac":
                    return "audio/aac";
                case ".mp3":
                    return "audio/mpeg";
                case ".mp2":
                    return "audio/x-mpeg";
                case ".wav":
                    return "audio/wav";
                case ".oga":
                    return "audio/ogg";
                case ".mpeg":
                case ".mpg":
                case ".mpe":
                    return "video/mpeg";
                case ".mp4":
                    return "video/mp4";
                case ".ogg":
                case ".ogv":
                    return "video/ogg";
                case ".qt":
                case ".mov":
                    return "video/quicktime";
                case ".viv":
                case ".vivo":
                    return "video/vnd.vivo";
                case ".webm":
                case ".mkv":
                    return "video/webm";
                case ".avi":
                    return "video/x-msvideo";
                case ".movie":
                    return "video/x-sgi-movie";
                case ".3gp":
                    return "video/3gp";
                case ".3g2":
                    return "video/3gpp2";
                case ".apng":
                    return "image/apng";
                case ".bmp":
                    return "image/bmp";
                case ".gif":
                    return "image/gif";
                case ".ico":
                case ".cur":
                    return "image/x-ico";
                case ".jpg":
                case ".jpeg":
                case ".jfif":
                case ".pjpeg":
                case ".pjp":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".svg":
                    return "image/svg+xml";
                case ".tif":
                case ".tiff":
                    return "image/tiff";
                case ".webp":
                    return "image/webp";
            }

            return "application/octet-stream";
        }

        public static FileItemInfo GetInfo(FileInfo info)
        {
            return new FileItemInfo()
            {
                Name = info.Name,
                Extension = info.Extension,
                Size = info.Length,
                FullPath = info.FullName,
                LastAccessTime = info.LastAccessTime,
                LastWriteTime = info.LastWriteTime,
                CreationTime = info.CreationTime,
                Attributes = info.Attributes,
            };
        }

        public static FolderItemInfo GetInfo(DirectoryInfo info)
        {
            int count;
            long size;
            GetFileCountAndSize(info, out count, out size);

            return new FolderItemInfo()
            {
                Name = info.Name,
                FullPath = info.FullName,
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
    }
}