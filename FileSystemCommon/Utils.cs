using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileSystemCommon.Models.FileSystem;

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
                .Replace('/', '_').Replace('=', '!');
        }

        public static string DecodePath(string customBase64)
        {
            try
            {
                if (customBase64 == null) return null;
                string base64 = customBase64.Replace('_', '/').Replace('!', '=');
                return Encoding.Unicode.GetString(Convert.FromBase64String(base64));
            }
            catch
            {
                return null;
            }
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

        public static string ToPath(this IEnumerable<PathPart> parts)
        {
            return parts?.LastOrDefault().Path ?? string.Empty;
        }

        public static string JoinPaths(params string[] paths)
        {
            return JoinPaths((IEnumerable<string>)paths);
        }

        public static string JoinPaths(IEnumerable<string> paths)
        {
            IEnumerable<string> parts = paths?.Select(TrimPath).Where(p => !string.IsNullOrEmpty(p));
            return parts == null
                ? string.Empty
                : string.Join(Path.DirectorySeparatorChar.ToString(), parts);

            string TrimPath(string path)
            {
                return path?.Trim(' ', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        public static string[] SplitPath(string path)
        {
            return path
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .Split(Path.DirectorySeparatorChar);
        }

        public static string GetParentPath(string path)
        {
            int index = path.LastIndexOf(Path.DirectorySeparatorChar);
            return index == -1 ? string.Empty : path.Substring(0, index + 1);
        }

        public static IEnumerable<PathPart> GetChildPathParts(this PathPart[] parentPath, IPathItem item)
        {
            return parentPath.Concat(new PathPart[]
            {
                new PathPart()
                {
                    Name = item.Name,
                    Path = item.Path,
                },
            });
        }

        public static string ReplaceNonAscii(string text, char replace = '_')
        {
            StringBuilder builder = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                builder.Append(Convert.ToInt32(c) < 128 ? c : replace);
            }

            return builder.ToString();
        }

        public static string FormatSizeSortable(long size)
        {
            return size.ToString().PadLeft(15, '0');
        }
    }
}
