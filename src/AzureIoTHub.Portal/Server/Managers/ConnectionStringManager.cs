// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using Microsoft.Azure.Devices.Provisioning.Service;

    public class ConnectionStringManager : IConnectionStringManager
    {
        private readonly IDeviceProvisioningServiceManager deviceProvisioningServiceManager;

        public ConnectionStringManager(IDeviceProvisioningServiceManager deviceProvisioningServiceManager)
        {
            this.deviceProvisioningServiceManager = deviceProvisioningServiceManager;
        }

        public async Task<string> GetSymmetricKey(string deviceId, string deviceType)
        {
            try
            {
                var attestationMechanism = await this.deviceProvisioningServiceManager.GetAttestationMechanism(deviceType);

                return DeviceHelper.RetrieveSymmetricKey(deviceId, attestationMechanism);
            }
            catch (ProvisioningServiceClientHttpException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _ = await this.deviceProvisioningServiceManager.CreateEnrollmentGroupAsync(deviceType);
                    var attestationMechanism = await this.deviceProvisioningServiceManager.GetAttestationMechanism(deviceType);

                    return DeviceHelper.RetrieveSymmetricKey(deviceId, attestationMechanism);
                }

                throw new InvalidOperationException("Failed to get symmetricKey.", e);
            }
        }
    }
}
