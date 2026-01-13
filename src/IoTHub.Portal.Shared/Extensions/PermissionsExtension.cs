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
    }
}
