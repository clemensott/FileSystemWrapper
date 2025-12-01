using System.Net;

namespace FileSystemCommon.Models.FileSystem.Files.Many
{
    public struct FileExistsManyItem
    {
        public string FilePath { get; set; }

        public bool? Exists { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public int? ErrorCode { get; set; }

        public FileExistsManyItem(string filePath, bool? exists, HttpStatusCode statusCode, string errorMessage,
            int? errorCode)
        {
            FilePath = filePath;
            Exists = exists;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }
}