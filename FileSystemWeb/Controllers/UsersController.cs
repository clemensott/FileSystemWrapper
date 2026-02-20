using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        [HttpGet("me")]
        public async Task<ActionResult<User>> GetMe()
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            AppUser user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new NotFoundException("Me not found.", 1104);

            return new User()
            {
                Id = user.Id,
                Name = user.UserName,
            };
        }

        [HttpPut("changePassword")]
        public async Task<ActionResult> PutChangePassword([FromBody] ChangePasswordBody body)
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            AppUser user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new NotFoundException("Me not found.", 1104);

            IdentityResult result = await userManager.ChangePasswordAsync(user, body.OldPassword, body.NewPassword);

            if (result.Succeeded) return Ok();

            throw new BadRequestException("Changing password failed", 1106);
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
            if (user == null) throw new NotFoundException("User not found.", 1105);
            IdentityResult result = await userManager.DeleteAsync(user);

            return result.Succeeded ? (ActionResult)Ok("Success") : BadRequest();
        }
    }
}