// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Shared.Models.Concentrator;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    [Route("api/[controller]")]
    [ApiController]
    public class ConcentratorsController : ControllerBase
    {
        private readonly IDeviceService devicesService;
        private readonly IConcentratorTwinMapper concentratorTwinMapper;
        private readonly IRouterConfigManager routerConfigManager;
        private readonly ILogger<ConcentratorsController> logger;

        public ConcentratorsController(
            ILogger<ConcentratorsController> logger,
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
            try
            {
            // Gets all the twins from this devices
            var items = await this.devicesService.GetAllDevice();
            var itemFilter = items.Where(x => x.Tags["deviceType"] == "LoRa Concentrator");
                List<Concentrator> result = new List<Concentrator>();

                foreach (var item in itemFilter)
                {
                    result.Add(this.concentratorTwinMapper.CreateDeviceDetails(item));
                }

                return this.Ok(result);
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetDeviceConcentrator(string deviceId)
        {
            try
        {
            var item = await this.devicesService.GetDeviceTwin(deviceId);
                return this.Ok(this.concentratorTwinMapper.CreateDeviceDetails(item));
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
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
                this.logger.LogError($"{device.DeviceId} - Create device failed", e);
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
            try
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
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        /// <summary>
        /// this function delete a device.
        /// </summary>
        /// <param name="deviceId">the device id.</param>
        /// <returns>ok status on success.</returns>
        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> Delete(string deviceId)
        {
            try
            {
            await this.devicesService.DeleteDevice(deviceId);
            return this.Ok("the device was successfully deleted.");
        }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }
    }
}
