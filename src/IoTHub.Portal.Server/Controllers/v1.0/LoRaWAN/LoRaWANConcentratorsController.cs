// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0.LoRaWAN
{
    using System;
    using System.Threading.Tasks;
    using Hellang.Middleware.ProblemDetails;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Server.Filters;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Logging;
    using Shared;
    using Shared.Models.v1._0.Filters;
    using Shared.Models.v1._0.LoRaWAN;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/concentrators")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANConcentratorsController : ControllerBase
    {
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
        /// <param name="loRaWANConcentratorService">The device Lora wan concentrators controller.</param>
        public LoRaWANConcentratorsController(
            ILogger<LoRaWANConcentratorsController> logger,
            ILoRaWANConcentratorService loRaWANConcentratorService)
        {
            this.logger = logger;
            this.loRaWANConcentratorService = loRaWANConcentratorService;
        }

        /// <summary>
        /// Gets all device concentrators.
        /// </summary>
        [HttpGet(Name = "GET LoRaWAN Concentrator list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<ConcentratorDto>>> GetAllDeviceConcentrator([FromQuery] ConcentratorFilter concentratorFilter)
        {
            var paginatedDevices = await this.loRaWANConcentratorService.GetAllDeviceConcentrator(concentratorFilter);

            var nextPage = string.Empty;

            if (paginatedDevices.HasNextPage)
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    RouteName = "GET LoRaWAN Concentrator list",
                    Values = new
                    {
                        concentratorFilter.PageSize,
                        pageNumber = concentratorFilter.PageNumber + 1,
                        concentratorFilter.OrderBy
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
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            _ = await this.loRaWANConcentratorService.CreateDeviceAsync(device);

            return Ok(device);
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
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            _ = await this.loRaWANConcentratorService.UpdateDeviceAsync(device);

            return Ok(device);
        }

        /// <summary>
        /// Deletes the specified device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        [HttpDelete("{deviceId}", Name = "DELETE Remove LoRaWAN concentrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(string deviceId)
        {
            await this.loRaWANConcentratorService.DeleteDeviceAsync(deviceId);

            return Ok();
        }
    }
}
