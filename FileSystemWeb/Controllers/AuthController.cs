using System.Collections.Generic;
using System.Linq;
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
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser([FromBody] LoginBody body)
        {
            SignInResult signInResult = await signInManager.PasswordSignInAsync(body.Username,
                body.Password, body.KeepLoggedIn, false);

            return signInResult.Succeeded ? (ActionResult) Ok() : BadRequest();
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet("add")]
        public async Task<ActionResult> Add([FromQuery] string name, [FromQuery] string password)
        {
            AppUser user = new AppUser()
            {
                UserName = name,
            };
            
            IdentityResult result = await userManager.CreateAsync(user, password);
            
            return result.Succeeded ? (ActionResult) Ok("Success") : BadRequest();
        }

        [HttpGet]
        public IEnumerable<string> GetUsers()
        {
            return userManager.Users.Select(u => u.UserName);
        }
    }
}