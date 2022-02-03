// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
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

        public async Task<string> GetSymmetricKey(string deviceId)
        {
            try
            {
                var attestationMechanism = await this.deviceProvisioningServiceManager.GetAttestationMechanism();

                return DeviceHelper.RetrieveSymmetricKey(deviceId, attestationMechanism);
            }
            catch (ProvisioningServiceClientHttpException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _ = await this.deviceProvisioningServiceManager.CreateEnrollmentGroupAsync();
                    var attestationMechanism = await this.deviceProvisioningServiceManager.GetAttestationMechanism();

                    return DeviceHelper.RetrieveSymmetricKey(deviceId, attestationMechanism);
                }

                throw new System.Exception(e.Message);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }
    }
}
