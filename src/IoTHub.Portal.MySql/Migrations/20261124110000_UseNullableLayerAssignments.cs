// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.MySql.Migrations
{
    using IoTHub.Portal.Infrastructure;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;

    [DbContext(typeof(PortalDbContext))]
    [Migration("20261124110000_UseNullableLayerAssignments")]
    public class UseNullableLayerAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AlterColumn<string>(name: "Planning", table: "Layers", type: "longtext", nullable: true, oldClrType: typeof(string), oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
            _ = migrationBuilder.Sql("UPDATE `Layers` SET `Planning` = NULL WHERE BINARY `Planning` IN ('None', '');");
            _ = migrationBuilder.Sql("UPDATE `Devices` SET `LayerId` = NULL WHERE BINARY `LayerId` IN ('None', '');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.Sql("UPDATE `Layers` SET `Planning` = 'None' WHERE `Planning` IS NULL;");
            _ = migrationBuilder.AlterColumn<string>(name: "Planning", table: "Layers", type: "longtext", nullable: false, oldClrType: typeof(string), oldType: "longtext", oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
