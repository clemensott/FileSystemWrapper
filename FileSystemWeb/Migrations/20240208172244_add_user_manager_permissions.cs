using Microsoft.EntityFrameworkCore.Migrations;

namespace FileSystemWeb.Migrations
{
    public partial class add_user_manager_permissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "BE51B666-B8B0-437D-B51E-27D23D5114AB", "63FC4752-E000-4D96-95A2-22BBB8B862A6", "user_manager", "USER_MANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "FAA6421E-6E8B-4B38-B963-28851886F08A",
                columns: new[] { "PasswordHash", "SecurityStamp" },
                values: new object[] { "AQAAAAEAACcQAAAAENBKFrGvayF7a7Ow8I+ik06fhdpchFjO4rg6OQRrFEW4XNG5JFui5aVLsGH3/KDiAA==", "3a54f369-3a45-4ab8-8d22-885a8d8b60d7" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 11, "claims/permission", "permissions.users.post_user", "BE51B666-B8B0-437D-B51E-27D23D5114AB" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 12, "claims/permission", "permissions.users.post_user", "BE51B666-B8B0-437D-B51E-27D23D5114AB" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 13, "claims/permission", "permissions.users.get_all_users", "BE51B666-B8B0-437D-B51E-27D23D5114AB" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "BE51B666-B8B0-437D-B51E-27D23D5114AB");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "FAA6421E-6E8B-4B38-B963-28851886F08A",
                columns: new[] { "PasswordHash", "SecurityStamp" },
                values: new object[] { "AQAAAAEAACcQAAAAEBy/B/tfxordd6bWoQIcEy4tofML/MHjfv2rNmsfBS7UUQH6QYwGhTIW3Q90LgkB2A==", "9ff96f04-2a2d-46bf-b8bf-e772c0fdb61b" });
        }
    }
}
