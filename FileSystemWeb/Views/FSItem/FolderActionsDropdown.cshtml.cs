using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FSItem
{
    public record FolderActionsDropdownModel(string Title, IFolderItem Folder);

    [HtmlTargetElement("folder-actions-dropdown-partial")]
    public class FolderActionsDropdownPartialTagHelper : BasePartialTagHelper<FolderActionsDropdownModel>
    {
        public string Title { get; set; } = string.Empty;

        public IFolderItem Folder { get; set; } = null;

        public FolderActionsDropdownPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FSItem/FolderActionsDropdown.cshtml";

        protected override FolderActionsDropdownModel CreateModel(string _)
        {
            return new FolderActionsDropdownModel(Title, Folder);
        }
    }
}
