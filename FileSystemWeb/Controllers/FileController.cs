using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.Controllers.Base;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Services.File;
using FileSystemWeb.Views.File;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FileSystemWeb.Controllers
{
    [Route("[controller]")]
    public class FileController : LayoutController
    {
        private readonly FileService fileService;

        public FileController(FileService fileService)
        {
            this.fileService = fileService;
        }

        [HttpGet("view")]
        public async Task<IActionResult> GetView([FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            FileItemInfo file = await fileService.GetInfo(userId, path);

            Title = file.Name;
            
            return View("./View", new ViewModel(file));
        }
    }
}
