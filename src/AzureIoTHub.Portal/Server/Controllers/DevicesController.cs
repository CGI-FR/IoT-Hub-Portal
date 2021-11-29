﻿// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Models.Device;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    [Authorize(Roles = RoleNames.Admin)]
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> logger;
        private readonly IDeviceService devicesService;
        private readonly ITableClientFactory tableClientFactory;
        private readonly IDeviceTwinMapper deviceTwinMapper;

        public DevicesController(
            ILogger<DevicesController> logger,
            ITableClientFactory tableClientFactory,
            IDeviceService devicesService,
            IDeviceTwinMapper deviceTwinMapper)
        {
            this.logger = logger;
            this.devicesService = devicesService;
            this.tableClientFactory = tableClientFactory;
            this.deviceTwinMapper = deviceTwinMapper;
        }

        /// <summary>
        /// Gets a list of devices as DeviceListItem from Azure IoT Hub.
        /// Fields that do not appear in the device list are not defined here.
        /// </summary>
        /// <returns>A list of DeviceListItem.</returns>
        [HttpGet]
        public async Task<IEnumerable<DeviceListItem>> Get()
        {
            // Gets all the twins from this devices
            var items = await this.devicesService.GetAllDevice();
            var results = new List<DeviceListItem>();

            return items.Select(this.deviceTwinMapper.CreateDeviceListItem);
        }

        /// <summary>
        /// Retrieve a specific device and from the IoT Hub.
        /// Converts it to a DeviceListItem.
        /// </summary>
        /// <param name="deviceID">ID of the device to retrieve.</param>
        /// <returns>The DeviceListItem corresponding to the given ID.</returns>
        [HttpGet("{deviceID}")]
        public async Task<DeviceDetails> Get(string deviceID)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceID);

            return this.deviceTwinMapper.CreateDeviceDetails(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeviceAsync(DeviceDetails device)
        {
            try
            {
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
                this.logger.LogError($"{device.DeviceID} - Create device failed", e);
                return this.BadRequest();
            }
        }

        /// <summary>
        /// this function update the twin and the device.
        /// </summary>
        /// <param name="device">the device object.</param>
        /// <returns>the update twin.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateDeviceAsync(DeviceDetails device)
        {
            try
            {
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
            catch (Exception e)
            {
                this.logger.LogError($"{device.DeviceID} - Update device failed", e);
                return this.BadRequest();
            }
        }

        /// <summary>
        /// this function delete a device.
        /// </summary>
        /// <param name="deviceID">the device id.</param>
        /// <returns>ok status on success.</returns>
        [HttpDelete("{deviceID}")]
        public async Task<IActionResult> Delete(string deviceID)
        {
            try
            {
                await this.devicesService.DeleteDevice(deviceID);
                return this.Ok();
            }
            catch (Exception e)
            {
                this.logger.LogError($"{deviceID} - Device deletion failed", e);
                return this.BadRequest();
            }
        }

        /// <summary>
        /// Permit to execute cloud to device message.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <param name="command">the command who contain the name and the trame.</param>
        /// <returns>a CloudToDeviceMethodResult .</returns>
        [HttpPost("{deviceId}/{methodName}")]
        public async Task<IActionResult> ExecuteCommand(string deviceId, Command command)
        {
            try
            {
                var commandEntity = this.tableClientFactory
                       .GetDeviceCommands()
                       .Query<TableEntity>(filter: $"RowKey  eq '{command.CommandId}'")
                       .Single();

                JsonContent commandContent = JsonContent.Create(new
                {
                    rawPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(commandEntity[nameof(SensorCommand.Frame)].ToString())),
                    fport = commandEntity["port"]
                });

                commandContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var result = await this.devicesService.ExecuteLoraMethod(deviceId, commandContent);

                this.logger.LogInformation($"{result.Content}");

                return this.Ok(await result.Content.ReadFromJsonAsync<dynamic>());
            }
            catch (Exception e)
            {
                this.logger.LogError($"{deviceId} - Execute command on device failed", e);
                return this.BadRequest();
            }
        }
    }
}
