// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using AzureIoTHub.Portal.Server.Interfaces;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;

    public class DevicesServices : IDevicesService
    {
        private readonly RegistryManager registryManager;

        public DevicesServices(
            RegistryManager registryManager)
        {
            this.registryManager = registryManager;
        }

        /// <summary>
        /// This function create a new device with his twin.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="isEdge">boolean.</param>
        /// <param name="twin">the twin of my new device.</param>
        /// <returns>BulkRegistryOperation.</returns>
        public BulkRegistryOperationResult CreateDeviceWithTwin(string deviceId, bool isEdge, Twin twin)
        {
            try
            {
                Device device = new (deviceId)
                {
                    Capabilities = new DeviceCapabilities { IotEdge = isEdge },
                };

                return this.registryManager.AddDeviceWithTwinAsync(device, twin).Result;
            }
            catch (DeviceAlreadyExistsException e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// This function delete a device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        public void Delete(string deviceId)
        {
            this.registryManager.RemoveDeviceAsync(deviceId).Wait();
        }

        public IEnumerable<Twin> GetAllEdgeDevice()
        {
            try
            {
                IQuery queryEdgeDevice = this.registryManager.CreateQuery("SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' GROUP BY deviceId", 10);

                while (queryEdgeDevice.HasMoreResults)
                {
                    return queryEdgeDevice.GetNextAsTwinAsync().Result;
                }

                return Enumerable.Empty<Twin>();
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// this function get and return the list of all the edge device with the tags.
        /// </summary>
        /// <returns>IEnumerable Twin.</returns>
        public IEnumerable<Twin> GetAllEdgeDeviceWithTags()
        {
            try
            {
                IQuery queryEdgeDevice = this.registryManager.CreateQuery("SELECT * FROM devices where devices.capabilities.iotEdge = true", 10);

                while (queryEdgeDevice.HasMoreResults)
                {
                    return queryEdgeDevice.GetNextAsTwinAsync().Result;
                }

                return Enumerable.Empty<Twin>();
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// this function use the registry manager to find the twin
        /// of a device.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <returns>Twin of a device.</returns>
        public Twin GetDeviceTwin(string deviceId)
        {
            try
            {
                return this.registryManager.GetTwinAsync(deviceId).Result;
            }
            catch (DeviceNotFoundException e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// this function execute a query with the registry manager to find
        /// the twin of the device with this module.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <returns>Twin of the device.</returns>
        public Twin GetDeviceWithModule(string deviceId)
        {
            try
            {
                IQuery devicesWithModules = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeAgent' AND deviceId in ['{deviceId}']");

                while (devicesWithModules.HasMoreResults)
                {
                    IEnumerable<Twin> devicesTwins = devicesWithModules.GetNextAsTwinAsync().Result;

                    return devicesTwins.ElementAt(0);
                }

                return null;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// This function update a device.
        /// </summary>
        /// <param name="device">the device id.</param>
        /// <returns>the updated device.</returns>
        public Device UpdateDevice(Device device)
        {
            try
            {
                return this.registryManager.UpdateDeviceAsync(device).Result;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// This function update the twin of the device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="twin">the new twin.</param>
        /// <returns>the updated twin.</returns>
        public Twin UpdateDeviceTwin(string deviceId, Twin twin)
        {
            try
            {
                return this.registryManager.UpdateTwinAsync(deviceId, twin, twin.ETag).Result;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
