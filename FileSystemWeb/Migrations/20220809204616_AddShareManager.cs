using Microsoft.EntityFrameworkCore.Migrations;

namespace FileSystemWeb.Migrations
{
    public partial class AddShareManager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "0B31FD59-1205-437C-AF9C-8F831C69F200", "A4CBB65E-6481-4FDC-A3A3-653030012687", "share_manager", "SHARE_MANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "FAA6421E-6E8B-4B38-B963-28851886F08A",
                columns: new[] { "PasswordHash", "SecurityStamp" },
                values: new object[] { "AQAAAAEAACcQAAAAEBZelFpQhWYTFtlOcTvtLN0rt4fREufEjiIbwyluGH0csuemo5EVhoKzGvsGVs2CmQ==", "5cb0a7f5-2654-4b15-a001-4afc0fa1058d" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.get_share_files", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.post_share_file", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.get_share_file", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.get_share_file", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.delete_share_file", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.get_share_folders", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.post_share_folder", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.get_share_folder", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.get_share_folder", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { "claims/permission", "permissions.share.delete_share_folder", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { "FAA6421E-6E8B-4B38-B963-28851886F08A", "0B31FD59-1205-437C-AF9C-8F831C69F200" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { "FAA6421E-6E8B-4B38-B963-28851886F08A", "0B31FD59-1205-437C-AF9C-8F831C69F200" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0B31FD59-1205-437C-AF9C-8F831C69F200");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "FAA6421E-6E8B-4B38-B963-28851886F08A",
                columns: new[] { "PasswordHash", "SecurityStamp" },
                values: new object[] { "AQAAAAEAACcQAAAAEMHBFoSWKmqC8P0o5CLNcrCCgf8uG6gUf2dU4HaxYSeqiWVt4VCXFhrUNTBCew1dOg==", "33ead55f-df4b-4875-8d09-324e727d0eb9" });
        }
    }
}
