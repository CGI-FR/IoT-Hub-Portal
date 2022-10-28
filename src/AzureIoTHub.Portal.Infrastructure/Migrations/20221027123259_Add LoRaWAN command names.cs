// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddLoRaWANcommandnames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DeviceModelCommands",
                type: "text",
                nullable: false,
                defaultValue: "");

            _ = migrationBuilder.Sql("UPDATE \"DeviceModelCommands\" SET \"Name\" = \"Id\"");
            _ = migrationBuilder.Sql("UPDATE \"DeviceModelCommands\" SET \"Id\" = uuid_in(overlay(overlay(md5(random()::text || ':' || random()::text) placing '4' from 13) placing to_hex(floor(random()*(11-8+1) + 8)::int)::text from 17)::cstring)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.Sql("UPDATE \"DeviceModelCommands\" SET \"Id\" = \"Name\"");

            _ = migrationBuilder.DropColumn(
                name: "Name",
                table: "DeviceModelCommands");
        }
    }
}
