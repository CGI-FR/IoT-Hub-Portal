// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class Storeedgedeviceconnectionstate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<string>(
                name: "ConnectionState",
                table: "EdgeDevices",
                type: "text",
                nullable: true,
                defaultValue: "");

            _ = migrationBuilder.Sql("UPDATE \"EdgeDevices\" SET \"ConnectionState\" = ''");

            _ = migrationBuilder.AlterColumn<string>(
                name: "ConnectionState",
                table: "EdgeDevices",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "ConnectionState",
                table: "EdgeDevices");
        }
    }
}
