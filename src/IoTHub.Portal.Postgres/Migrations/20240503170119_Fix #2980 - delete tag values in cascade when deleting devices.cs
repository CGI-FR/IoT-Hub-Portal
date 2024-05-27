// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class Fix2980deletetagvaluesincascadewhendeletingdevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(
                name: "FK_DeviceTagValues_Devices_DeviceId",
                table: "DeviceTagValues");

            _ = migrationBuilder.DropForeignKey(
                name: "FK_DeviceTagValues_EdgeDevices_EdgeDeviceId",
                table: "DeviceTagValues");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_DeviceTagValues_Devices_DeviceId",
                table: "DeviceTagValues",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            _ = migrationBuilder.AddForeignKey(
                name: "FK_DeviceTagValues_EdgeDevices_EdgeDeviceId",
                table: "DeviceTagValues",
                column: "EdgeDeviceId",
                principalTable: "EdgeDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(
                name: "FK_DeviceTagValues_Devices_DeviceId",
                table: "DeviceTagValues");

            _ = migrationBuilder.DropForeignKey(
                name: "FK_DeviceTagValues_EdgeDevices_EdgeDeviceId",
                table: "DeviceTagValues");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_DeviceTagValues_Devices_DeviceId",
                table: "DeviceTagValues",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_DeviceTagValues_EdgeDevices_EdgeDeviceId",
                table: "DeviceTagValues",
                column: "EdgeDeviceId",
                principalTable: "EdgeDevices",
                principalColumn: "Id");
        }
    }
}
