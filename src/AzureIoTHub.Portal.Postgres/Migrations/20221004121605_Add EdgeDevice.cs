// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddEdgeDevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<string>(
                name: "EdgeDeviceId",
                table: "DeviceTagValues",
                type: "text",
                nullable: true);

            _ = migrationBuilder.CreateTable(
                name: "EdgeDevices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DeviceModelId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: true),
                    NbDevices = table.Column<int>(type: "integer", nullable: false),
                    NbModules = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EdgeDevices", x => x.Id);
                });

            _ = migrationBuilder.CreateIndex(name: "IX_DeviceTagValues_EdgeDeviceId", table: "DeviceTagValues", column: "EdgeDeviceId");

            _ = migrationBuilder.AddForeignKey(name: "FK_DeviceTagValues_EdgeDevices_EdgeDeviceId", table: "DeviceTagValues", column: "EdgeDeviceId", principalTable: "EdgeDevices", principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(name: "FK_DeviceTagValues_EdgeDevices_EdgeDeviceId", table: "DeviceTagValues");

            _ = migrationBuilder.DropTable(
                name: "EdgeDevices");

            _ = migrationBuilder.DropIndex(name: "IX_DeviceTagValues_EdgeDeviceId", table: "DeviceTagValues");

            _ = migrationBuilder.DropColumn(name: "EdgeDeviceId", table: "DeviceTagValues");
        }
    }
}
