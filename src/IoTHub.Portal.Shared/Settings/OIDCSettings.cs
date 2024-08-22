// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Settings
{
    using System;

    /// <summary>
    /// Open ID Connect Settings.
    /// </summary>
    public class OIDCSettings
    {
        /// <summary>
        /// The OIDC Authority.
        /// </summary>
        public string Authority { get; set; } = default!;

        /// <summary>
        /// The OIDC Metadata Url.
        /// </summary>
        public Uri MetadataUrl { get; set; } = default!;

        /// <summary>
        /// The Client Identifier.
        /// </summary>
        public string ClientId { get; set; } = default!;

        /// <summary>
        /// The OIDC Scope.
        /// </summary>
        public string Scope { get; set; } = default!;
    }
}
