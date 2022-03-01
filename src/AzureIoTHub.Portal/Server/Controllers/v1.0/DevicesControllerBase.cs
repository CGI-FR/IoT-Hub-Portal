// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    public abstract class DevicesControllerBase<TListItem, TModel> : ControllerBase
        where TListItem : DeviceListItem
        where TModel : DeviceDetails
    {
        protected readonly ILogger logger;
        private readonly IDeviceService devicesService;
        private readonly IDeviceTwinMapper<TListItem, TModel> deviceTwinMapper;
        private readonly ITableClientFactory tableClientFactory;
        private readonly IDeviceProvisioningServiceManager deviceProvisioningServiceManager;

        public DevicesControllerBase(
            ILogger logger,
            IDeviceService devicesService,
            IDeviceTwinMapper<TListItem, TModel> deviceTwinMapper,
            IDeviceProvisioningServiceManager deviceProvisioningServiceManager,
            ITableClientFactory tableClientFactory)
        {
            this.logger = logger;
            this.devicesService = devicesService;
            this.deviceTwinMapper = deviceTwinMapper;
            this.deviceProvisioningServiceManager = deviceProvisioningServiceManager;
            this.tableClientFactory = tableClientFactory;
        }

        /// <summary>
        /// Gets the device list.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TListItem>> Get()
        {
            var items = await this.devicesService.GetAllDevice(excludeDeviceType: "LoRa Concentrator");

            return items.Select(this.deviceTwinMapper.CreateDeviceListItem);
        }

        /// <summary>
        /// Gets the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns></returns>
        public virtual async Task<TModel> Get(string deviceID)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceID);

            return this.deviceTwinMapper.CreateDeviceDetails(item);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> CreateDeviceAsync(TModel device)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }

                // Create a new Twin from the form's fields.
                var newTwin = new Twin()
                {
                    DeviceId = device.DeviceID
                };

                this.deviceTwinMapper.UpdateTwin(newTwin, device);

                DeviceStatus status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceID, false, newTwin, status);

                return this.Ok(result);
            }
            catch (DeviceAlreadyExistsException e)
            {
                this.logger?.LogError($"{device.DeviceID} - Create device failed", e);
                return this.BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                this.logger?.LogError("{a0} - Create device failed \n {a1}", device.DeviceID, e.Message);
                return this.BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> UpdateDeviceAsync(TModel device)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Device status (enabled/disabled) has to be dealt with afterwards
            Device currentDevice = await this.devicesService.GetDevice(device.DeviceID);
            currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.devicesService.UpdateDevice(currentDevice);

            // Get the current twin from the hub, based on the device ID
            Twin currentTwin = await this.devicesService.GetDeviceTwin(device.DeviceID);

            // Update the twin properties
            this.deviceTwinMapper.UpdateTwin(currentTwin, device);

            _ = await this.devicesService.UpdateDeviceTwin(device.DeviceID, currentTwin);

            return this.Ok();
        }


        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns></returns>
        public virtual async Task<IActionResult> Delete(string deviceID)
        {
            await this.devicesService.DeleteDevice(deviceID);

            return this.Ok();
        }

        /// <summary>
        /// Returns the device enrollment credentials.
        /// </summary>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns></returns>
        public virtual async Task<ActionResult<EnrollmentCredentials>> GetCredentials(string deviceID)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceID);

            if (item == null)
            {
                return this.NotFound("Device doesn't exist.");
            }

            if (!item.Tags.Contains("modelId"))
            {
                return this.BadRequest($"Cannot find device type from device {deviceID}");
            }

            Response<TableEntity> response = await this.tableClientFactory.GetDeviceTemplates()
                                            .GetEntityAsync<TableEntity>("0", item.Tags["modelId"].ToString());

            var modelEntity = response.Value;

            var credentials = await this.deviceProvisioningServiceManager.GetEnrollmentCredentialsAsync(deviceID, modelEntity[nameof(DeviceModel.Name)].ToString());

            return this.Ok(credentials);
        }
    }
}
