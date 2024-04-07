using FileSystemWeb.Views.FileViewer;
using FileSystemWeb.Views.FolderViewer;

namespace FileSystemWeb.Views.Home
{
    public record IndexModel(FolderViewerModel FolderViewer, FileViewerOverlayModel FileViewerOverlay);
}
