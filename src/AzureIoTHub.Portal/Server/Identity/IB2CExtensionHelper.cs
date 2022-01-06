// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Identity
{
    public interface IB2CExtensionHelper
    {
        string RoleExtensionName { get; }

        string RoleClaimName { get; }
    }
}
