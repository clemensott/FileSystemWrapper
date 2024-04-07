using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers.Base
{
    public class LayoutController : Controller
    {
        [ViewData]
        public string Title { get; protected set; }
    }
}
