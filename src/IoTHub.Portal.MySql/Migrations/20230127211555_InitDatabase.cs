// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace IoTHub.Portal.MySql.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "Concentrators",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoraRegion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientThumbprint = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsConnected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Concentrators", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "DeviceModelCommands",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Frame = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeviceModelId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceModelCommands", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "DeviceModelProperties",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsWritable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PropertyType = table.Column<int>(type: "int", nullable: false),
                    ModelId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceModelProperties", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsBuiltin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SupportLoRaFeatures = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UseOTAA = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    PreferredWindow = table.Column<int>(type: "int", nullable: true),
                    Deduplication = table.Column<int>(type: "int", nullable: true),
                    ClassType = table.Column<int>(type: "int", nullable: false),
                    ABPRelaxMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Downlink = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    KeepAliveTimeout = table.Column<int>(type: "int", nullable: true),
                    RXDelay = table.Column<int>(type: "int", nullable: true),
                    SensorDecoder = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppEUI = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceModels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "DeviceTags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Searchable = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceTags", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "EdgeDeviceModelCommands",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EdgeDeviceModelId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModuleName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EdgeDeviceModelCommands", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "EdgeDeviceModels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EdgeDeviceModels", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceModelId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsConnected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StatusUpdatedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Devices", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Devices_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "EdgeDevices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceModelId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ConnectionState = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Scope = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NbDevices = table.Column<int>(type: "int", nullable: false),
                    NbModules = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EdgeDevices", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_EdgeDevices_EdgeDeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "EdgeDeviceModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "LorawanDevices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UseOTAA = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AppKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppEUI = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppSKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NwkSKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DevAddr = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AlreadyLoggedInOnce = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataRate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TxPower = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NbRep = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedRX2DataRate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedRX1DROffset = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedRXDelay = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayID = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Downlink = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    ClassType = table.Column<int>(type: "int", nullable: false),
                    PreferredWindow = table.Column<int>(type: "int", nullable: false),
                    Deduplication = table.Column<int>(type: "int", nullable: false),
                    RX1DROffset = table.Column<int>(type: "int", nullable: true),
                    RX2DataRate = table.Column<int>(type: "int", nullable: true),
                    RXDelay = table.Column<int>(type: "int", nullable: true),
                    ABPRelaxMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    FCntUpStart = table.Column<int>(type: "int", nullable: true),
                    FCntDownStart = table.Column<int>(type: "int", nullable: true),
                    FCntResetCounter = table.Column<int>(type: "int", nullable: true),
                    Supports32BitFCnt = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    KeepAliveTimeout = table.Column<int>(type: "int", nullable: true),
                    SensorDecoder = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_LorawanDevices", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_LorawanDevices_Devices_Id",
                        column: x => x.Id,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "DeviceTagValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EdgeDeviceId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceTagValues", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_DeviceTagValues_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id");
                    _ = table.ForeignKey(
                        name: "FK_DeviceTagValues_EdgeDevices_EdgeDeviceId",
                        column: x => x.EdgeDeviceId,
                        principalTable: "EdgeDevices",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "Labels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceModelId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EdgeDeviceId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EdgeDeviceModelId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Labels", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Labels_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "Id");
                    _ = table.ForeignKey(
                        name: "FK_Labels_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id");
                    _ = table.ForeignKey(
                        name: "FK_Labels_EdgeDeviceModels_EdgeDeviceModelId",
                        column: x => x.EdgeDeviceModelId,
                        principalTable: "EdgeDeviceModels",
                        principalColumn: "Id");
                    _ = table.ForeignKey(
                        name: "FK_Labels_EdgeDevices_EdgeDeviceId",
                        column: x => x.EdgeDeviceId,
                        principalTable: "EdgeDevices",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateTable(
                name: "LoRaDeviceTelemetry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnqueuedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Telemetry = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LorawanDeviceId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_LoRaDeviceTelemetry", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_LoRaDeviceTelemetry_LorawanDevices_LorawanDeviceId",
                        column: x => x.LorawanDeviceId,
                        principalTable: "LorawanDevices",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceModelId",
                table: "Devices",
                column: "DeviceModelId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_DeviceTagValues_DeviceId",
                table: "DeviceTagValues",
                column: "DeviceId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_DeviceTagValues_EdgeDeviceId",
                table: "DeviceTagValues",
                column: "EdgeDeviceId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_EdgeDevices_DeviceModelId",
                table: "EdgeDevices",
                column: "DeviceModelId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Labels_DeviceId",
                table: "Labels",
                column: "DeviceId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Labels_DeviceModelId",
                table: "Labels",
                column: "DeviceModelId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Labels_EdgeDeviceId",
                table: "Labels",
                column: "EdgeDeviceId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Labels_EdgeDeviceModelId",
                table: "Labels",
                column: "EdgeDeviceModelId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_LoRaDeviceTelemetry_LorawanDeviceId",
                table: "LoRaDeviceTelemetry",
                column: "LorawanDeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Concentrators");

            _ = migrationBuilder.DropTable(
                name: "DeviceModelCommands");

            _ = migrationBuilder.DropTable(
                name: "DeviceModelProperties");

            _ = migrationBuilder.DropTable(
                name: "DeviceTags");

            _ = migrationBuilder.DropTable(
                name: "DeviceTagValues");

            _ = migrationBuilder.DropTable(
                name: "EdgeDeviceModelCommands");

            _ = migrationBuilder.DropTable(
                name: "Labels");

            _ = migrationBuilder.DropTable(
                name: "LoRaDeviceTelemetry");

            _ = migrationBuilder.DropTable(
                name: "EdgeDevices");

            _ = migrationBuilder.DropTable(
                name: "LorawanDevices");

            _ = migrationBuilder.DropTable(
                name: "EdgeDeviceModels");

            _ = migrationBuilder.DropTable(
                name: "Devices");

            _ = migrationBuilder.DropTable(
                name: "DeviceModels");
        }
    }
}
