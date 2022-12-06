// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Wrappers
{
    using Microsoft.Azure.Devices.Provisioning.Service;

    public interface IAttestationMechanism
    {
        Attestation GetAttestation();
    }
}
