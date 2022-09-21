using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureIoTHub.Portal.Infrastructure.Migrations
{
    public partial class AddModuleNamePropertyonEdgeDeviceModelCommand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModuleName",
                table: "EdgeDeviceModelCommands",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModuleName",
                table: "EdgeDeviceModelCommands");
        }
    }
}
