// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Identity
{
    using System;

    public sealed class ClientApiIndentityOptions
    {
        public string Authority { get; set; }

        public Uri MetadataUrl { get; set; }

        public string ClientId { get; set; }

        public string Scope { get; set; }
    }
}
