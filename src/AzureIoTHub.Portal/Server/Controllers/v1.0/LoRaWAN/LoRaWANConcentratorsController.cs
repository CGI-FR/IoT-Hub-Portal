// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN
{
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Filters;
    using Mappers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Extensions.Logging;
    using Services;

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
        private readonly IExternalDeviceService externalDevicesService;

        /// <summary>
        /// The device IConcentrator twin mapper.
        /// </summary>
        private readonly IConcentratorTwinMapper concentratorTwinMapper;

        /// <summary>
        /// The LoRaWAN concentrator service.
        /// </summary>
        private readonly ILoRaWANConcentratorService loRaWANConcentratorService;

        /// <summary>
        /// The device Lora wan concentrators controller.
        /// </summary>
        private readonly ILogger<LoRaWANConcentratorsController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaWANConcentratorsController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="externalDevicesService">The devices service.</param>
        /// <param name="concentratorTwinMapper">The concentrator twin mapper.</param>
        /// <param name="loRaWANConcentratorService">The device Lora wan concentrators controller.</param>
        public LoRaWANConcentratorsController(
            ILogger<LoRaWANConcentratorsController> logger,
            IExternalDeviceService externalDevicesService,
            IConcentratorTwinMapper concentratorTwinMapper,
            ILoRaWANConcentratorService loRaWANConcentratorService)
        {
            this.externalDevicesService = externalDevicesService;
            this.concentratorTwinMapper = concentratorTwinMapper;
            this.logger = logger;
            this.loRaWANConcentratorService = loRaWANConcentratorService;
        }

        /// <summary>
        /// Gets all device concentrators.
        /// </summary>
        [HttpGet(Name = "GET LoRaWAN Concentrator list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<ConcentratorDto>>> GetAllDeviceConcentrator(
            string routeName = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null)
        {

            var paginatedDevices = await this.loRaWANConcentratorService.GetAllDeviceConcentrator(
                pageSize,
                pageNumber,
                orderBy);

            var nextPage = string.Empty;

            if (paginatedDevices.HasNextPage)
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    RouteName = routeName,
                    Values = new
                    {
                        pageSize,
                        pageNumber = pageNumber + 1,
                        orderBy
                    }
                });
            }

            return new PaginationResult<ConcentratorDto>
            {
                Items = paginatedDevices.Data,
                TotalItems = paginatedDevices.TotalCount,
                NextPage = nextPage
            };
        }

        /// <summary>
        /// Gets the device concentrator.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        [HttpGet("{deviceId}", Name = "GET LoRaWAN Concentrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ConcentratorDto>> GetDeviceConcentrator(string deviceId)
        {
            return Ok(await this.loRaWANConcentratorService.GetConcentrator(deviceId));
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPost(Name = "POST Create LoRaWAN concentrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDeviceAsync(ConcentratorDto device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ArgumentNullException.ThrowIfNull(device, nameof(device));

            try
            {
                if (await this.loRaWANConcentratorService.CreateDeviceAsync(device))
                {
                    return this.Ok();
                }
            }
            catch (DeviceAlreadyExistsException e)
            {
                return this.BadRequest(e.Message);
            }

            return this.BadRequest();
        }

        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        [HttpPut(Name = "PUT Update LoRaWAN concentrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDeviceAsync(ConcentratorDto device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _ = await this.loRaWANConcentratorService.UpdateDeviceAsync(device);

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
            await this.externalDevicesService.DeleteDevice(deviceId);
            return Ok("the device was successfully deleted.");
        }
    }
}
