using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        public static string Debug;
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "New Hello Word! " + Debug;
        }
    }
}