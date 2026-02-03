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
            // Use a fixed GUID for the Administrators role to ensure consistency across environments
            const string administratorsRoleId = "a8e8f0c6-4c6e-4f8a-9e3e-7a5b8c9d0e1f";

            // Add role with all permissions
            _ = migrationBuilder.Sql(
                $@"
                INSERT INTO ""Roles"" (""Id"", ""Name"", ""Color"", ""Description"")
                VALUES ('{administratorsRoleId}', 'Administrators', '#FF0000', 'Default administrators role');
                ");

            // Add actions to the role with fixed GUIDs to ensure consistency across environments
            _ = migrationBuilder.Sql(
                $@"
                INSERT INTO ""Actions"" (""Id"", ""Name"", ""RoleId"")
                VALUES ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e11', 'group:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e12', 'group:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e13', 'access-control:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e14', 'access-control:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e15', 'dashboard:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e16', 'device:export', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e17', 'device:import', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e18', 'device:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e19', 'device:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e1a', 'device-configuration:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e1b', 'device-configuration:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e1c', 'model:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e1d', 'model:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e1e', 'device-tag:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e1f', 'device-tag:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e20', 'edge-device:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e21', 'edge-device:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e22', 'edge-device:execute', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e23', 'edge-model:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e24', 'edge-model:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e25', 'idea:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e26', 'layer:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e27', 'layer:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e28', 'planning:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e29', 'planning:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e2a', 'role:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e2b', 'role:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e2c', 'user:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e2d', 'user:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e2e', 'schedule:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e2f', 'schedule:write', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e30', 'setting:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e31', 'concentrator:read', '{administratorsRoleId}'),
                       ('b1e1f1c1-5d1f-4f1a-8e1e-1a1b1c1d1e32', 'concentrator:write', '{administratorsRoleId}')
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
