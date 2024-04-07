using FileSystemWeb.Views.Shared.Components;
using Microsoft.AspNetCore.Http;

namespace FileSystemWeb.Extensions.Http
{
    static class NavbarExtensions
    {
        public static NavbarModel GetNavbarModel(this HttpContext context, bool isOutOfBound)
        {
            bool isAuthenticated = context.User.Identity.IsAuthenticated;
            bool isShareAllowed = context.IsShareManager();

            return new NavbarModel(isOutOfBound, context.Request.Path, isAuthenticated, isShareAllowed);
        }
    }
}
