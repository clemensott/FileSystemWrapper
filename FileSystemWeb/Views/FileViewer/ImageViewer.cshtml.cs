using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FileViewer
{
    public record ImageViewerModel(FileItemInfo File);

    [HtmlTargetElement("image-viewer-partial")]
    public class ImageViewerPartialTagHelper : BasePartialTagHelper<ImageViewerModel>
    {
        public FileItemInfo File { get; set; }

        public ImageViewerPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FileViewer/ImageViewer.cshtml";

        protected override ImageViewerModel CreateModel(string _)
        {
            return new ImageViewerModel(File);
        }
    }
}
