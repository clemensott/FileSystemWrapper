using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.Shared.Components
{
    public record NavbarModel(bool IsOutOfBound, string Path, bool IsAuthenticated, bool IsShareAllowed);

    [HtmlTargetElement("navbar-partial")]
    public class NavbarPartialTagHelper : BasePartialTagHelper<NavbarModel>
    {
        public bool IsOutOfBound { get; set; }

        public string Path { get; set; }
        
        public bool IsAuthenticated { get; set; }
        
        public bool IsShareAllowed { get; set; }

        public NavbarPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/Shared/Components/Navbar.cshtml";

        protected override NavbarModel CreateModel(string _)
        {
            return new NavbarModel(IsOutOfBound, Path, IsAuthenticated, IsShareAllowed);
        }
    }
}
