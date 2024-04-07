using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon.Models.Users;
using FileSystemWeb.Constants;
using FileSystemWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Controllers.API
{
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet("all")]
        [Authorize(Policy = Permissions.Users.GetAllUsers)]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await userManager.Users.Select(u => new User()
            {
                Id = u.Id,
                Name = u.UserName,
            }).ToArrayAsync();
        }

        [HttpGet("add")]
        [Authorize(Policy = Permissions.Users.PostUser)]
        public async Task<ActionResult> Add([FromQuery] string name, [FromQuery] string password)
        {
            AppUser user = new AppUser()
            {
                UserName = name,
            };

            IdentityResult result = await userManager.CreateAsync(user, password);

            return result.Succeeded ? Ok("Success") : BadRequest();
        }
    }
}