// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.Concentrator;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/concentrators")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    public class LoRaWANConcentratorsController : ControllerBase
    {
        private readonly IDeviceService devicesService;
        private readonly IConcentratorTwinMapper concentratorTwinMapper;
        private readonly IRouterConfigManager routerConfigManager;
        private readonly ILogger<LoRaWANConcentratorsController> logger;

        public LoRaWANConcentratorsController(
            ILogger<LoRaWANConcentratorsController> logger,
            IDeviceService devicesService,
            IRouterConfigManager routerConfigManager,
            IConcentratorTwinMapper concentratorTwinMapper)
        {
            this.devicesService = devicesService;
            this.concentratorTwinMapper = concentratorTwinMapper;
            this.logger = logger;
            this.routerConfigManager = routerConfigManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeviceConcentrator()
        {
            // Gets all the twins from this devices
            var items = await this.devicesService.GetAllDevice(filterDeviceType: "LoRa Concentrator");

            var result = items.Select(this.concentratorTwinMapper.CreateDeviceDetails);

            return this.Ok(result);
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetDeviceConcentrator(string deviceId)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceId);
            return this.Ok(this.concentratorTwinMapper.CreateDeviceDetails(item));
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeviceAsync(Concentrator device)
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
                    DeviceId = device.DeviceId,
                };

                device.RouterConfig = await this.routerConfigManager.GetRouterConfig(device.LoraRegion);

                this.concentratorTwinMapper.UpdateTwin(newTwin, device);

                DeviceStatus status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceId, false, newTwin, status);

                return this.Ok(result);
            }
            catch (DeviceAlreadyExistsException e)
            {
                this.logger?.LogError($"{device.DeviceId} - Create device failed", e);
                return this.BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                this.logger?.LogError("{a0} - Create device failed \n {a1}", device.DeviceId, e.Message);
                return this.BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDeviceAsync(Concentrator device)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            // Device status (enabled/disabled) has to be dealt with afterwards
            Device currentDevice = await this.devicesService.GetDevice(device.DeviceId);
            currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.devicesService.UpdateDevice(currentDevice);

            // Get the current twin from the hub, based on the device ID
            Twin currentTwin = await this.devicesService.GetDeviceTwin(device.DeviceId);
            device.RouterConfig = await this.routerConfigManager.GetRouterConfig(device.LoraRegion);

            // Update the twin properties
            this.concentratorTwinMapper.UpdateTwin(currentTwin, device);

            _ = await this.devicesService.UpdateDeviceTwin(device.DeviceId, currentTwin);

            return this.Ok("Device updated.");
        }

        /// <summary>
        /// this function delete a device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <returns>ok status on success.</returns>
        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> Delete(string deviceId)
        {
            await this.devicesService.DeleteDevice(deviceId);
            return this.Ok("the device was successfully deleted.");
        }
    }
}
