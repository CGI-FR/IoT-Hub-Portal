// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddDeviceTagValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "Tags",
                table: "LorawanDevices");

            _ = migrationBuilder.DropColumn(
                name: "Tags",
                table: "Devices");

            _ = migrationBuilder.AlterColumn<int>(
                name: "ClassType",
                table: "DeviceModels",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            _ = migrationBuilder.CreateTable(
                name: "DeviceTagValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: true),
                    LorawanDeviceId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceTagValues", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_DeviceTagValues_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id");
                    _ = table.ForeignKey(
                        name: "FK_DeviceTagValues_LorawanDevices_LorawanDeviceId",
                        column: x => x.LorawanDeviceId,
                        principalTable: "LorawanDevices",
                        principalColumn: "Id");
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_DeviceTagValues_DeviceId",
                table: "DeviceTagValues",
                column: "DeviceId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_DeviceTagValues_LorawanDeviceId",
                table: "DeviceTagValues",
                column: "LorawanDeviceId");

            _ = migrationBuilder.Sql("UPDATE \"Devices\" SET \"Version\" = 0;");
            _ = migrationBuilder.Sql("UPDATE \"LorawanDevices\" SET \"Version\" = 0;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "DeviceTagValues");

            _ = migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "LorawanDevices",
                type: "text",
                nullable: false,
                defaultValue: "");

            _ = migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Devices",
                type: "text",
                nullable: false,
                defaultValue: "");

            _ = migrationBuilder.AlterColumn<int>(
                name: "ClassType",
                table: "DeviceModels",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
