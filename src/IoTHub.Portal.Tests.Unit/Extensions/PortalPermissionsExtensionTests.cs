// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Extensions
{
    using System;
    using FluentAssertions;
    using IoTHub.Portal.Shared.Extensions;
    using IoTHub.Portal.Shared.Security;
    using NUnit.Framework;

    [TestFixture]
    public class PortalPermissionsExtensionTests
    {
        [TestCase(PortalPermissions.AccessControlRead, "access-control:read")]
        [TestCase(PortalPermissions.AccessControlWrite, "access-control:write")]
        [TestCase(PortalPermissions.ConcentratorRead, "concentrator:read")]
        [TestCase(PortalPermissions.ConcentratorWrite, "concentrator:write")]
        [TestCase(PortalPermissions.DashboardRead, "dashboard:read")]
        [TestCase(PortalPermissions.DeviceExport, "device:export")]
        [TestCase(PortalPermissions.DeviceImport, "device:import")]
        [TestCase(PortalPermissions.DeviceWrite, "device:write")]
        [TestCase(PortalPermissions.DeviceRead, "device:read")]
        [TestCase(PortalPermissions.DeviceConfigurationRead, "device-configuration:read")]
        [TestCase(PortalPermissions.DeviceConfigurationWrite, "device-configuration:write")]
        [TestCase(PortalPermissions.DeviceTagRead, "device-tag:read")]
        [TestCase(PortalPermissions.DeviceTagWrite, "device-tag:write")]
        [TestCase(PortalPermissions.EdgeDeviceRead, "edge-device:read")]
        [TestCase(PortalPermissions.EdgeDeviceWrite, "edge-device:write")]
        [TestCase(PortalPermissions.EdgeDeviceExecute, "edge-device:execute")]
        [TestCase(PortalPermissions.EdgeModelRead, "edge-model:read")]
        [TestCase(PortalPermissions.EdgeModelWrite, "edge-model:write")]
        [TestCase(PortalPermissions.GroupRead, "group:read")]
        [TestCase(PortalPermissions.GroupWrite, "group:write")]
        [TestCase(PortalPermissions.IdeaWrite, "idea:write")]
        [TestCase(PortalPermissions.LayerRead, "layer:read")]
        [TestCase(PortalPermissions.LayerWrite, "layer:write")]
        [TestCase(PortalPermissions.ModelRead, "model:read")]
        [TestCase(PortalPermissions.ModelWrite, "model:write")]
        [TestCase(PortalPermissions.PlanningRead, "planning:read")]
        [TestCase(PortalPermissions.PlanningWrite, "planning:write")]
        [TestCase(PortalPermissions.RoleRead, "role:read")]
        [TestCase(PortalPermissions.RoleWrite, "role:write")]
        [TestCase(PortalPermissions.ScheduleRead, "schedule:read")]
        [TestCase(PortalPermissions.ScheduleWrite, "schedule:write")]
        [TestCase(PortalPermissions.SettingRead, "setting:read")]
        [TestCase(PortalPermissions.UserRead, "user:read")]
        [TestCase(PortalPermissions.UserWrite, "user:write")]
        public void AsStringShouldReturnExpectedValue(PortalPermissions permission, string expected)
        {
            // Act
            var result = permission.AsString();

            // Assert
            _ = result.Should().Be(expected);
        }

        [Test]
        public void AsStringShouldThrowArgumentOutOfRangeExceptionForUnknownPermission()
        {
            // Arrange
            const PortalPermissions permission = (PortalPermissions)999;

            // Act
            var act = () => permission.AsString();

            // Assert
            _ = act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestCase("access-control:read", PortalPermissions.AccessControlRead)]
        [TestCase("access-control:write", PortalPermissions.AccessControlWrite)]
        [TestCase("concentrator:read", PortalPermissions.ConcentratorRead)]
        [TestCase("concentrator:write", PortalPermissions.ConcentratorWrite)]
        [TestCase("dashboard:read", PortalPermissions.DashboardRead)]
        [TestCase("device:export", PortalPermissions.DeviceExport)]
        [TestCase("device:import", PortalPermissions.DeviceImport)]
        [TestCase("device:write", PortalPermissions.DeviceWrite)]
        [TestCase("device:read", PortalPermissions.DeviceRead)]
        [TestCase("device-configuration:read", PortalPermissions.DeviceConfigurationRead)]
        [TestCase("device-configuration:write", PortalPermissions.DeviceConfigurationWrite)]
        [TestCase("device-tag:read", PortalPermissions.DeviceTagRead)]
        [TestCase("device-tag:write", PortalPermissions.DeviceTagWrite)]
        [TestCase("edge-device:read", PortalPermissions.EdgeDeviceRead)]
        [TestCase("edge-device:write", PortalPermissions.EdgeDeviceWrite)]
        [TestCase("edge-device:execute", PortalPermissions.EdgeDeviceExecute)]
        [TestCase("edge-model:read", PortalPermissions.EdgeModelRead)]
        [TestCase("edge-model:write", PortalPermissions.EdgeModelWrite)]
        [TestCase("group:read", PortalPermissions.GroupRead)]
        [TestCase("group:write", PortalPermissions.GroupWrite)]
        [TestCase("idea:write", PortalPermissions.IdeaWrite)]
        [TestCase("layer:read", PortalPermissions.LayerRead)]
        [TestCase("layer:write", PortalPermissions.LayerWrite)]
        [TestCase("model:read", PortalPermissions.ModelRead)]
        [TestCase("model:write", PortalPermissions.ModelWrite)]
        [TestCase("planning:read", PortalPermissions.PlanningRead)]
        [TestCase("planning:write", PortalPermissions.PlanningWrite)]
        [TestCase("role:read", PortalPermissions.RoleRead)]
        [TestCase("role:write", PortalPermissions.RoleWrite)]
        [TestCase("schedule:read", PortalPermissions.ScheduleRead)]
        [TestCase("schedule:write", PortalPermissions.ScheduleWrite)]
        [TestCase("setting:read", PortalPermissions.SettingRead)]
        [TestCase("user:read", PortalPermissions.UserRead)]
        [TestCase("user:write", PortalPermissions.UserWrite)]
        public void AsPermissionShouldReturnExpectedValue(string permission, PortalPermissions expected)
        {
            // Act
            var result = permission.AsPermission();

            // Assert
            _ = result.Should().Be(expected);
        }

        [Test]
        public void AsPermissionShouldThrowArgumentOutOfRangeExceptionForUnknownString()
        {
            // Arrange
            const string permission = "unknown:permission";

            // Act
            var act = () => permission.AsPermission();

            // Assert
            _ = act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
