using FileSystemWeb.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Data.Seeds
{
    static class SeedShareManager
    {
        public static void Seed(ModelBuilder builder)
        {
            SeedRoles(builder);
            SeedRoleClaims(builder);
            SeedUserRoles(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(new IdentityRole()
            {
                Id = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                Name = Roles.ShareManager,
                NormalizedName = Roles.ShareManager.ToUpper(),
                ConcurrencyStamp = "A4CBB65E-6481-4FDC-A3A3-653030012687",
            });
        }

        private static void SeedRoleClaims(ModelBuilder builder)
        {
            builder.Entity<IdentityRoleClaim<string>>().HasData(
                new IdentityRoleClaim<string>()
                {
                    Id = 1,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.GetShareFiles,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 2,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.PostShareFile,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 3,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.GetShareFile,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 4,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.PutShareFile,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 5,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.DeleteShareFile,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 6,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.GetShareFolders,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 7,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.PostShareFolder,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 8,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.GetShareFolder,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 9,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.PutShareFolder,
                },
                new IdentityRoleClaim<string>()
                {
                    Id = 10,
                    RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = Permissions.Share.DeleteShareFolder,
                }
            );
        }

        private static void SeedUserRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>()
            {
                RoleId = "0B31FD59-1205-437C-AF9C-8F831C69F200",
                UserId = "FAA6421E-6E8B-4B38-B963-28851886F08A",
            });
        }
    }
}
