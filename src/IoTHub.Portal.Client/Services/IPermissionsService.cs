// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    using IoTHub.Portal.Shared.Security;

    public interface IPermissionsService
    {
        Task<PortalPermissions[]> GetUserPermissions();
    }
}
