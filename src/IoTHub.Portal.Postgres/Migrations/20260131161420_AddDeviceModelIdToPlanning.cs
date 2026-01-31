using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
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
                type: "text",
                nullable: true);
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
