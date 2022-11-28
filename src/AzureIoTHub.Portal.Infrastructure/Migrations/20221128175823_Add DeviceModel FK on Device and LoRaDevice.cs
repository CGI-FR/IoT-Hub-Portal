// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddDeviceModelFKonDeviceandLoRaDevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateIndex(
                name: "IX_LorawanDevices_DeviceModelId",
                table: "LorawanDevices",
                column: "DeviceModelId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceModelId",
                table: "Devices",
                column: "DeviceModelId");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceModels_DeviceModelId",
                table: "Devices",
                column: "DeviceModelId",
                principalTable: "DeviceModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            _ = migrationBuilder.AddForeignKey(
                name: "FK_LorawanDevices_DeviceModels_DeviceModelId",
                table: "LorawanDevices",
                column: "DeviceModelId",
                principalTable: "DeviceModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceModels_DeviceModelId",
                table: "Devices");

            _ = migrationBuilder.DropForeignKey(
                name: "FK_LorawanDevices_DeviceModels_DeviceModelId",
                table: "LorawanDevices");

            _ = migrationBuilder.DropIndex(
                name: "IX_LorawanDevices_DeviceModelId",
                table: "LorawanDevices");

            _ = migrationBuilder.DropIndex(
                name: "IX_Devices_DeviceModelId",
                table: "Devices");
        }
    }
}
