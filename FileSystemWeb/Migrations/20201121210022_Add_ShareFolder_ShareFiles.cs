using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FileSystemWeb.Migrations
{
    public partial class Add_ShareFolder_ShareFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileItemPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Read = table.Column<bool>(nullable: false),
                    Info = table.Column<bool>(nullable: false),
                    Hash = table.Column<bool>(nullable: false),
                    Write = table.Column<bool>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    List = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileItemPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShareFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    IsListed = table.Column<bool>(nullable: false),
                    PermissionId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShareFiles_FileItemPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "FileItemPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShareFiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.UniqueConstraint(
                        name: "UK_ShareFiles_Uuid",
                        columns: x => new
                        {
                            x.Uuid,
                        });
                    table.UniqueConstraint(
                        name: "UK_ShareFiles_Name_UserId",
                        columns: x => new
                        {
                            x.Name,
                            x.UserId,
                        });
                });

            migrationBuilder.CreateTable(
                name: "ShareFolders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    IsListed = table.Column<bool>(nullable: false),
                    PermissionId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShareFolders_FileItemPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "FileItemPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShareFolders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.UniqueConstraint(
                        name: "UK_ShareFolders_Uuid",
                        columns: x => new
                        {
                            x.Uuid,
                        });
                    table.UniqueConstraint(
                        name: "UK_ShareFolders_Name_UserId",
                        columns: x => new
                        {
                            x.Name,
                            x.UserId,
                        });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShareFiles_PermissionId",
                table: "ShareFiles",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareFiles_UserId",
                table: "ShareFiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareFolders_PermissionId",
                table: "ShareFolders",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareFolders_UserId",
                table: "ShareFolders",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShareFiles");

            migrationBuilder.DropTable(
                name: "ShareFolders");

            migrationBuilder.DropTable(
                name: "FileItemPermissions");
        }
    }
}
