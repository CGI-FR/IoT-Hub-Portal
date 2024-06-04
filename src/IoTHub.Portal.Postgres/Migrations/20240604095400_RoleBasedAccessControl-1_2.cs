// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class RoleBasedAccessControl1_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Groups");

            _ = migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Roles",
                type: "text",
                nullable: false,
                defaultValue: "");

            _ = migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Groups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "Color",
                table: "Roles");

            _ = migrationBuilder.DropColumn(
                name: "Color",
                table: "Groups");

            _ = migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Groups",
                type: "text",
                nullable: true);
        }
    }
}
