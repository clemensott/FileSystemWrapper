using FileSystemWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Data.Seeds
{
    static class SeedAdminUser
    {
        public static void Seed(ModelBuilder builder)
        {
            AppUser user = new AppUser()
            {
                Id = "FAA6421E-6E8B-4B38-B963-28851886F08A",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                ConcurrencyStamp = "0F36EEAD-944A-4DB0-BFBF-F009BDCD3104",
            };

            PasswordHasher<AppUser> passwordHasher = new PasswordHasher<AppUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, "nas_access");

            builder.Entity<AppUser>().HasData(user);
        }
    }
}
