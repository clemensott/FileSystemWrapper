using System.Net;

namespace FileSystemCommon.Models.FileSystem.Files.Many
{
    public struct FileInfoManyItem
    {
        public string FilePath { get; set; }

        public FileItemInfo? Info { get;  set;}

        public HttpStatusCode StatusCode { get;  set;}

        public string ErrorMessage { get; set; }

        public int? ErrorCode { get; set; }

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
