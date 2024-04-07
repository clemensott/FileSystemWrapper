using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace FileSystemWeb.Views.Shared.Components
{
    public enum IconType
    {
        Default,
        Solid,
    }

    public enum IconSize
    {
        XS2,
        XS,
        SM,
        Default,
        LG,
        XL,
        XL2,
        X1,
        X2,
        X3,
        X4,
        X5,
        X6,
        X7,
        X8,
        X9,
        X10,
    }

    public record IconModel(IconType Type, string Name, IconSize Size, string ClassName);

    [HtmlTargetElement("icon-partial")]
    public class IconPartialTagHelper : BasePartialTagHelper<IconModel>
    {
        public IconType Type { get; set; } = IconType.Default;

        public string Name { get; set; } = null;

        public IconSize Size { get; set; } = IconSize.Default;

        public string ClassName { get; set; } = "";

        public IconPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/Shared/Components/Icon.cshtml";

        protected override IconModel CreateModel(string _)
        {
            return new IconModel(Type, Name, Size, ClassName);
        }
    }
}
