using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FileViewer
{
    public record FileViewerOverlayUpdaterModel(string ChildContent, string File);

    [HtmlTargetElement("file-viewer-overlay-updater-partial")]
    public class FileViewerOverlayUpdaterPartialTagHelper : BasePartialTagHelper<FileViewerOverlayUpdaterModel>
    {
        public string File { get; set; } = null;

        public FileViewerOverlayUpdaterPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FileViewer/FileViewerOverlayUpdater.cshtml";

        protected override FileViewerOverlayUpdaterModel CreateModel(string childContent)
        {
            return new FileViewerOverlayUpdaterModel(childContent, File);
        }
    }
}
