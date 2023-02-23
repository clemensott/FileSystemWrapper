using Microsoft.AspNetCore.Http;

namespace FileSystemWeb.Models.RequestBodies
{
    public class WriteFileBody
    {
        public IFormFile FileContent { get; set; }
    }
}
