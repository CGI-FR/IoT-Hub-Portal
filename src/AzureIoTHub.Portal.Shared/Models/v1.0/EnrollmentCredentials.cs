// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Models.v10
{
    /// <summary>
    /// Enrollment credentials.
    /// </summary>
    public class EnrollmentCredentials
    {
        /// <summary>
        /// The registration identifier.
        /// </summary>
        public string RegistrationID { get; set; }

        /// <summary>
        /// The symmetric key.
        /// </summary>
        public string SymmetricKey { get; set; }

        /// <summary>
        /// The scope identifier.
        /// </summary>
        public string ScopeID { get; set; }

        /// <summary>
        /// The provisioning endpoint.
        /// </summary>
        public string ProvisioningEndpoint { get; set; }
    }
}
