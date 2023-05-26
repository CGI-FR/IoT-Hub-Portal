﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureIoTHub.Portal.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddAwsDeploymentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdProvider",
                table: "EdgeDeviceModels",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdProvider",
                table: "EdgeDeviceModels");
        }
    }
}
