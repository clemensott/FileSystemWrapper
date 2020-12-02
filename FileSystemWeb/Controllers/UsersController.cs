using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileSystemCommon.Models.Users;
using FileSystemWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly AppDbContext dbContext;

        public UsersController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await dbContext.Users.Select(u => new User()
            {
                Id = u.Id,
                Name = u.UserName,
            }).ToArrayAsync();
        }
    }
}