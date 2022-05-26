// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/concentrators")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANConcentratorsController : ControllerBase
    {
        /// <summary>
        /// The device Idevice service.
        /// </summary>
        private readonly IDeviceService devicesService;

        /// <summary>
        /// The device IConcentrator twin mapper.
        /// </summary>
        private readonly IConcentratorTwinMapper concentratorTwinMapper;

        /// <summary>
        /// The device IRouter config manager.
        /// </summary>
        private readonly IRouterConfigManager routerConfigManager;

        /// <summary>
        /// The device Lora wan concentrators controller.
        /// </summary>
        private readonly ILogger<LoRaWANConcentratorsController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaWANConcentratorsController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="devicesService">The devices service.</param>
        /// <param name="routerConfigManager">The router config manager.</param>
        /// <param name="concentratorTwinMapper">The concentrator twin mapper.</param>
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

        /// <summary>
        /// Gets all device concentrators.
        /// </summary>
        [HttpGet(Name = "GET LoRaWAN Concentrator list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<Concentrator>>> GetAllDeviceConcentrator()
        {
            // Gets all the twins from this devices
            var result = await this.devicesService.GetAllDevice(filterDeviceType: "LoRa Concentrator");
            string nextPage = null;

            if (!string.IsNullOrEmpty(result.NextPage))
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    Values = new
                    {
                        continuationToken = result.NextPage
                    }
                });
            }

            return Ok(new PaginationResult<Concentrator>
            {
                Items = result.Items.Select(this.concentratorTwinMapper.CreateDeviceDetails),
                TotalItems = result.TotalItems,
                NextPage = nextPage
            });
        }

        /// <summary>
        /// Gets the device concentrator.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        [HttpGet("{deviceId}", Name = "GET LoRaWAN Concentrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Concentrator>> GetDeviceConcentrator(string deviceId)
        {
            var item = await this.devicesService.GetDeviceTwin(deviceId);
            return Ok(this.concentratorTwinMapper.CreateDeviceDetails(item));
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPost(Name = "POST Create LoRaWAN concentrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDeviceAsync(Concentrator device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ArgumentNullException.ThrowIfNull(device, nameof(device));

            try
            {
                // Create a new Twin from the form's fields.
                var newTwin = new Twin()
                {
                    DeviceId = device.DeviceId,
                };

                device.RouterConfig = await this.routerConfigManager.GetRouterConfig(device.LoraRegion);

                device.ClientThumbprint ??= string.Empty;

                this.concentratorTwinMapper.UpdateTwin(newTwin, device);

                var status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceId, false, newTwin, status);

                return Ok(result);
            }
            catch (DeviceAlreadyExistsException e)
            {
                this.logger?.LogError($"{device.DeviceId} - Create device failed", e);
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                this.logger?.LogError($"Create device failed for {device.DeviceId} \n {e.Message}");
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPut(Name = "PUT Update LoRaWAN concentrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDeviceAsync(Concentrator device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ArgumentNullException.ThrowIfNull(device, nameof(device));

            // Device status (enabled/disabled) has to be dealt with afterwards
            var currentDevice = await this.devicesService.GetDevice(device.DeviceId);
            currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.devicesService.UpdateDevice(currentDevice);

            // Get the current twin from the hub, based on the device ID
            var currentTwin = await this.devicesService.GetDeviceTwin(device.DeviceId);
            device.RouterConfig = await this.routerConfigManager.GetRouterConfig(device.LoraRegion);

            // Update the twin properties
            this.concentratorTwinMapper.UpdateTwin(currentTwin, device);

            _ = await this.devicesService.UpdateDeviceTwin(device.DeviceId, currentTwin);

            return Ok("Device updated.");
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        [HttpDelete("{deviceId}", Name = "DELETE Remove LoRaWAN concentrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(string deviceId)
        {
            await this.devicesService.DeleteDevice(deviceId);
            return Ok("the device was successfully deleted.");
        }
    }
}
