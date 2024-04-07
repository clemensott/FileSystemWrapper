using FileSystemWeb.Controllers.Base;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Models;
using FileSystemWeb.Views.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FileSystemWeb.Controllers
{
    [Route("[controller]")]
    public class UsersController : LayoutController
    {
        private readonly SignInManager<AppUser> signInManager;

        public UsersController(SignInManager<AppUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        [HttpGet("login")]
        public IActionResult GetLogin()
        {
            Title = "Login";

            return View("./Login");
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin([FromForm] string username, [FromForm] string password)
        {
            var signInResult = await signInManager.PasswordSignInAsync(username, password, false, false);

            if (signInResult.Succeeded)
            {
                HttpContext.Response.SetHtmxLocation("/");
                return NoContent();
            }

            return View("./LoginError", new LoginErrorModel("Please enter a correct Username and password"));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> PostLogout()
        {
            await signInManager.SignOutAsync();

            HttpContext.Response.SetHtmxLocation("/users/login");
            return NoContent();
        }
    }
}
