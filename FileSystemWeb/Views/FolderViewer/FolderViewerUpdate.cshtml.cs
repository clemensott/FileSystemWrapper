using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Views.FolderViewer
{
    public record FolderViewerUpdateModel(string Title, FolderViewerModel FolderViewer);

    static class FolderViewerUpdateExtensions
    {
        const string partialViewName = "~/Views/FolderViewer/FolderViewerUpdate.cshtml";

        public static ViewResult ViewFolderViewerUpdate(this Controller controller, FolderViewerUpdateModel model)
        {
            return controller.View(partialViewName, model);
        }
    }
}
