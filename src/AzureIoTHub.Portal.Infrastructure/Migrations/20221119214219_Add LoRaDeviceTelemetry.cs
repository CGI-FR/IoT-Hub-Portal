// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddLoRaDeviceTelemetry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "LoRaDeviceTelemetry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    EnqueuedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Telemetry = table.Column<string>(type: "text", nullable: false),
                    LorawanDeviceId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_LoRaDeviceTelemetry", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_LoRaDeviceTelemetry_LorawanDevices_LorawanDeviceId",
                        column: x => x.LorawanDeviceId,
                        principalTable: "LorawanDevices",
                        principalColumn: "Id");
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_LoRaDeviceTelemetry_LorawanDeviceId",
                table: "LoRaDeviceTelemetry",
                column: "LorawanDeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "LoRaDeviceTelemetry");
        }
    }
}
