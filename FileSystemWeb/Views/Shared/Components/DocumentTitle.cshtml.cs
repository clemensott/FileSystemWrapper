using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.Shared.Components
{
    public record DocumentTitleModel(bool IsOutOfBound, string Title);

    [HtmlTargetElement("document-title-partial")]
    public class DocumentTitlePartialTagHelper : BasePartialTagHelper<DocumentTitleModel>
    {
        public bool IsOutOfBound { get; set; }

        public string Title { get; set; }

        public DocumentTitlePartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/Shared/Components/DocumentTitle.cshtml";

        protected override DocumentTitleModel CreateModel(string childContent)
        {
            return new DocumentTitleModel(IsOutOfBound, Title ?? childContent);
        }
    }
}
