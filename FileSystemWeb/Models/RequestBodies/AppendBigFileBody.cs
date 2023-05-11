using Microsoft.AspNetCore.Http;

namespace FileSystemWeb.Models.RequestBodies
{
    public class AppendBigFileBody
    {
        public IFormFile File { get; set; }

        public byte[] Data { get; set; }
    }
}
