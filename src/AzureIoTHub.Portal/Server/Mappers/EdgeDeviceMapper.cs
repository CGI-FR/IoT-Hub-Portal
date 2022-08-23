// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Helpers;
    using Microsoft.Azure.Devices.Shared;

    public class EdgeDeviceMapper : IEdgeDeviceMapper
    {

        public IoTEdgeListItem CreateEdgeDeviceListItem(Twin deviceTwin)
        {
            return new IoTEdgeListItem()
            {
                DeviceId = deviceTwin?.DeviceId,
                Status = deviceTwin?.Status.ToString(),
                Type = DeviceHelper.RetrieveTagValue(deviceTwin, nameof(IoTEdgeDevice.Type)) ?? "Undefined",
                NbDevices = DeviceHelper.RetrieveConnectedDeviceCount(deviceTwin)
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceTwin"></param>
        /// <param name="deviceTwinWithModules"></param>
        /// <param name="nbConnectedDevice"></param>
        /// <param name="lastConfiguration"></param>
        /// <returns></returns>
        public IoTEdgeDevice CreateEdgeDevice(Twin deviceTwin, Twin deviceTwinWithModules, int nbConnectedDevice, ConfigItem lastConfiguration)
        {
            return new IoTEdgeDevice()
            {
                DeviceId = deviceTwin.DeviceId,
                Status = deviceTwin.Status?.ToString(),
                Scope = deviceTwin?.DeviceScope,
                ConnectionState = deviceTwin.ConnectionState?.ToString(),
                Type = DeviceHelper.RetrieveTagValue(deviceTwin, nameof(IoTEdgeDevice.Type)) ?? "Undefined",
                Environment = DeviceHelper.RetrieveTagValue(deviceTwin, "env"),
                NbDevices = nbConnectedDevice,
                NbModules = DeviceHelper.RetrieveNbModuleCount(deviceTwinWithModules, deviceTwin.DeviceId),
                RuntimeResponse = DeviceHelper.RetrieveRuntimeResponse(deviceTwinWithModules),
                Modules = DeviceHelper.RetrieveModuleList(deviceTwinWithModules),
                LastDeployment = lastConfiguration
            };
        }
    }
}
