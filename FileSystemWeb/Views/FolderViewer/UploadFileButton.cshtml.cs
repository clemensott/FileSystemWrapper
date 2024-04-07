using FileSystemWeb.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FileSystemWeb.Views.FolderViewer
{
    public record UploadFileButtonModel(string FolderPath);


    [HtmlTargetElement("upload-file-button-partial")]
    public class UploadFileButtonPartialTagHelper : BasePartialTagHelper<UploadFileButtonModel>
    {
        public string FolderPath { get; set; } = null;

        public bool PushUrl { get; set; } = true;

        public UploadFileButtonPartialTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }

        public override string PartialViewName => "~/Views/FolderViewer/UploadFileButton.cshtml";

        protected override UploadFileButtonModel CreateModel(string _)
        {
            return new UploadFileButtonModel(FolderPath);
        }
    }
}
