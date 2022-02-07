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
            Attestation attestation = null;

            try
            {
                attestation = await this.deviceProvisioningServiceManager.GetAttestation(deviceType);
            }
            catch (ProvisioningServiceClientHttpException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _ = await this.deviceProvisioningServiceManager.CreateEnrollmentGroupAsync(deviceType);
                    attestation = await this.deviceProvisioningServiceManager.GetAttestation(deviceType);
                }

                throw new InvalidOperationException("Failed to get symmetricKey.", e);
            }

            return DeviceHelper.RetrieveSymmetricKey(deviceId, this.CheckAttestation(attestation));
        }

        private SymmetricKeyAttestation CheckAttestation(Attestation attestation)
        {
            var symmetricKeyAttestation = attestation as SymmetricKeyAttestation;

            if (symmetricKeyAttestation == null)
            {
                throw new InvalidOperationException($"Cannot get symmetric key for {attestation.GetType()}.");
            }

            return symmetricKeyAttestation;
        }
    }
}
