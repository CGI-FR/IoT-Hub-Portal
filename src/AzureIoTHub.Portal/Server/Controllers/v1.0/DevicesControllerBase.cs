// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10.Device;
    using Microsoft.AspNetCore.Http;
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

        public DevicesControllerBase(
            ILogger logger,
            IDeviceService devicesService,
            IDeviceTwinMapper<TListItem, TModel> deviceTwinMapper)
        {
            this.logger = logger;
            this.devicesService = devicesService;
            this.deviceTwinMapper = deviceTwinMapper;
        }

        /// <summary>
        /// Gets a list of devices as DeviceListItem from Azure IoT Hub.
        /// Fields that do not appear in the device list are not defined here.
        /// </summary>
        /// <returns>A list of DeviceListItem.</returns>
        [HttpGet]
        public async Task<IEnumerable<TListItem>> Get()
        {
            var items = await this.devicesService.GetAllDevice(excludeDeviceType: "LoRa Concentrator");

            return items.Select(this.deviceTwinMapper.CreateDeviceListItem);
        }

        /// <summary>
        /// Retrieve a specific device and from the IoT Hub.
        /// Converts it to a DeviceListItem.
        /// </summary>
        /// <param name="deviceID">ID of the device to retrieve.</param>
        /// <returns>The DeviceListItem corresponding to the given ID.</returns>
        [HttpGet("{deviceID}")]
        public async Task<TModel> Get(string deviceID)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceID);

            return this.deviceTwinMapper.CreateDeviceDetails(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeviceAsync(TModel device)
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
        /// this function update the twin and the device.
        /// </summary>
        /// <param name="device">the device object.</param>
        /// <returns>the update twin.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateDeviceAsync(TModel device)
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
        /// this function delete a device.
        /// </summary>
        /// <param name="deviceID">the device id.</param>
        /// <returns>ok status on success.</returns>
        [HttpDelete("{deviceID}")]
        public async Task<IActionResult> Delete(string deviceID)
        {
            await this.devicesService.DeleteDevice(deviceID);
            return this.Ok();
        }
    }
}
