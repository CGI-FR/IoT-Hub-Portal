// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class RemoveGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "GroupUser");

            _ = migrationBuilder.DropTable(
                name: "Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PrincipalId = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Groups", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Groups_Principals_PrincipalId",
                        column: x => x.PrincipalId,
                        principalTable: "Principals",
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
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_Groups_PrincipalId",
                table: "Groups",
                column: "PrincipalId",
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_GroupUser_MembersId",
                table: "GroupUser",
                column: "MembersId");
        }
    }
}
