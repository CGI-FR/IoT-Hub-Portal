// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class IntroduceDeviceBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.Sql("INSERT INTO \"Devices\"(\"Id\",\"Name\", \"DeviceModelId\", \"IsConnected\", \"IsEnabled\", \"StatusUpdatedTime\", \"Version\")" +
                " SELECT \"Id\",\"Name\", \"DeviceModelId\", \"IsConnected\", \"IsEnabled\", \"StatusUpdatedTime\", \"Version\" FROM \"LorawanDevices\"");

            _ = migrationBuilder.Sql("UPDATE \"DeviceTagValues\" SET \"DeviceId\" = \"LorawanDeviceId\"" +
                " WHERE \"LorawanDeviceId\" <> NULL");

            _ = migrationBuilder.Sql("UPDATE \"Labels\" SET \"DeviceId\" = \"LorawanDeviceId\"" +
                " WHERE \"LorawanDeviceId\" <> NULL");

            _ = migrationBuilder.DropForeignKey(
                name: "FK_DeviceTagValues_LorawanDevices_LorawanDeviceId",
                table: "DeviceTagValues");

            _ = migrationBuilder.DropForeignKey(
                name: "FK_Labels_LorawanDevices_LorawanDeviceId",
                table: "Labels");

            _ = migrationBuilder.DropForeignKey(
                name: "FK_LorawanDevices_DeviceModels_DeviceModelId",
                table: "LorawanDevices");

            _ = migrationBuilder.DropIndex(
                name: "IX_LorawanDevices_DeviceModelId",
                table: "LorawanDevices");

            _ = migrationBuilder.DropIndex(
                name: "IX_Labels_LorawanDeviceId",
                table: "Labels");

            _ = migrationBuilder.DropIndex(
                name: "IX_DeviceTagValues_LorawanDeviceId",
                table: "DeviceTagValues");

            _ = migrationBuilder.DropColumn(
                name: "DeviceModelId",
                table: "LorawanDevices");

            _ = migrationBuilder.DropColumn(
                name: "IsConnected",
                table: "LorawanDevices");

            _ = migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "LorawanDevices");

            _ = migrationBuilder.DropColumn(
                name: "Name",
                table: "LorawanDevices");

            _ = migrationBuilder.DropColumn(
                name: "StatusUpdatedTime",
                table: "LorawanDevices");

            _ = migrationBuilder.DropColumn(
                name: "Version",
                table: "LorawanDevices");

            _ = migrationBuilder.DropColumn(
                name: "LorawanDeviceId",
                table: "Labels");

            _ = migrationBuilder.DropColumn(
                name: "LorawanDeviceId",
                table: "DeviceTagValues");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_LorawanDevices_Devices_Id",
                table: "LorawanDevices",
                column: "Id",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(
                name: "FK_LorawanDevices_Devices_Id",
                table: "LorawanDevices");

            _ = migrationBuilder.AddColumn<string>(
                name: "DeviceModelId",
                table: "LorawanDevices",
                type: "text",
                nullable: false,
                defaultValue: "");

            _ = migrationBuilder.AddColumn<bool>(
                name: "IsConnected",
                table: "LorawanDevices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            _ = migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "LorawanDevices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            _ = migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "LorawanDevices",
                type: "text",
                nullable: false,
                defaultValue: "");

            _ = migrationBuilder.AddColumn<DateTime>(
                name: "StatusUpdatedTime",
                table: "LorawanDevices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            _ = migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "LorawanDevices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            _ = migrationBuilder.AddColumn<string>(
                name: "LorawanDeviceId",
                table: "Labels",
                type: "text",
                nullable: true);

            _ = migrationBuilder.AddColumn<string>(
                name: "LorawanDeviceId",
                table: "DeviceTagValues",
                type: "text",
                nullable: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_LorawanDevices_DeviceModelId",
                table: "LorawanDevices",
                column: "DeviceModelId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Labels_LorawanDeviceId",
                table: "Labels",
                column: "LorawanDeviceId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_DeviceTagValues_LorawanDeviceId",
                table: "DeviceTagValues",
                column: "LorawanDeviceId");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_DeviceTagValues_LorawanDevices_LorawanDeviceId",
                table: "DeviceTagValues",
                column: "LorawanDeviceId",
                principalTable: "LorawanDevices",
                principalColumn: "Id");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_Labels_LorawanDevices_LorawanDeviceId",
                table: "Labels",
                column: "LorawanDeviceId",
                principalTable: "LorawanDevices",
                principalColumn: "Id");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_LorawanDevices_DeviceModels_DeviceModelId",
                table: "LorawanDevices",
                column: "DeviceModelId",
                principalTable: "DeviceModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
