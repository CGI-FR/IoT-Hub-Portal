// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Labels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: true),
                    DeviceModelId = table.Column<string>(type: "text", nullable: true),
                    EdgeDeviceId = table.Column<string>(type: "text", nullable: true),
                    EdgeDeviceModelId = table.Column<string>(type: "text", nullable: true),
                    LorawanDeviceId = table.Column<string>(type: "text", nullable: true)
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
                    _ = table.ForeignKey(
                        name: "FK_Labels_LorawanDevices_LorawanDeviceId",
                        column: x => x.LorawanDeviceId,
                        principalTable: "LorawanDevices",
                        principalColumn: "Id");
                });

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
                name: "IX_Labels_LorawanDeviceId",
                table: "Labels",
                column: "LorawanDeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Labels");
        }
    }
}
