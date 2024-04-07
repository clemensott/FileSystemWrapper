using FileSystemCommon.Models.FileSystem;
using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FSItem
{
    public record FSItemPathModel(PathPart[] Path);

    [HtmlTargetElement("fs-item-path-partial")]
    public class FSItemPathPartialTagHelper : BasePartialTagHelper<FSItemPathModel>
    {
        public PathPart[] Path { get; set; }

        public FSItemPathPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FSItem/FSItemPath.cshtml";

        protected override FSItemPathModel CreateModel(string _)
        {
            return new FSItemPathModel(Path);
        }
    }
}
