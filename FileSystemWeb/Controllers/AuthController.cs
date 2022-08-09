using System.Threading.Tasks;
using FileSystemCommon.Models.Auth;
using FileSystemWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<AppUser> signInManager;

        public AuthController(SignInManager<AppUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser([FromBody] LoginBody body)
        {
            SignInResult signInResult = await signInManager.PasswordSignInAsync(body.Username,
                body.Password, body.KeepLoggedIn, false);

            return signInResult.Succeeded ? (ActionResult)Ok() : BadRequest();
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }
    }
}