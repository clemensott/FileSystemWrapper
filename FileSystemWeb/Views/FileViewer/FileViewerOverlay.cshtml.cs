using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FileViewer
{
    public record FileViewerOverlayModel(FileItemInfo? File, IFileItem PreviousFile, IFileItem NextFile);

    [HtmlTargetElement("file-viewer-overlay-partial")]
    public class FileViewerOverlayPartialTagHelper : BasePartialTagHelper<FileViewerOverlayModel>
    {
        public FileItemInfo? File { get; set; }

        public IFileItem PreviousFile { get; set; }

        public IFileItem NextFile { get; set; }

        public FileViewerOverlayPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FileViewer/FileViewerOverlay.cshtml";

        protected override FileViewerOverlayModel CreateModel(string _)
        {
            return new FileViewerOverlayModel(File, PreviousFile, NextFile);
        }
    }
}
