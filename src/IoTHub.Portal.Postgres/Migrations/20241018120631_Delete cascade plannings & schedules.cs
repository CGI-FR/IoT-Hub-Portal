// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class Deletecascadeplanningsschedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateIndex(
                name: "IX_Schedules_PlanningId",
                table: "Schedules",
                column: "PlanningId");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Plannings_PlanningId",
                table: "Schedules",
                column: "PlanningId",
                principalTable: "Plannings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Plannings_PlanningId",
                table: "Schedules");

            _ = migrationBuilder.DropIndex(
                name: "IX_Schedules_PlanningId",
                table: "Schedules");
        }
    }
}
