
// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#nullable disable
namespace AzureIoTHub.Portal.Postgres.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AWSThingTypeinitialcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "ThingTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_ThingTypes", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "ThingTypeSearchableAttributes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ThingTypeId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_ThingTypeSearchableAttributes", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_ThingTypeSearchableAttributes_ThingTypes_ThingTypeId",
                        column: x => x.ThingTypeId,
                        principalTable: "ThingTypes",
                        principalColumn: "Id");
                });

            _ = migrationBuilder.CreateTable(
                name: "ThingTypeTags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    ThingTypeId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_ThingTypeTags", x => x.Id);
                    _ = table.ForeignKey(name: "FK_ThingTypeTags_ThingTypes_ThingTypeId", column: x => x.ThingTypeId, principalTable: "ThingTypes", principalColumn: "Id");
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_ThingTypeSearchableAttributes_ThingTypeId",
                table: "ThingTypeSearchableAttributes",
                column: "ThingTypeId");

            _ = migrationBuilder.CreateIndex(name: "IX_ThingTypeTags_ThingTypeId", table: "ThingTypeTags", column: "ThingTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "ThingTypeSearchableAttributes");

            _ = migrationBuilder.DropTable(name: "ThingTypeTags");

            _ = migrationBuilder.DropTable(name: "ThingTypes");
        }
    }
}
