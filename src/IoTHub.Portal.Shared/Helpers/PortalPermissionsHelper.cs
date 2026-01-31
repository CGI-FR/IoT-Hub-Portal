// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace IoTHub.Portal.Shared.Helpers
{
    using System;
    using System.Linq;

    using Extensions;
    using Security;

    public static class PortalPermissionsHelper
    {
        /// <summary>
        /// Gets all permission strings from the PortalPermissions enum
        /// </summary>
        public static string[] GetAllPermissionStrings()
        {
            return Enum.GetValues<PortalPermissions>()
                .Select(p => p.AsString())
                .ToArray();
        }

        /// <summary>
        /// Gets all PortalPermissions enum values
        /// </summary>
        public static PortalPermissions[] GetAllPermissions()
        {
            return Enum.GetValues<PortalPermissions>();
        }
    }
}
