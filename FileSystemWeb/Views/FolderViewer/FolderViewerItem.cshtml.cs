using FileSystemCommon.Models.FileSystem;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FolderViewer
{
    public record FolderViewerItemModel(IFileSystemItem Item);

    [HtmlTargetElement("folder-viewer-item-partial")]
    public class FolderViewerItemPartialTagHelper : BasePartialTagHelper<FolderViewerItemModel>
    {
        public IFileSystemItem Item { get; set; } = null;

        public FolderViewerItemPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FolderViewer/FolderViewerItem.cshtml";

        protected override FolderViewerItemModel CreateModel(string childContent)
        {
            return new FolderViewerItemModel(Item);
        }
    }
}
