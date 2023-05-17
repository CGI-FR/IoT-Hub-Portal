// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Postgres.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddThingTypeFKonDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<string>(
                name: "ThingTypeId",
                table: "Devices",
                type: "text",
                nullable: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_Devices_ThingTypeId",
                table: "Devices",
                column: "ThingTypeId");

            _ = migrationBuilder.AlterColumn<string>(
                name: "DeviceModelId",
                table: "Devices",
                type: "text",
                nullable: true);

            _ = migrationBuilder.AddForeignKey(
                name: "FK_Devices_ThingTypes_ThingTypeId",
                table: "Devices",
                column: "ThingTypeId",
                principalTable: "ThingTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(
                name: "FK_Devices_ThingTypes_ThingTypeId",
                table: "Devices");

            _ = migrationBuilder.DropIndex(
                name: "IX_Devices_ThingTypeId",
                table: "Devices");

            _ = migrationBuilder.AlterColumn<string>(
                name: "DeviceModelId",
                table: "Devices",
                type: "text",
                nullable: false);

            _ = migrationBuilder.DropColumn(
                name: "ThingTypeId",
                table: "Devices");
        }
    }
}
