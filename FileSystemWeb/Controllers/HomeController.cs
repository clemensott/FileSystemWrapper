using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.Controllers.Base;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Helpers;
using FileSystemWeb.Services.File;
using FileSystemWeb.Services.Folder;
using FileSystemWeb.Views.FileViewer;
using FileSystemWeb.Views.FolderViewer;
using FileSystemWeb.Views.Home;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWeb.Controllers
{
    [Route("")]
    [Route("[controller]")]
    public class HomeController : LayoutController
    {
        private readonly FolderService folderService;
        private readonly FileService fileService;

        public HomeController(FolderService folderService, FileService fileService)
        {
            this.folderService = folderService;
            this.fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string folder, [FromQuery] string file,
            [FromQuery] FileSystemItemSortType sortType = FileSystemItemSortType.Name,
            [FromQuery] FileSystemItemSortDirection sortDirection = FileSystemItemSortDirection.ASC)
        {
            string userId = HttpContext.GetUserId();
            FolderContent folderContent = await folderService.GetContent(userId, folder, sortType, sortDirection);
            FolderSortButtonModel folderSortButtonModel = new FolderSortButtonModel(new FileSystemItemSortBy()
            {
                Type = sortType,
                Direction = sortDirection,
            });
            FolderViewerModel folderViewerModel = new FolderViewerModel(folderSortButtonModel, folderContent);

            FileViewerOverlayModel fileViewerOverlayModel;
            if (!string.IsNullOrWhiteSpace(file))
            {
                string virtualFilePath = Path.Combine(folder, file);
                FileItemInfo fileInfo = await fileService.GetInfo(userId, virtualFilePath);
                (IFileItem previousFile, IFileItem nextFile) = FileHelper.GetFileSiblings(folderContent.Files, fileInfo);
                fileViewerOverlayModel = new FileViewerOverlayModel(fileInfo, previousFile, nextFile);

                Title = fileInfo.Name;
            }
            else
            {
                fileViewerOverlayModel = null;
                Title = folderContent.Path.Length > 0 ? folderContent.Path.Last().Name : "Root";
            }

            IndexModel indexModel = new IndexModel(folderViewerModel, fileViewerOverlayModel);
            return View(indexModel);
        }
    }
}
