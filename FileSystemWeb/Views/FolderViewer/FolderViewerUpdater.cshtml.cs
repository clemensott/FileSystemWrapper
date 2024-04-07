using FileSystemCommon.Models.FileSystem.Content;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FolderViewer
{
    public record FolderViewerUpdaterModel(string ChildContent, string Folder, string File,
        FileSystemItemSortType? SortType, FileSystemItemSortDirection? SortDirection, bool PushUrl);

    [HtmlTargetElement("folder-viewer-updater-partial")]
    public class FolderViewerUpdaterPartialTagHelper : BasePartialTagHelper<FolderViewerUpdaterModel>
    {
        public string Folder { get; set; } = null;

        public string File { get; set; } = null;

        public FileSystemItemSortType? SortType { get; set; } = null;

        public FileSystemItemSortDirection? SortDirection { get; set; } = null;

        public bool PushUrl { get; set; } = true;

        public FolderViewerUpdaterPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FolderViewer/FolderViewerUpdater.cshtml";

        protected override FolderViewerUpdaterModel CreateModel(string childContent)
        {
            return new FolderViewerUpdaterModel(childContent, Folder, File, SortType, SortDirection, PushUrl);
        }
    }
}
