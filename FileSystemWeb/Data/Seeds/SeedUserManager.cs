using FileSystemWeb.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Data.Seeds
{
    static class SeedUserManager
    {
        private const string RoleId = "BE51B666-B8B0-437D-B51E-27D23D5114AB";

        public static void Seed(ModelBuilder builder)
        {
            SeedRoles(builder);
            SeedRoleClaims(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(new IdentityRole()
            {
                Id = RoleId,
                Name = Roles.UserManager,
                NormalizedName = Roles.UserManager.ToUpper(),
                ConcurrencyStamp = "63FC4752-E000-4D96-95A2-22BBB8B862A6",
            });
        }

        private static void SeedRoleClaims(ModelBuilder builder)
        {
            builder.Entity<IdentityRoleClaim<string>>().HasData(
                new IdentityRoleClaim<string>()
                {
                    Id = 11,
                    RoleId = RoleId,
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Users.PostUser,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 12,
                    RoleId = RoleId,
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Users.DeleteUser,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 13,
                    RoleId = RoleId,
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Users.GetAllUsers,
                }
            );
        }
    }
}
