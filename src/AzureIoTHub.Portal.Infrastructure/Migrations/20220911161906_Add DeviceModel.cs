// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddDeviceModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsBuiltin = table.Column<bool>(type: "boolean", nullable: false),
                    SupportLoRaFeatures = table.Column<bool>(type: "boolean", nullable: false),
                    UseOTAA = table.Column<bool>(type: "boolean", nullable: true),
                    PreferredWindow = table.Column<int>(type: "integer", nullable: true),
                    Deduplication = table.Column<int>(type: "integer", nullable: true),
                    ABPRelaxMode = table.Column<bool>(type: "boolean", nullable: true),
                    Downlink = table.Column<bool>(type: "boolean", nullable: true),
                    KeepAliveTimeout = table.Column<int>(type: "integer", nullable: true),
                    RXDelay = table.Column<int>(type: "integer", nullable: true),
                    SensorDecoder = table.Column<string>(type: "text", nullable: true),
                    AppEUI = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceModels", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "DeviceModels");
        }
    }
}
