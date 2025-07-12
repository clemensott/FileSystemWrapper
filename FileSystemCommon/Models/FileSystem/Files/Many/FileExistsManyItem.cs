using System.Net;

namespace FileSystemCommon.Models.FileSystem.Files.Many
{
    public struct FileExistsManyItem
    {
        public string FilePath { get; }

        public bool? Exists { get; }

        public HttpStatusCode StatusCode { get; }

        public string ErrorMessage { get; }

        public int? ErrorCode { get; }

        public FileExistsManyItem(string filePath, bool? exists, HttpStatusCode statusCode, string errorMessage, int? errorCode)
        {
            FilePath = filePath;
            Exists = exists;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }
}
