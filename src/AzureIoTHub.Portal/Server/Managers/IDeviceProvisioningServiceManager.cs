// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Provisioning.Service;

    public interface IDeviceProvisioningServiceManager
    {
        Task<AttestationMechanism> GetAttestationMechanism(string deviceType);

        Task<EnrollmentGroup> CreateEnrollmentGroupAsync(string deviceType);
    }
}
