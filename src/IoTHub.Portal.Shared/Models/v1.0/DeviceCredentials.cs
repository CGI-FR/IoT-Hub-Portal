// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Shared.Models.v1._0
{
    public enum AuthenticationMode
    {
        SymmetricKey,
        Certificate
    }

    public class DeviceCredentials
    {

        /// <summary>
        /// The authentication mode (either SymmetricKey or Certificate)
        /// </summary>
        public AuthenticationMode AuthenticationMode { get; set; }

        /// <summary>
        /// The SymmetricCredentials if AuthenticationMode == SymmetricKey
        /// </summary>
        public SymmetricCredentials SymmetricCredentials { get; set; }

        /// <summary>
        /// The CertificateCredentials if AuthenticationMode == Certificate
        /// </summary>
        public CertificateCredentials CertificateCredentials { get; set; }

    }
}
