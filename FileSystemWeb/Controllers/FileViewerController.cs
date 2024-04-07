using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Helpers;
using FileSystemWeb.Services.File;
using FileSystemWeb.Services.Folder;
using FileSystemWeb.Views.FileViewer;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemWeb.Controllers
{
    [Route("[controller]")]
    public class FileViewerController : Controller
    {
        private readonly FolderService folderService;
        private readonly FileService fileService;

        public FileViewerController(FolderService folderService, FileService fileService)
        {
            this.folderService = folderService;
            this.fileService = fileService;
        }

        [HttpGet("fileViewerOverlay")]
        public async Task<IActionResult> GetFileViewerOverlay([FromQuery] string folder, [FromQuery] string file,
            [FromQuery] FileSystemItemSortType sortType = FileSystemItemSortType.Name,
            [FromQuery] FileSystemItemSortDirection sortDirection = FileSystemItemSortDirection.ASC)
        {
            string userId = HttpContext.GetUserId();
            FolderContent folderContent = await folderService.GetContent(userId, folder, sortType, sortDirection);

            string title;
            FileViewerOverlayModel fileViewerOverlayModel;
            if (string.IsNullOrWhiteSpace(file))
            {
                title = folderContent.Path.Length > 0 ? folderContent.Path.Last().Name : "Root";
                fileViewerOverlayModel = null;
            }
            else
            {
                string virtualFilePath = Path.Combine(folder, file);
                FileItemInfo fileInfo = await fileService.GetInfo(userId, virtualFilePath);
                (IFileItem previousFile, IFileItem nextFile) = FileHelper.GetFileSiblings(folderContent.Files, fileInfo);

                title = fileInfo.Name;
                fileViewerOverlayModel = new FileViewerOverlayModel(fileInfo, previousFile, nextFile);
            }

            FileViewerOverlayUpdateModel fileViewerOverlayUpdateModel = new FileViewerOverlayUpdateModel(title, fileViewerOverlayModel);
            return this.ViewFileViewerOverlayUpdate(fileViewerOverlayUpdateModel);
        }
    }
}
