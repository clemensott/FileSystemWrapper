using Microsoft.AspNetCore.Http;

namespace FileSystemWeb.Models.RequestBodies
{
    public class AppendBigFileBody
    {
        public IFormFile Data { get; set; }
    }
}
