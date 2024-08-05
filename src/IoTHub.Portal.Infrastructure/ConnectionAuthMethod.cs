// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure
{
    public class ConnectionAuthMethod
    {
        public enum ConnectionAuthScope
        {
            Hub,
            Device,
            Module
        }

        public enum ConnectionAuthType
        {
            Symkey,
            Sas,
            X509
        }

        public ConnectionAuthScope Scope { get; set; }

        public ConnectionAuthType Type { get; set; }

        public string Issuer { get; set; } = default!;
    }
}
