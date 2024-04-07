using FileSystemCommon.Models.FileSystem.Content;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FolderViewer
{
    public record FolderSortButtonModel(FileSystemItemSortBy SortBy);

    [HtmlTargetElement("folder-sort-button-partial")]
    public class FolderSortButtonPartialTagHelper : BasePartialTagHelper<FolderSortButtonModel>
    {
        public FileSystemItemSortBy SortBy { get; set; }

        public FolderSortButtonPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FolderViewer/FolderSortButton.cshtml";

        protected override FolderSortButtonModel CreateModel(string childContent)
        {
            return new FolderSortButtonModel(SortBy);
        }
    }

    internal record SortByOption(string Text, FileSystemItemSortBy Value);
}
