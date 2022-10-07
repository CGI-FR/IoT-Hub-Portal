// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddConcentrator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Concentrators",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LoraRegion = table.Column<string>(type: "text", nullable: false),
                    DeviceType = table.Column<string>(type: "text", nullable: false),
                    ClientThumbprint = table.Column<string>(type: "text", nullable: false),
                    IsConnected = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Concentrators", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Concentrators");
        }
    }
}
