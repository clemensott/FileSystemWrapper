using FileSystemCommon.Models.FileSystem;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FSItem
{
    public record FSItemModel(IFileSystemItem Item);

    [HtmlTargetElement("fs-item-partial")]
    public class FSItemPartialTagHelper : BasePartialTagHelper<FSItemModel>
    {
        public IFileSystemItem Item { get; set; }

        public FSItemPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FSItem/FSItem.cshtml";

        protected override FSItemModel CreateModel(string _)
        {
            return new FSItemModel(Item);
        }
    }
}
