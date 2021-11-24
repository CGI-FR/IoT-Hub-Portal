// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Interfaces;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Provisioning.Service;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;

    public class DevicesServices : IDevicesService
    {
        private readonly RegistryManager registryManager;
        private readonly ProvisioningServiceClient dps;
        private readonly ServiceClient serviceClient;
        private readonly HttpClient http;
        private readonly IConfiguration configuration;

        public DevicesServices(
            IConfiguration configuration,
            RegistryManager registryManager,
            ServiceClient serviceClient,
            HttpClient http,
            ProvisioningServiceClient dps)
        {
            this.dps = dps;
            this.http = http;
            this.configuration = configuration;
            this.serviceClient = serviceClient;
            this.registryManager = registryManager;
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
            try
            {
                Device device = new (deviceId)
                {
                    Capabilities = new DeviceCapabilities { IotEdge = isEdge },
                    Status = isEnabled
                };

                return await this.registryManager.AddDeviceWithTwinAsync(device, twin);
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
        public async Task DeleteDevice(string deviceId)
        {
            await this.registryManager.RemoveDeviceAsync(deviceId);
        }

        /// <summary>
        /// this.function return a list of all edge device.
        /// </summary>
        /// <returns>Ienumerable twin.</returns>
        public async Task<IEnumerable<Twin>> GetAllEdgeDevice()
        {
            try
            {
                IQuery queryEdgeDevice = this.registryManager.CreateQuery("SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeHub' GROUP BY deviceId", 10);

                while (queryEdgeDevice.HasMoreResults)
                {
                    return await queryEdgeDevice.GetNextAsTwinAsync();
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
        public async Task<IEnumerable<Twin>> GetAllEdgeDeviceWithTags()
        {
            try
            {
                IQuery queryEdgeDevice = this.registryManager.CreateQuery("SELECT * FROM devices where devices.capabilities.iotEdge = true", 10);

                while (queryEdgeDevice.HasMoreResults)
                {
                    return await queryEdgeDevice.GetNextAsTwinAsync();
                }

                return Enumerable.Empty<Twin>();
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// this function return the device we want with the registry manager.
        /// </summary>
        /// <param name="deviceId">device id.</param>
        /// <returns>Device.</returns>
        public async Task<Device> GetDevice(string deviceId)
        {
            try
            {
                return await this.registryManager.GetDeviceAsync(deviceId);
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
        public async Task<Twin> GetDeviceTwin(string deviceId)
        {
            try
            {
                return await this.registryManager.GetTwinAsync(deviceId);
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
        public async Task<Twin> GetDeviceTwinWithModule(string deviceId)
        {
            try
            {
                IQuery devicesWithModules = this.registryManager.CreateQuery($"SELECT * FROM devices.modules WHERE devices.modules.moduleId = '$edgeAgent' AND deviceId in ['{deviceId}']");

                while (devicesWithModules.HasMoreResults)
                {
                    IEnumerable<Twin> devicesTwins = await devicesWithModules.GetNextAsTwinAsync();

                    return devicesTwins.ElementAt(0);
                }

                return null;
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// this function get the attestation mechanism of the DPS.
        /// </summary>
        /// <returns>AttestationMechanism.</returns>
        public async Task<AttestationMechanism> GetDpsAttestionMechanism()
        {
            return await this.dps.GetEnrollmentGroupAttestationAsync(this.configuration["IoTDPS:DefaultEnrollmentGroupe"]);
        }

        /// <summary>
        /// This function update a device.
        /// </summary>
        /// <param name="device">the device id.</param>
        /// <returns>the updated device.</returns>
        public async Task<Device> UpdateDevice(Device device)
        {
            try
            {
                return await this.registryManager.UpdateDeviceAsync(device);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// This function update the twin of the device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="twin">the new twin.</param>
        /// <returns>the updated twin.</returns>
        public async Task<Twin> UpdateDeviceTwin(string deviceId, Twin twin)
        {
            try
            {
                return await this.registryManager.UpdateTwinAsync(deviceId, twin, twin.ETag);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// this function execute a methode on the device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="method">the cloud to device method.</param>
        /// <returns>CloudToDeviceMethodResult.</returns>
        public async Task<CloudToDeviceMethodResult> ExecuteC2DMethod(string deviceId, CloudToDeviceMethod method)
        {
            try
            {
                return await this.serviceClient.InvokeDeviceMethodAsync(deviceId, "$edgeAgent", method);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }

        /// <summary>
        /// this function execute a methode on a lora device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <param name="commandContent">the command.</param>
        /// <returns>HttpResponseMessage.</returns>
        public async Task<HttpResponseMessage> ExecuteLoraMethod(string deviceId, JsonContent commandContent)
        {
            try
            {
                return await this.http.PostAsync($"{this.configuration["IoTAzureFunction:url"]}/{deviceId}{this.configuration["IoTAzureFunction:code"]}", commandContent);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
            }
        }
    }
}
