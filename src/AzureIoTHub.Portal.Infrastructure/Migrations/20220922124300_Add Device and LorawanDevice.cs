// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddDeviceandLorawanDevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DeviceModelId = table.Column<string>(type: "text", nullable: false),
                    IsConnected = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    StatusUpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Devices", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "LorawanDevices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DeviceModelId = table.Column<string>(type: "text", nullable: false),
                    IsConnected = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    StatusUpdatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: false),
                    UseOTAA = table.Column<bool>(type: "boolean", nullable: false),
                    AppKey = table.Column<string>(type: "text", nullable: true),
                    AppEUI = table.Column<string>(type: "text", nullable: true),
                    AppSKey = table.Column<string>(type: "text", nullable: true),
                    NwkSKey = table.Column<string>(type: "text", nullable: true),
                    DevAddr = table.Column<string>(type: "text", nullable: true),
                    AlreadyLoggedInOnce = table.Column<bool>(type: "boolean", nullable: false),
                    DataRate = table.Column<string>(type: "text", nullable: true),
                    TxPower = table.Column<string>(type: "text", nullable: true),
                    NbRep = table.Column<string>(type: "text", nullable: true),
                    ReportedRX2DataRate = table.Column<string>(type: "text", nullable: true),
                    ReportedRX1DROffset = table.Column<string>(type: "text", nullable: true),
                    ReportedRXDelay = table.Column<string>(type: "text", nullable: true),
                    GatewayID = table.Column<string>(type: "text", nullable: true),
                    Downlink = table.Column<bool>(type: "boolean", nullable: true),
                    ClassType = table.Column<int>(type: "integer", nullable: false),
                    PreferredWindow = table.Column<int>(type: "integer", nullable: false),
                    Deduplication = table.Column<int>(type: "integer", nullable: false),
                    RX1DROffset = table.Column<int>(type: "integer", nullable: true),
                    RX2DataRate = table.Column<int>(type: "integer", nullable: true),
                    RXDelay = table.Column<int>(type: "integer", nullable: true),
                    ABPRelaxMode = table.Column<bool>(type: "boolean", nullable: true),
                    FCntUpStart = table.Column<int>(type: "integer", nullable: true),
                    FCntDownStart = table.Column<int>(type: "integer", nullable: true),
                    FCntResetCounter = table.Column<int>(type: "integer", nullable: true),
                    Supports32BitFCnt = table.Column<bool>(type: "boolean", nullable: true),
                    KeepAliveTimeout = table.Column<int>(type: "integer", nullable: true),
                    SensorDecoder = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_LorawanDevices", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Devices");

            _ = migrationBuilder.DropTable(
                name: "LorawanDevices");
        }
    }
}
