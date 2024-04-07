using FileSystemCommon.Models.FileSystem.Files;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FSItem
{
    public record FileActionsDropdownModel(string Title, IFileItem File);

    [HtmlTargetElement("file-actions-dropdown-partial")]
    public class FileActionsDropdownPartialTagHelper : BasePartialTagHelper<FileActionsDropdownModel>
    {
        public string Title { get; set; } = string.Empty;

        public IFileItem File { get; set; } = null;

        public FileActionsDropdownPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FSItem/FileActionsDropdown.cshtml";

        protected override FileActionsDropdownModel CreateModel(string _)
        {
            return new FileActionsDropdownModel(Title, File);
        }
    }
}
