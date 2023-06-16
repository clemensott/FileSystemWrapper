using Microsoft.AspNetCore.Http;

namespace FileSystemWeb.Models.RequestBodies
{
    public class AppendBigFileBody
    {
        public IFormFile PartialFile { get; set; }
    }
}
