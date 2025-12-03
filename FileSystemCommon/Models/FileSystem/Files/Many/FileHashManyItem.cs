using System.Net;

namespace FileSystemCommon.Models.FileSystem.Files.Many
{
    public struct FileHashManyItem
    {
        public string FilePath { get; set; }

        public string Hash { get; set; }
        
        public HttpStatusCode StatusCode{ get; set; }
        
        public string ErrorMessage{ get; set; }
        
        public int? ErrorCode { get; set; }

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
