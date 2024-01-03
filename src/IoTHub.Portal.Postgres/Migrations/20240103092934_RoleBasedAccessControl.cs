// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "text", nullable: false),
                    HttpMethod = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Actions", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: false)
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
                    Avatar = table.Column<string>(type: "text", nullable: false)
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
                    Name = table.Column<string>(type: "text", nullable: false),
                    Forename = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Users", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "AccessControls",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_AccessControls", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_AccessControls_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "RoleActions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ActionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_RoleActions", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_RoleActions_Actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_RoleActions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    GroupId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Members", x => new { x.UserId, x.GroupId });
                    _ = table.ForeignKey(
                        name: "FK_Members_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_Members_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "GroupAccessControls",
                columns: table => new
                {
                    GroupId = table.Column<string>(type: "text", nullable: false),
                    AccessControlId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_GroupAccessControls", x => new { x.GroupId, x.AccessControlId });
                    _ = table.ForeignKey(
                        name: "FK_GroupAccessControls_AccessControls_AccessControlId",
                        column: x => x.AccessControlId,
                        principalTable: "AccessControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_GroupAccessControls_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "UserAccessControls",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AccessControlId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_UserAccessControls", x => new { x.UserId, x.AccessControlId });
                    _ = table.ForeignKey(
                        name: "FK_UserAccessControls_AccessControls_AccessControlId",
                        column: x => x.AccessControlId,
                        principalTable: "AccessControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_UserAccessControls_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_AccessControls_RoleId",
                table: "AccessControls",
                column: "RoleId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_GroupAccessControls_AccessControlId",
                table: "GroupAccessControls",
                column: "AccessControlId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Members_GroupId",
                table: "Members",
                column: "GroupId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_RoleActions_ActionId",
                table: "RoleActions",
                column: "ActionId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_RoleActions_RoleId",
                table: "RoleActions",
                column: "RoleId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserAccessControls_AccessControlId",
                table: "UserAccessControls",
                column: "AccessControlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "GroupAccessControls");

            _ = migrationBuilder.DropTable(
                name: "Members");

            _ = migrationBuilder.DropTable(
                name: "RoleActions");

            _ = migrationBuilder.DropTable(
                name: "UserAccessControls");

            _ = migrationBuilder.DropTable(
                name: "Groups");

            _ = migrationBuilder.DropTable(
                name: "Actions");

            _ = migrationBuilder.DropTable(
                name: "AccessControls");

            _ = migrationBuilder.DropTable(
                name: "Users");

            _ = migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
