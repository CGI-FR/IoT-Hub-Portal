// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Settings
{
    public class MSALSettings
    {
        public string Authority { get; set; }

        public string ClientId { get; set; }

        public string ScopeUri { get; set; }

        public bool ValidateAuthority { get; set; }
    }
}
