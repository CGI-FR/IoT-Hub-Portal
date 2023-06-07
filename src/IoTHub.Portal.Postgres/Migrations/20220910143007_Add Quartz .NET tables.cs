// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class AddQuartzNETtables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{currentAssembly.GetName().Name}.Migrations.SQL.UpQuartzNetTables.sql";
            using var stream = currentAssembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            var sqlResult = reader.ReadToEnd();
            _ = migrationBuilder.Sql(sqlResult);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{currentAssembly.GetName().Name}.Migrations.SQL.DownQuartzNetTables.sql";
            using var stream = currentAssembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            var sqlResult = reader.ReadToEnd();
            _ = migrationBuilder.Sql(sqlResult);
        }
    }
}
