using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTHub.Portal.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceModelIdToPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceModelId",
                table: "Plannings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceModelId",
                table: "Plannings");
        }
    }
}
