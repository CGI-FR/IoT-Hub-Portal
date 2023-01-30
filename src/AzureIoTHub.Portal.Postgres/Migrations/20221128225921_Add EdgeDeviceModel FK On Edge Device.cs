// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddEdgeDeviceModelFKOnEdgeDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateIndex(
                name: "IX_EdgeDevices_DeviceModelId",
                table: "EdgeDevices",
                column: "DeviceModelId");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_EdgeDevices_EdgeDeviceModels_DeviceModelId",
                table: "EdgeDevices",
                column: "DeviceModelId",
                principalTable: "EdgeDeviceModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(
                name: "FK_EdgeDevices_EdgeDeviceModels_DeviceModelId",
                table: "EdgeDevices");

            _ = migrationBuilder.DropIndex(
                name: "IX_EdgeDevices_DeviceModelId",
                table: "EdgeDevices");
        }
    }
}
