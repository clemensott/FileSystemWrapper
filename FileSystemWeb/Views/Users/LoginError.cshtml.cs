using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.Users
{
    public record LoginErrorModel(string Message);

    [HtmlTargetElement("login-error-partial")]
    public class LoginErrorPartialTagHelper : BasePartialTagHelper<LoginErrorModel>
    {
        public string Message { get; set; } = null;

        public LoginErrorPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/Users/LoginError.cshtml";

        protected override LoginErrorModel CreateModel(string childContent)
        {
            return new LoginErrorModel(Message ?? childContent);
        }
    }
}
