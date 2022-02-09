// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.Device;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> logger;
        private readonly IDeviceService devicesService;
        private readonly ITableClientFactory tableClientFactory;
        private readonly IDeviceTwinMapper deviceTwinMapper;
        private readonly ILoraDeviceMethodManager loraDeviceMethodManager;
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;

        public DevicesController(
            ILogger<DevicesController> logger,
            ITableClientFactory tableClientFactory,
            IDeviceService devicesService,
            IDeviceTwinMapper deviceTwinMapper,
            ILoraDeviceMethodManager loraDeviceMethodManager,
            IDeviceModelCommandMapper deviceModelCommandMapper)
        {
            this.logger = logger;
            this.devicesService = devicesService;
            this.tableClientFactory = tableClientFactory;
            this.deviceTwinMapper = deviceTwinMapper;
            this.loraDeviceMethodManager = loraDeviceMethodManager;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
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
            var itemFilter = items.Where(x => x.Tags["deviceType"] != "LoRa Concentrator");

            return itemFilter.Select(this.deviceTwinMapper.CreateDeviceListItem);
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
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }
                if (!Eui.TryParse(device.DeviceID, out ulong deviceIdConvert) && device.DeviceType == "LoRa Network Server")
                {
                    throw new InvalidOperationException("the device id is in the wrong format.");
                }

            try
            {
                if (device.DeviceType == "LoRa Concentrator")
                {
                    twinProperties.Desired["NetId"] = 1;
                    twinProperties.Desired["JoinEui"] = new List<string> { "0000000000000000", "FFFFFFFFFFFFFFFF" };
                    twinProperties.Desired["hwspec"] = "sx1301/1";
                    twinProperties.Desired["freq_range"] = new List<string> { "470000000", "510000000" };
                    twinProperties.Desired["nocca"] = true;
                    twinProperties.Desired["nodc"] = true;
                    twinProperties.Desired["nodwell"] = true;
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
        public async Task<IActionResult> UpdateDeviceAsync(DeviceDetails device)
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

        /// <summary>
        /// Permit to execute cloud to device message.
        /// </summary>
        /// <param name="deviceId">id of the device.</param>
        /// <param name="commandId">the command who contain the name and the trame.</param>
        /// <returns>a CloudToDeviceMethodResult .</returns>
        [HttpPost("{deviceId}/{commandId}")]
        public async Task<IActionResult> ExecuteCommand(string deviceId, string commandId)
        {
            try
            {
                var commandEntity = this.tableClientFactory
                       .GetDeviceCommands()
                       .Query<TableEntity>(filter: $"RowKey  eq '{commandId}'")
                       .Single();

                var deviceModelCommand = this.deviceModelCommandMapper.GetDeviceModelCommand(commandEntity);

                var result = await this.loraDeviceMethodManager.ExecuteLoRaDeviceMessage(deviceId, deviceModelCommand);

                if (result.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    this.logger.LogError($"{deviceId} - Execute command on device failed \n {result}");
                    throw new FormatException("Incorrect port or invalid DevEui Format.");
                }

                this.logger.LogInformation($"{deviceId} - Execute command: {result}");

                return this.Ok(await result.Content.ReadFromJsonAsync<dynamic>());
            }
            catch (FormatException e)
            {
                this.logger.LogError($"{deviceId} - Execute command on device failed \n {e.Message}");

                return this.BadRequest("Something went wrong when executing the command.");
            }
        }
    }
}
