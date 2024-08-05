#nullable disable

namespace IoTHub.Portal.SQLite.Migrations
{
    using System;
    using System.Reflection;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Concentrators",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LoraRegion = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceType = table.Column<string>(type: "TEXT", nullable: false),
                    ClientThumbprint = table.Column<string>(type: "TEXT", nullable: true),
                    IsConnected = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Concentrators", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FriendlyName = table.Column<string>(type: "TEXT", nullable: true),
                    Xml = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "DeviceModelCommands",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Frame = table.Column<string>(type: "TEXT", nullable: false),
                    Confirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeviceModelId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceModelCommands", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "DeviceModelProperties",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    IsWritable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    PropertyType = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceModelProperties", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsBuiltin = table.Column<bool>(type: "INTEGER", nullable: false),
                    SupportLoRaFeatures = table.Column<bool>(type: "INTEGER", nullable: false),
                    UseOTAA = table.Column<bool>(type: "INTEGER", nullable: true),
                    PreferredWindow = table.Column<int>(type: "INTEGER", nullable: true),
                    Deduplication = table.Column<int>(type: "INTEGER", nullable: true),
                    ClassType = table.Column<int>(type: "INTEGER", nullable: false),
                    ABPRelaxMode = table.Column<bool>(type: "INTEGER", nullable: true),
                    Downlink = table.Column<bool>(type: "INTEGER", nullable: true),
                    KeepAliveTimeout = table.Column<int>(type: "INTEGER", nullable: true),
                    RXDelay = table.Column<int>(type: "INTEGER", nullable: true),
                    SensorDecoder = table.Column<string>(type: "TEXT", nullable: true),
                    AppEUI = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceModels", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "DeviceTags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Required = table.Column<bool>(type: "INTEGER", nullable: false),
                    Searchable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceTags", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "EdgeDeviceModelCommands",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    EdgeDeviceModelId = table.Column<string>(type: "TEXT", nullable: false),
                    ModuleName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EdgeDeviceModelCommands", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "EdgeDeviceModels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalIdentifier = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_EdgeDeviceModels", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceModelId = table.Column<string>(type: "TEXT", nullable: false),
                    IsConnected = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatusUpdatedTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
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
                });

            _ = migrationBuilder.CreateTable(
                name: "EdgeDevices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceModelId = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectionState = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Scope = table.Column<string>(type: "TEXT", nullable: true),
                    NbDevices = table.Column<int>(type: "INTEGER", nullable: false),
                    NbModules = table.Column<int>(type: "INTEGER", nullable: false)
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
                });

            _ = migrationBuilder.CreateTable(
                name: "LorawanDevices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UseOTAA = table.Column<bool>(type: "INTEGER", nullable: false),
                    AppKey = table.Column<string>(type: "TEXT", nullable: true),
                    AppEUI = table.Column<string>(type: "TEXT", nullable: true),
                    AppSKey = table.Column<string>(type: "TEXT", nullable: true),
                    NwkSKey = table.Column<string>(type: "TEXT", nullable: true),
                    DevAddr = table.Column<string>(type: "TEXT", nullable: true),
                    AlreadyLoggedInOnce = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataRate = table.Column<string>(type: "TEXT", nullable: true),
                    TxPower = table.Column<string>(type: "TEXT", nullable: true),
                    NbRep = table.Column<string>(type: "TEXT", nullable: true),
                    ReportedRX2DataRate = table.Column<string>(type: "TEXT", nullable: true),
                    ReportedRX1DROffset = table.Column<string>(type: "TEXT", nullable: true),
                    ReportedRXDelay = table.Column<string>(type: "TEXT", nullable: true),
                    GatewayID = table.Column<string>(type: "TEXT", nullable: true),
                    Downlink = table.Column<bool>(type: "INTEGER", nullable: true),
                    ClassType = table.Column<int>(type: "INTEGER", nullable: false),
                    PreferredWindow = table.Column<int>(type: "INTEGER", nullable: false),
                    Deduplication = table.Column<int>(type: "INTEGER", nullable: false),
                    RX1DROffset = table.Column<int>(type: "INTEGER", nullable: true),
                    RX2DataRate = table.Column<int>(type: "INTEGER", nullable: true),
                    RXDelay = table.Column<int>(type: "INTEGER", nullable: true),
                    ABPRelaxMode = table.Column<bool>(type: "INTEGER", nullable: true),
                    FCntUpStart = table.Column<int>(type: "INTEGER", nullable: true),
                    FCntDownStart = table.Column<int>(type: "INTEGER", nullable: true),
                    FCntResetCounter = table.Column<int>(type: "INTEGER", nullable: true),
                    Supports32BitFCnt = table.Column<bool>(type: "INTEGER", nullable: true),
                    KeepAliveTimeout = table.Column<int>(type: "INTEGER", nullable: true),
                    SensorDecoder = table.Column<string>(type: "TEXT", nullable: true)
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
                });

            _ = migrationBuilder.CreateTable(
                name: "DeviceTagValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    EdgeDeviceId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_DeviceTagValues", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_DeviceTagValues_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_DeviceTagValues_EdgeDevices_EdgeDeviceId",
                        column: x => x.EdgeDeviceId,
                        principalTable: "EdgeDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "Labels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceModelId = table.Column<string>(type: "TEXT", nullable: true),
                    EdgeDeviceId = table.Column<string>(type: "TEXT", nullable: true),
                    EdgeDeviceModelId = table.Column<string>(type: "TEXT", nullable: true)
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
                });

            _ = migrationBuilder.CreateTable(
                name: "LoRaDeviceTelemetry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnqueuedTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Telemetry = table.Column<string>(type: "TEXT", nullable: false),
                    LorawanDeviceId = table.Column<string>(type: "TEXT", nullable: true)
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

            var currentAssembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{currentAssembly.GetName().Name}.Migrations.SQL.UpQuartzNetTables.sql";
            using var stream = currentAssembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            var sqlResult = reader.ReadToEnd();
            _ = migrationBuilder.Sql(sqlResult);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Concentrators");

            _ = migrationBuilder.DropTable(
                name: "DataProtectionKeys");

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

            var currentAssembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{currentAssembly.GetName().Name}.Migrations.SQL.DownQuartzNetTables.sql";
            using var stream = currentAssembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            var sqlResult = reader.ReadToEnd();
            _ = migrationBuilder.Sql(sqlResult);
        }
    }
}
