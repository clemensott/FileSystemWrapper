using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileSystemWeb.Migrations
{
    /// <inheritdoc />
    public partial class ExpiresAt_to_ShareItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "ShareFolders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "ShareFiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                column: "ClaimValue",
                value: "permissions.users.get_all_overview_users");

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 14, "claims/permission", "permissions.users.get_all_overview_users", "BE51B666-B8B0-437D-B51E-27D23D5114AB" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "ShareFolders");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "ShareFiles");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                column: "ClaimValue",
                value: "permissions.users.get_all_users");
        }
    }
}
