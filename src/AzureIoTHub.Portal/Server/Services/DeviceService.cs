// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;

    public class DeviceService : IDeviceService
    {
        private readonly RegistryManager registryManager;
        private readonly ServiceClient serviceClient;

        public DeviceService(
            RegistryManager registryManager,
            ServiceClient serviceClient)
        {
            this.serviceClient = serviceClient;
            this.registryManager = registryManager;
        }

        /// <summary>
        /// this function return a list of all edge device wthiout tags.
        /// </summary>
        /// <returns>IEnumerable twin.</returns>
        public async Task<IEnumerable<Twin>> GetAllEdgeDevice()
        {
            IQuery queryEdgeDevice = this.registryManager.CreateQuery("SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' GROUP BY deviceId", 10);

            while (queryEdgeDevice.HasMoreResults)
            {
                return await queryEdgeDevice.GetNextAsTwinAsync();
            }

            return Enumerable.Empty<Twin>();
        }

        /// <summary>
        /// this function get and return the list of all the edge device with the tags.
        /// </summary>
        /// <returns>IEnumerable Twin.</returns>
        public async Task<IEnumerable<Twin>> GetAllEdgeDeviceWithTags()
        {
            IQuery queryEdgeDevice = this.registryManager.CreateQuery("SELECT * FROM devices where devices.capabilities.iotEdge = true", 10);

            while (queryEdgeDevice.HasMoreResults)
            {
                return await queryEdgeDevice.GetNextAsTwinAsync();
            }

            return Enumerable.Empty<Twin>();
        }

        /// <summary>
        /// this function return a list of all device exept edge device.
        /// </summary>
        /// <returns>IEnumerable twin.</returns>
        public async Task<IEnumerable<Twin>> GetAllDevice(string filterDeviceType = null, string excludeDeviceType = null)
        {
            var queryString = "SELECT * FROM devices WHERE devices.capabilities.iotEdge = false";

            if (!string.IsNullOrWhiteSpace(filterDeviceType))
            {
                queryString += $" AND devices.tags.deviceType = '{ filterDeviceType }'";
            }

            if (!string.IsNullOrWhiteSpace(excludeDeviceType))
            {
                queryString += $" AND devices.tags.deviceType != '{ excludeDeviceType }'";
            }

            var query = this.registryManager
                    .CreateQuery(queryString);

            while (query.HasMoreResults)
            {
                return await query.GetNextAsTwinAsync();
            }

            return Enumerable.Empty<Twin>();
        }

        /// <summary>
        /// this function return the device we want with the registry manager.
        /// </summary>
        /// <param name="deviceId">device id.</param>
        /// <returns>Device.</returns>
        public async Task<Device> GetDevice(string deviceId)
        {
            return await this.registryManager.GetDeviceAsync(deviceId);
        }

        /// <summary>
        /// this function use the registry manager to find the twin
        /// of a device.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <returns>Twin of a device.</returns>
        public async Task<Twin> GetDeviceTwin(string deviceId)
        {
            return await this.registryManager.GetTwinAsync(deviceId);
        }

        /// <summary>
        /// this function execute a query with the registry manager to find
        /// the twin of the device with this module.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <returns>Twin of the device.</returns>
        public async Task<Twin> GetDeviceTwinWithModule(string deviceId)
        {
            IQuery devicesWithModules = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeAgent' AND deviceId in ['{deviceId}']");

            while (devicesWithModules.HasMoreResults)
            {
                IEnumerable<Twin> devicesTwins = await devicesWithModules.GetNextAsTwinAsync();

                return devicesTwins.ElementAt(0);
            }

            return null;
        }

        /// <summary>
        /// This function create a new device with his twin.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="isEdge">boolean.</param>
        /// <param name="twin">the twin of my new device.</param>
        /// <param name="isEnabled">the status of the device(disabled by default).</param>
        /// <returns>BulkRegistryOperation.</returns>
        public async Task<BulkRegistryOperationResult> CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin, DeviceStatus isEnabled = DeviceStatus.Disabled)
        {
            var device = new Device(deviceId)
            {
                Capabilities = new DeviceCapabilities { IotEdge = isEdge },
                Status = isEnabled
            };

            return await this.registryManager.AddDeviceWithTwinAsync(device, twin);
        }

        /// <summary>
        /// This function delete a device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        public async Task DeleteDevice(string deviceId)
        {
            await this.registryManager.RemoveDeviceAsync(deviceId);
        }

        /// <summary>
        /// This function update a device.
        /// </summary>
        /// <param name="device">the device id.</param>
        /// <returns>the updated device.</returns>
        public async Task<Device> UpdateDevice(Device device)
        {
            return await this.registryManager.UpdateDeviceAsync(device);
        }

        /// <summary>
        /// This function update the twin of the device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="twin">the new twin.</param>
        /// <returns>the updated twin.</returns>
        public async Task<Twin> UpdateDeviceTwin(string deviceId, Twin twin)
        {
            return await this.registryManager.UpdateTwinAsync(deviceId, twin, twin.ETag);
        }

        /// <summary>
        /// this function execute a methode on the device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="method">the cloud to device method.</param>
        /// <returns>CloudToDeviceMethodResult.</returns>
        public async Task<CloudToDeviceMethodResult> ExecuteC2DMethod(string deviceId, CloudToDeviceMethod method)
        {
            return await this.serviceClient.InvokeDeviceMethodAsync(deviceId, "$edgeAgent", method);
        }
    }
}
