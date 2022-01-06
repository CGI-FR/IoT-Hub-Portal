// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Identity
{
    using System;
    using Microsoft.Extensions.Configuration;

    public sealed class ClientApiIndentityOptions
    {
        public string Authority { get; set; }

        public string ClientId { get; set; }

        public string ScopeUri { get; set; }

        public bool ValidateAuthority { get; set; } = false;

        public ClientApiIndentityOptions()
        {
        }
    }
}
