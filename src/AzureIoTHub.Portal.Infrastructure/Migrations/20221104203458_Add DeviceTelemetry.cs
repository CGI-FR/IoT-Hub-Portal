// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddDeviceTelemetry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "DeviceTelemetries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    EnqueuedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Telemetry = table.Column<string>(type: "text", nullable: false),
                    LorawanDeviceId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceTelemetries", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_DeviceTelemetries_LorawanDevices_LorawanDeviceId",
                        column: x => x.LorawanDeviceId,
                        principalTable: "LorawanDevices",
                        principalColumn: "Id");
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_DeviceTelemetries_LorawanDeviceId",
                table: "DeviceTelemetries",
                column: "LorawanDeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "DeviceTelemetries");
        }
    }
}
