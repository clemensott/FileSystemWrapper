using System.Net;

namespace FileSystemCommon.Models.FileSystem.Files.Many
{
    public struct FileHashManyItem
    {
        public string FilePath { get; }

        public string Hash { get; }
        
        public HttpStatusCode StatusCode{ get; }
        
        public string ErrorMessage{ get; }
        
        public int? ErrorCode { get; }

        public FileHashManyItem(string filePath, string hash, HttpStatusCode statusCode, string errorMessage, int? errorCode)
        {
            FilePath = filePath;
            Hash = hash;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }
}
