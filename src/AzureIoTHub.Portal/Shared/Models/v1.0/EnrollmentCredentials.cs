// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models.v10
{
    public class EnrollmentCredentials
    {
        public string RegistrationID { get; set; }

        public string SymmetricKey { get; set; }

        public string ScopeID { get; set; }

        public string ProvisioningEndpoint { get; set; }
    }
}
