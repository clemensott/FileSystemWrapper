using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon.Models.Users;
using FileSystemWeb.Constants;
using FileSystemWeb.Models;
using FileSystemWeb.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Controllers
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

        [HttpPost("add")]
        [Authorize(Policy = Permissions.Users.PostUser)]
        public async Task<ActionResult> Add([FromBody] AddUserBody body)
        {
            if (string.IsNullOrWhiteSpace(body.UserName)) throw new BadRequestException("UserName is required.", 1101);
            if (string.IsNullOrWhiteSpace(body.Password)) throw new BadRequestException("Password is required", 1102);

            AppUser user = new AppUser()
            {
                Id = Guid.NewGuid().ToString().ToUpper(),
                UserName = body.UserName,
            };

            IdentityResult result = await userManager.CreateAsync(user, body.Password);

            return result.Succeeded ? (ActionResult)Ok("Success") : BadRequest();
        }

        [HttpDelete]
        [Authorize(Policy = Permissions.Users.DeleteUser)]
        public async Task<ActionResult> Delete([FromBody] DeleteUserBody body)
        {
            if (string.IsNullOrWhiteSpace(body.UserName)) throw new BadRequestException("UserName is required.", 1103);

            AppUser user = await userManager.FindByNameAsync(body.UserName);
            IdentityResult result = await userManager.DeleteAsync(user);

            return result.Succeeded ? (ActionResult)Ok("Success") : BadRequest();
        }
    }
}