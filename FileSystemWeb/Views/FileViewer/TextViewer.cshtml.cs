using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FileViewer
{
    public record TextViewerModel(FileItemInfo File);

    [HtmlTargetElement("text-viewer-partial")]
    public class TextViewerPartialTagHelper : BasePartialTagHelper<TextViewerModel>
    {
        public FileItemInfo File { get; set; }

        public TextViewerPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FileViewer/TextViewer.cshtml";

        protected override TextViewerModel CreateModel(string _)
        {
            return new TextViewerModel(File);
        }
    }
}
