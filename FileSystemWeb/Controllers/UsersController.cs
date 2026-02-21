using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FileSystemCommon.Models;
using FileSystemCommon.Models.Users;
using FileSystemWeb.Constants;
using FileSystemWeb.Data;
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
        private readonly AppDbContext dbContext;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersController(AppDbContext dbContext, UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet("me")]
        public async Task<ActionResult<MeUser>> GetMe()
        {
            string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            AppUser user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new NotFoundException("Me not found.", 1104);

            List<string> roleIds = await dbContext.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();
            IQueryable<IdentityRole> roles = roleManager.Roles.Where(r => roleIds.Contains(r.Id));
            string[] permissions = await dbContext.RoleClaims
                .Where(rc => roleIds.Contains(rc.RoleId))
                .Select(rc => rc.ClaimValue)
                .Distinct()
                .ToArrayAsync();

            return new MeUser()
            {
                Id = user.Id,
                Name = user.UserName,
                Roles = roles.Select(r => r.Name).ToArray(),
                Permissions = permissions,
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

        [HttpGet("overview")]
        [Authorize(Policy = Permissions.Users.GetAllOverviewUsers)]
        public async Task<ActionResult<IEnumerable<OverviewUser>>> GetAllUserOverview()
        {
            AppUser[] users = await userManager.Users
                .ToArrayAsync();
            IdentityUserRole<string>[] allUserRoles = await dbContext.UserRoles.ToArrayAsync();
            IdentityRole[] allRoles = await roleManager.Roles.ToArrayAsync();

            ILookup<string, IdentityUserRole<string>> userRolesLookup = allUserRoles.ToLookup(ur => ur.UserId);
            Dictionary<string, IdentityRole> rolesLookup = allRoles.ToDictionary(ur => ur.Id);

            return users.Select(user =>
            {
                IEnumerable<string> roleIds = userRolesLookup[user.Id].Select(ur => ur.RoleId);
                IEnumerable<IdentityRole> roles = roleIds.Select(roleId => rolesLookup[roleId]);
                return new OverviewUser()
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Roles = roles.Select(r => r.Name).ToArray(),
                };
            }).ToArray();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Users.DeleteUser)]
        public async Task<ActionResult> DeleteUser(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user == null) throw new NotFoundException("User not found.", 1107);

            dbContext.ShareFiles.RemoveRange(dbContext.ShareFiles.Where(f => f.UserId == user.Id));
            dbContext.ShareFolders.RemoveRange(dbContext.ShareFolders.Where(f => f.UserId == user.Id));
            dbContext.Users.Remove(user);

            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("roles")]
        [Authorize(Policy = Permissions.Users.GetAllRoles)]
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            return await roleManager.Roles.Select(r => new Role()
            {
                Id = r.Id,
                Name = r.Name
            }).ToArrayAsync();
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
            if (!result.Succeeded) throw new BadRequestException("Password is required", 1108);

            if (body.RoleIds != null)
            {
                dbContext.UserRoles.AddRange(body.RoleIds.Select(roleId => new IdentityUserRole<string>()
                {
                    RoleId = roleId,
                    UserId = user.Id
                }));
                await dbContext.SaveChangesAsync();
            }

            return Ok();
        }
    }
}