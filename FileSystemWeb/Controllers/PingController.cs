using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers
{
    public class PingController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "Success";
        }
        
        [HttpGet("auth")]
        [Authorize]
        public ActionResult<string> GetIsAuthorized()
        {
            return "Success";
        }
    }
}