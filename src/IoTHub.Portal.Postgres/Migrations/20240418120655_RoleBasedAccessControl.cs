// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;
    /// <inheritdoc />
    public partial class RoleBasedAccessControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Groups", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Roles", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    GivenName = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FamilyName = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Users", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Actions", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Actions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            _ = migrationBuilder.CreateTable(
                name: "AccessControls",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    GroupId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_AccessControls", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_AccessControls_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_AccessControls_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    _ = table.ForeignKey(
                        name: "FK_AccessControls_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "GroupUser",
                columns: table => new
                {
                    GroupsId = table.Column<string>(type: "text", nullable: false),
                    MembersId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_GroupUser", x => new { x.GroupsId, x.MembersId });
                    _ = table.ForeignKey(
                        name: "FK_GroupUser_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_GroupUser_Users_MembersId",
                        column: x => x.MembersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_AccessControls_GroupId",
                table: "AccessControls",
                column: "GroupId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_AccessControls_RoleId_Scope_UserId_GroupId",
                table: "AccessControls",
                columns: new[] { "RoleId", "Scope", "UserId", "GroupId" },
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_AccessControls_UserId",
                table: "AccessControls",
                column: "UserId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Actions_RoleId",
                table: "Actions",
                column: "RoleId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_GroupUser_MembersId",
                table: "GroupUser",
                column: "MembersId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "AccessControls");

            _ = migrationBuilder.DropTable(
                name: "Actions");

            _ = migrationBuilder.DropTable(
                name: "GroupUser");

            _ = migrationBuilder.DropTable(
                name: "Roles");

            _ = migrationBuilder.DropTable(
                name: "Groups");

            _ = migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
