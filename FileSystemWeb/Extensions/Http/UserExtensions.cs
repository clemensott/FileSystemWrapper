using FileSystemWeb.Constants;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FileSystemWeb.Extensions.Http
{
    static class UserExtensions
    {
        public static string GetUserId(this HttpContext context)
        {
            return context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static bool IsShareManager(this HttpContext context)
        {
            return context.User.IsInRole(Roles.ShareManager);
        }
    }
}
