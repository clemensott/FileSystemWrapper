using FileSystemWeb.Data.Seeds;
using FileSystemWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<ShareFolder> ShareFolders { get; set; }

        public DbSet<FolderItemPermission> FolderItemPermissions { get; set; }

        public DbSet<ShareFile> ShareFiles { get; set; }

        public DbSet<FileItemPermission> FileItemPermissions { get; set; }

        public DbSet<BigFileUpload> BigFileUploads { get; set; }

        public DbSet<FileChange> FileChanges { get; set; }

        public DbSet<FolderChange> FolderChanges { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            SeedAdminUser.Seed(builder);
            SeedShareManager.Seed(builder);
            SeedUserManager.Seed(builder);
        }
    }
}