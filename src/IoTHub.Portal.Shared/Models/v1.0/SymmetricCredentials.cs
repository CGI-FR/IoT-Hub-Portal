// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{

    /// <summary>
    /// Enrollment credentials.
    /// </summary>
    public class SymmetricCredentials
    {
        /// <summary>
        /// The registration identifier.
        /// </summary>
        public string RegistrationID { get; set; } = default!;

        /// <summary>
        /// The symmetric key.
        /// </summary>
        public string SymmetricKey { get; set; } = default!;

        /// <summary>
        /// The scope identifier.
        /// </summary>
        public string ScopeID { get; set; } = default!;

        /// <summary>
        /// The provisioning endpoint.
        /// </summary>
        public string ProvisioningEndpoint { get; set; } = default!;
    }
}
