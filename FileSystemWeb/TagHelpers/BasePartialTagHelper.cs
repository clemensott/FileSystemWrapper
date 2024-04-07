using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace FileSystemWeb.TagHelpers
{
    public abstract class BasePartialTagHelper<T> : TagHelper where T : class
    {
        private readonly HtmlHelper htmlHelper;

        public T Model { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeNotBound]
        public abstract string PartialViewName { get; }

        public BasePartialTagHelper(IHtmlHelper htmlHelper)
        {
            this.htmlHelper = htmlHelper as HtmlHelper;
        }

        protected virtual T CreateModel(string childContent)
        {
            return Model;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // This results in an empty HTML comment: <!-- -->
            output.TagName = "!-- --";
            output.TagMode = TagMode.StartTagOnly;

            var childContent = await output.GetChildContentAsync();

            htmlHelper.Contextualize(ViewContext);
            IHtmlContent partial = await htmlHelper.PartialAsync(PartialViewName, Model ?? CreateModel(childContent.GetContent()));

            output.PreElement.SetHtmlContent(partial);
        }
    }
}
