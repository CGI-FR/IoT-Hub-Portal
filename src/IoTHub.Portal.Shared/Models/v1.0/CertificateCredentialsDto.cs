// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Models.v10
{

    /// <summary>
    /// Certificate credentials.
    /// </summary>
    public class CertificateCredentialsDto
    {
        /// <summary>
        /// The Certificate Pem
        /// </summary>
        public string CertificatePem { get; set; } = default!;

        /// <summary>
        /// The public key
        /// </summary>
        public string PublicKey { get; set; } = default!;

        /// <summary>
        /// The private key
        /// </summary>
        public string PrivateKey { get; set; } = default!;
    }
}
