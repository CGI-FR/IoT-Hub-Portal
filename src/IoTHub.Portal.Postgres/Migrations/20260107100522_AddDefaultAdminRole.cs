// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddDefaultAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable pgcrypto extension for gen_random_uuid() function
            _ = migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            // Add role with all permissions
            _ = migrationBuilder.Sql(
                @"
                INSERT INTO ""Roles"" (""Id"", ""Name"", ""Color"", ""Description"")
                VALUES (gen_random_uuid(), 'Administrators', '#FF0000', 'Default administrators role');
                ");

            // Add actions to the role
            _ = migrationBuilder.Sql(
                @"
                INSERT INTO ""Actions"" (""Id"", ""Name"", ""RoleId"")
                VALUES (gen_random_uuid(), 'group:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'group:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'access-control:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'access-control:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'dashboard:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'device:export', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'device:import', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'device:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'device:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'device-configuration:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'device-configuration:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'model:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'model:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'device-tag:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'device-tag:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'edge-device:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'edge-device:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'edge-device:execute', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'edge-model:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'edge-model:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'idea:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'layer:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'layer:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'planning:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'planning:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'role:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'role:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'user:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'user:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'schedule:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'schedule:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'setting:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'concentrator:read', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')),
                       (gen_random_uuid(), 'concentrator:write', (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators'))
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.Sql(
                @"
                    DELETE FROM ""Actions""
                    WHERE ""RoleId"" = (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'Administrators')
                ");

            _ = migrationBuilder.Sql(
                @"
                DELETE FROM ""Groups""
                WHERE ""Name"" = 'Administrators' AND ""Description"" = 'Default administrators group';
                ");
        }
    }
}
