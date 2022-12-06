// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Wrappers
{
    using AzureIoTHub.Portal.Application.Wrappers;
    using Microsoft.Azure.Devices.Provisioning.Service;

    public class AttestationMechanismWrapper : IAttestationMechanism
    {
        private readonly AttestationMechanism mechanism;

        public AttestationMechanismWrapper(AttestationMechanism attestationMechanism)
        {
            this.mechanism = attestationMechanism;
        }

        public Attestation GetAttestation()
        {
            return this.mechanism.GetAttestation();
        }
    }
}
