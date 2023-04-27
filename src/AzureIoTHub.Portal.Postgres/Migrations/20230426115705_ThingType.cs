using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureIoTHub.Portal.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ThingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThingTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThingTypeSearchableAttributes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ThingTypeId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingTypeSearchableAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThingTypeSearchableAttributes_ThingTypes_ThingTypeId",
                        column: x => x.ThingTypeId,
                        principalTable: "ThingTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThingTypeTags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    ThingTypeId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThingTypeTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThingTypeTags_ThingTypes_ThingTypeId",
                        column: x => x.ThingTypeId,
                        principalTable: "ThingTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThingTypeSearchableAttributes_ThingTypeId",
                table: "ThingTypeSearchableAttributes",
                column: "ThingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ThingTypeTags_ThingTypeId",
                table: "ThingTypeTags",
                column: "ThingTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThingTypeSearchableAttributes");

            migrationBuilder.DropTable(
                name: "ThingTypeTags");

            migrationBuilder.DropTable(
                name: "ThingTypes");
        }
    }
}
