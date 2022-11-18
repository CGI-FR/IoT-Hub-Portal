// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain.Options
{
    public class LoRaWANOptions
    {
        public bool Enabled { get; set; }

        public string? KeyManagementUrl { get; set; }

        public string? KeyManagementCode { get; set; }

        public string? KeyManagementApiVersion { get; set; }
    }
}
