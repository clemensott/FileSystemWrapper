using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace FileSystemWeb.Migrations
{
    public partial class AddAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "FAA6421E-6E8B-4B38-B963-28851886F08A", 0, "0F36EEAD-944A-4DB0-BFBF-F009BDCD3104", null, false, false, null, null, "ADMIN", "AQAAAAEAACcQAAAAEMHBFoSWKmqC8P0o5CLNcrCCgf8uG6gUf2dU4HaxYSeqiWVt4VCXFhrUNTBCew1dOg==", null, false, "33ead55f-df4b-4875-8d09-324e727d0eb9", false, "admin" });

            string shareFolderUuid = Guid.NewGuid().ToString().ToUpper();
            migrationBuilder.Sql(@$"
                INSERT INTO FileItemPermissions (Read, Info, Hash, Write, Discriminator, List)
                VALUES (1, 1, 1, 1, 'FolderItemPermission', 1);

                INSERT INTO ShareFolders (Uuid, Name, Path, IsListed, PermissionId, UserId)
                VALUES ('{shareFolderUuid}', 'Drives', '', 1, last_insert_rowid(), 'FAA6421E-6E8B-4B38-B963-28851886F08A');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "FAA6421E-6E8B-4B38-B963-28851886F08A");

            migrationBuilder.Sql("DELETE FROM ShareFolders WHERE UserId = 'FAA6421E-6E8B-4B38-B963-28851886F08A';");
        }
    }
}
