using FileSystemCommon.Models.FileSystem.Content;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FolderViewer
{
    public record FolderViewerModel(FolderSortButtonModel SortButton, FolderContent Content);

    [HtmlTargetElement("folder-viewer-partial")]
    public class FolderViewerPartialTagHelper : BasePartialTagHelper<FolderViewerModel>
    {
        public FolderSortButtonModel SortButton { get; set; } = null;
        
        public FolderContent Content { get; set; } = null;

        public FolderViewerPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FolderViewer/FolderViewer.cshtml";

        protected override FolderViewerModel CreateModel(string childContent)
        {
            return new FolderViewerModel(SortButton, Content);
        }
    }
}
