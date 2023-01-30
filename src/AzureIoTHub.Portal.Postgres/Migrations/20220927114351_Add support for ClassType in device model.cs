// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddsupportforClassTypeindevicemodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<int>(
                name: "ClassType",
                table: "DeviceModels",
                type: "integer",
                nullable: false,
                defaultValue: ClassType.A);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "ClassType",
                table: "DeviceModels");
        }
    }
}
