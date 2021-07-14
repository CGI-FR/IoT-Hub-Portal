// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Identity
{
    using System;
    using Microsoft.Extensions.Configuration;

    public class B2CExtensionHelper : IB2CExtensionHelper
    {
        private const string RoleName = "Role";

        public string RoleExtensionName => $"extension_{this.mSALConfigSection[MsalSettingsConstants.B2CExtensionAppId].Replace("-", string.Empty)}_{RoleName}";

        public string RoleClaimName => $"extension_{RoleName}";

        private readonly IConfigurationSection mSALConfigSection;

        public B2CExtensionHelper(IConfiguration configuration)
        {
            this.mSALConfigSection = configuration.GetSection(MsalSettingsConstants.RootKey);
        }
    }
}
