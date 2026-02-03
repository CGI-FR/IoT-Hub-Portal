// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable disable

namespace IoTHub.Portal.Postgres.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddDefaultAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Generate a fixed GUID for the Administrators role
            var administratorsRoleId = Guid.NewGuid().ToString();

            // Add role with all permissions
            _ = migrationBuilder.Sql(
                $@"
                INSERT INTO ""Roles"" (""Id"", ""Name"", ""Color"", ""Description"")
                VALUES ('{administratorsRoleId}', 'Administrators', '#FF0000', 'Default administrators role');
                ");

            // Add actions to the role
            _ = migrationBuilder.Sql(
                $@"
                INSERT INTO ""Actions"" (""Id"", ""Name"", ""RoleId"")
                VALUES ('{Guid.NewGuid()}', 'group:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'group:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'access-control:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'access-control:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'dashboard:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'device:export', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'device:import', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'device:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'device:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'device-configuration:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'device-configuration:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'model:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'model:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'device-tag:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'device-tag:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'edge-device:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'edge-device:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'edge-device:execute', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'edge-model:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'edge-model:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'idea:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'layer:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'layer:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'planning:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'planning:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'role:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'role:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'user:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'user:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'schedule:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'schedule:write', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'setting:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'concentrator:read', '{administratorsRoleId}'),
                       ('{Guid.NewGuid()}', 'concentrator:write', '{administratorsRoleId}')
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
