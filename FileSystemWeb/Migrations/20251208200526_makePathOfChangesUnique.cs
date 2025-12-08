using Microsoft.EntityFrameworkCore.Migrations;

namespace FileSystemWeb.Migrations
{
    public partial class makePathOfChangesUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FolderChanges_Path",
                table: "FolderChanges");

            migrationBuilder.DropIndex(
                name: "IX_FileChanges_Path",
                table: "FileChanges");

            migrationBuilder.CreateIndex(
                name: "IX_FolderChanges_Path",
                table: "FolderChanges",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileChanges_Path",
                table: "FileChanges",
                column: "Path",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FolderChanges_Path",
                table: "FolderChanges");

            migrationBuilder.DropIndex(
                name: "IX_FileChanges_Path",
                table: "FileChanges");

            migrationBuilder.CreateIndex(
                name: "IX_FolderChanges_Path",
                table: "FolderChanges",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_FileChanges_Path",
                table: "FileChanges",
                column: "Path");
        }
    }
}
