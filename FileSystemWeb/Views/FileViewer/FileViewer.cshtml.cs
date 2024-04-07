using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FileViewer
{
    public enum FileViewerTheme
    {
        Light,
        Dark,
    }

    public record FileViewerModel(FileItemInfo File, FileViewerTheme Theme);

    [HtmlTargetElement("file-viewer-partial")]
    public class FileViewerPartialTagHelper : BasePartialTagHelper<FileViewerModel>
    {
        public FileItemInfo File { get; set; }
        
        public FileViewerTheme Theme { get; set; }

        public FileViewerPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FileViewer/FileViewer.cshtml";

        protected override FileViewerModel CreateModel(string _)
        {
            return new FileViewerModel(File, Theme);
        }
    }
}
