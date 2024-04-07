using FileSystemCommon.Models.FileSystem.Content;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Services.File;
using FileSystemWeb.Services.Folder;
using FileSystemWeb.Views.FolderViewer;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWeb.Controllers
{
    [Route("[controller]")]
    public class FolderViewerController : Controller
    {
        private readonly FolderService folderService;

        public FolderViewerController(FolderService folderService)
        {
            this.folderService = folderService;
        }

        [HttpGet("folderViewer")]
        public async Task<IActionResult> GetFolderViewer([FromQuery] string folder,
            [FromQuery] FileSystemItemSortType sortType = FileSystemItemSortType.Name,
            [FromQuery] FileSystemItemSortDirection sortDirection = FileSystemItemSortDirection.ASC)
        {
            string userId = HttpContext.GetUserId();
            FolderContent folderContent = await folderService.GetContent(userId, folder, sortType, sortDirection);

            string title = folderContent.Path.Length > 0 ? folderContent.Path.Last().Name : "Root";

            FolderSortButtonModel folderSortButtonModel = new FolderSortButtonModel(new FileSystemItemSortBy()
            {
                Type = sortType,
                Direction = sortDirection,
            });
            FolderViewerModel folderViewerModel = new FolderViewerModel(folderSortButtonModel, folderContent);
            FolderViewerUpdateModel folderViewerUpdateModel = new FolderViewerUpdateModel(title, folderViewerModel);
            return this.ViewFolderViewerUpdate(folderViewerUpdateModel);
        }
    }
}
