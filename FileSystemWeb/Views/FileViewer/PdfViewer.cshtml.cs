using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FileViewer
{
    public record PdfViewerModel(FileItemInfo File);

    [HtmlTargetElement("pdf-viewer-partial")]
    public class PdfViewerPartialTagHelper : BasePartialTagHelper<PdfViewerModel>
    {
        public FileItemInfo File { get; set; }

        public PdfViewerPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FileViewer/PdfViewer.cshtml";

        protected override PdfViewerModel CreateModel(string _)
        {
            return new PdfViewerModel(File);
        }
    }
}
