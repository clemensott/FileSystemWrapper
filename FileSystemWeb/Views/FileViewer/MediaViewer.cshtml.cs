using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FileViewer
{
    public enum MediaViewerType
    {
        Audio,
        Video,
    }

    public record MediaViewerModel(FileItemInfo File, MediaViewerType Type);

    [HtmlTargetElement("media-viewer-partial")]
    public class MediaViewerPartialTagHelper : BasePartialTagHelper<MediaViewerModel>
    {
        public FileItemInfo File { get; set; }
        
        public MediaViewerType Type { get; set; }

        public MediaViewerPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FileViewer/MediaViewer.cshtml";

        protected override MediaViewerModel CreateModel(string _)
        {
            return new MediaViewerModel(File, Type);
        }
    }
}
