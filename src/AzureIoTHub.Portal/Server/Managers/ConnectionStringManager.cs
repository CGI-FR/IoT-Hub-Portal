// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Services;

    public class ConnectionStringManager : IConnectionStringManager
    {
        private readonly IDeviceService deviceService;

        public ConnectionStringManager(IDeviceService deviceService)
        {
            this.deviceService = deviceService;
        }

        public async Task<string> GetSymmetricKey(string deviceId)
        {
            try
            {
                var attestationMechanism = await this.deviceService.GetDpsAttestionMechanism();

                return DeviceHelper.RetrieveSymmetricKey(deviceId, attestationMechanism);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }
    }
}
