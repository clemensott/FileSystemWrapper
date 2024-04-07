using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Views.FileViewer
{
    public record FileViewerOverlayUpdateModel(string Title, FileViewerOverlayModel FileViewerOverlay);

    static class FileViewerOverlayUpdateExtensions
    {
        const string partialViewName = "~/Views/FileViewer/FileViewerOverlayUpdate.cshtml";

        public static ViewResult ViewFileViewerOverlayUpdate(this Controller controller, FileViewerOverlayUpdateModel model)
        {
            return controller.View(partialViewName, model);
        }
    }
}
