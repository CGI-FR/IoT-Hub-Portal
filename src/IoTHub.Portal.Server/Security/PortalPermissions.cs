// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Security
{
    using Microsoft.AspNetCore.Authorization;
    using Shared.Helpers;

    /// <summary>
    /// Central place where all portal permissions (policies) are declared.
    /// Keep in sync with UI expectations. Only simple (resource:action) policies for now.
    /// </summary>
    public static class PortalPermissions
    {
        /// <summary>
        /// Register every permission as an authorization policy using PermissionRequirement.
        /// </summary>
        public static void AddPolicies(AuthorizationOptions options)
        {
            foreach (var permission in PortalPermissionsHelper.GetAllPermissionStrings())
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
