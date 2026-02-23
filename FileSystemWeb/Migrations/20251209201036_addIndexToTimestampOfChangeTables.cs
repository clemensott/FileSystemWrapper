using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileSystemWeb.Migrations
{
    /// <inheritdoc />
    public partial class addIndexToTimestampOfChangeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FolderChanges_Timestamp",
                table: "FolderChanges",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_FileChanges_Timestamp",
                table: "FileChanges",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FolderChanges_Timestamp",
                table: "FolderChanges");

            migrationBuilder.DropIndex(
                name: "IX_FileChanges_Timestamp",
                table: "FileChanges");
        }
    }
}
