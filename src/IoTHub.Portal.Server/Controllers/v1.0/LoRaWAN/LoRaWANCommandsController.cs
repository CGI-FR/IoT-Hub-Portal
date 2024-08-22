// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0.LoRaWAN
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Server.Filters;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models.v1._0.LoRaWAN;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/models/{id}/commands")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANCommandsController : ControllerBase
    {
        /// <summary>
        /// The LoRaWAN commands service.
        /// </summary>
        private readonly ILoRaWANCommandService loRaWANCommandService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaWANCommandsController"/> class.
        /// </summary>
        /// <param name="loRaWANCommandService">The LoRaWAN command service</param>
        public LoRaWANCommandsController(ILoRaWANCommandService loRaWANCommandService)
        {
            this.loRaWANCommandService = loRaWANCommandService;
        }

        /// <summary>
        /// Updates the device model's commands.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <param name="commands">The commands.</param>
        /// <returns>The action result.</returns>
        [HttpPost(Name = "POST Set device model commands")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post(string id, DeviceModelCommandDto[] commands)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            ArgumentNullException.ThrowIfNull(commands, nameof(commands));

            await this.loRaWANCommandService.PostDeviceModelCommands(id, commands);

            return Ok();
        }

        /// <summary>
        /// Gets the device model's commands.
        /// </summary>
        /// <param name="id">The model identifier.</param>
        /// <returns>The action result.</returns>
        [HttpGet(Name = "GET Device model commands")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DeviceModelCommandDto[]>> Get(string id)
        {

            ArgumentNullException.ThrowIfNull(id, nameof(id));

            var commands = await this.loRaWANCommandService.GetDeviceModelCommandsFromModel(id);

            return Ok(commands);
        }
    }
}
