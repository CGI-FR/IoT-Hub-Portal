// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Security
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Central place where all portal permissions (policies) are declared.
    /// Keep in sync with UI expectations. Only simple (resource:action) policies for now.
    /// </summary>
    public static class PortalPermissions
    {
        public static readonly string[] All = new[]
        {
            "group:read",
            "group:write",
            "access-control:read",
            "access-control:write",
            "dashboard:read",
            "device:export",
            "device:import",
            "device:write",
            "device:read",
            "device-configuration:read",
            "device-configuration:write",
            "model:read",
            "model:write",
            "device-tag:read",
            "device-tag:write",
            "edge-device:read",
            "edge-device:write",
            "edge-device:execute",
            "edge-model:read",
            "edge-model:write",
            "idea:write",
            "layer:read",
            "layer:write",
            "planning:read",
            "planning:write",
            "role:read",
            "role:write",
            "user:read",
            "user:write",
            "schedule:read",
            "schedule:write",
            "setting:read",
            "concentrator:read",
            "concentrator:write"
        };

        /// <summary>
        /// Register every permission as an authorization policy using PermissionRequirement.
        /// </summary>
        public static void AddPolicies(AuthorizationOptions options)
        {
            foreach (var permission in All)
            {
                // Avoid duplicate registration (e.g. if called twice in tests)
                if (options.GetPolicy(permission) is null)
                {
                    options.AddPolicy(permission, policy =>
                        policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            }
        }
    }
}
