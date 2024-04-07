using FileSystemCommon.Models.Configuration;
using FileSystemWeb.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FileSystemWeb.Controllers.API
{
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        [HttpGet]
        public ActionResult<Config> GetIsAuthorized()
        {
            return ConfigHelper.Public;
        }
    }
}
