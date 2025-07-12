using System.Net;

namespace FileSystemCommon.Models.FileSystem.Files.Many
{
    public struct FileInfoManyItem
    {
        public string FilePath { get; }

        public FileItemInfo? Info { get; }

        public HttpStatusCode StatusCode { get; }

        public string ErrorMessage { get; }

        public int? ErrorCode { get; }

        public FileInfoManyItem(string filePath, FileItemInfo? info, HttpStatusCode statusCode, string errorMessage, int? errorCode)
        {
            FilePath = filePath;
            Info = info;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }
}
