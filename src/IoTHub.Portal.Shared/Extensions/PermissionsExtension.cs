// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Extensions
{
    using Security;

    public static class PortalPermissionsExtension
    {
        public static string AsString(this PortalPermissions permission)
        {
            return permission switch
            {
                PortalPermissions.AccessControlRead => "access-control:read",
                PortalPermissions.AccessControlWrite => "access-control:write",
                PortalPermissions.ConcentratorRead => "concentrator:read",
                PortalPermissions.ConcentratorWrite => "concentrator:write",
                PortalPermissions.DashboardRead => "dashboard:read",
                PortalPermissions.DeviceExport => "device:export",
                PortalPermissions.DeviceImport => "device:import",
                PortalPermissions.DeviceWrite => "device:write",
                PortalPermissions.DeviceRead => "device:read",
                PortalPermissions.DeviceExecute => "device:execute",
                PortalPermissions.DeviceConfigurationRead => "device-configuration:read",
                PortalPermissions.DeviceConfigurationWrite => "device-configuration:write",
                PortalPermissions.DeviceTagRead => "device-tag:read",
                PortalPermissions.DeviceTagWrite => "device-tag:write",
                PortalPermissions.EdgeDeviceRead => "edge-device:read",
                PortalPermissions.EdgeDeviceWrite => "edge-device:write",
                PortalPermissions.EdgeDeviceExecute => "edge-device:execute",
                PortalPermissions.EdgeModelRead => "edge-model:read",
                PortalPermissions.EdgeModelWrite => "edge-model:write",
                PortalPermissions.GroupRead => "group:read",
                PortalPermissions.GroupWrite => "group:write",
                PortalPermissions.IdeaWrite => "idea:write",
                PortalPermissions.LayerRead => "layer:read",
                PortalPermissions.LayerWrite => "layer:write",
                PortalPermissions.ModelRead => "model:read",
                PortalPermissions.ModelWrite => "model:write",
                PortalPermissions.PlanningRead => "planning:read",
                PortalPermissions.PlanningWrite => "planning:write",
                PortalPermissions.RoleRead => "role:read",
                PortalPermissions.RoleWrite => "role:write",
                PortalPermissions.ScheduleRead => "schedule:read",
                PortalPermissions.ScheduleWrite => "schedule:write",
                PortalPermissions.SettingRead => "setting:read",
                PortalPermissions.UserRead => "user:read",
                PortalPermissions.UserWrite => "user:write",
                _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
            };
        }

        public static PortalPermissions AsPermission(this string permission)
        {
            return permission switch
            {
                "access-control:read" => PortalPermissions.AccessControlRead,
                "access-control:write" => PortalPermissions.AccessControlWrite,
                "concentrator:read" => PortalPermissions.ConcentratorRead,
                "concentrator:write" => PortalPermissions.ConcentratorWrite,
                "dashboard:read" => PortalPermissions.DashboardRead,
                "device:export" => PortalPermissions.DeviceExport,
                "device:import" => PortalPermissions.DeviceImport,
                "device:write" => PortalPermissions.DeviceWrite,
                "device:read" => PortalPermissions.DeviceRead,
                "device:execute" => PortalPermissions.DeviceExecute,
                "device-configuration:read" => PortalPermissions.DeviceConfigurationRead,
                "device-configuration:write" => PortalPermissions.DeviceConfigurationWrite,
                "device-tag:read" => PortalPermissions.DeviceTagRead,
                "device-tag:write" => PortalPermissions.DeviceTagWrite,
                "edge-device:read" => PortalPermissions.EdgeDeviceRead,
                "edge-device:write" => PortalPermissions.EdgeDeviceWrite,
                "edge-device:execute" => PortalPermissions.EdgeDeviceExecute,
                "edge-model:read" => PortalPermissions.EdgeModelRead,
                "edge-model:write" => PortalPermissions.EdgeModelWrite,
                "group:read" => PortalPermissions.GroupRead,
                "group:write" => PortalPermissions.GroupWrite,
                "idea:write" => PortalPermissions.IdeaWrite,
                "layer:read" => PortalPermissions.LayerRead,
                "layer:write" => PortalPermissions.LayerWrite,
                "model:read" => PortalPermissions.ModelRead,
                "model:write" => PortalPermissions.ModelWrite,
                "planning:read" => PortalPermissions.PlanningRead,
                "planning:write" => PortalPermissions.PlanningWrite,
                "role:read" => PortalPermissions.RoleRead,
                "role:write" => PortalPermissions.RoleWrite,
                "schedule:read" => PortalPermissions.ScheduleRead,
                "schedule:write" => PortalPermissions.ScheduleWrite,
                "setting:read" => PortalPermissions.SettingRead,
                "user:read" => PortalPermissions.UserRead,
                "user:write" => PortalPermissions.UserWrite,
                _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
            };
        }
    }
}
