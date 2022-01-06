// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Identity
{
    using static AzureIoTHub.Portal.Server.Startup;

    public class B2CExtensionHelper : IB2CExtensionHelper
    {
        private const string RoleName = "Role";

        public string RoleExtensionName => $"extension_{this.configuration.MsalB2CExtensionAppId.Replace("-", string.Empty)}_{RoleName}";

        public string RoleClaimName => $"extension_{RoleName}";

        private readonly ConfigHandler configuration;

        internal B2CExtensionHelper(ConfigHandler configuration)
        {
            this.configuration = configuration;
        }
    }
}
