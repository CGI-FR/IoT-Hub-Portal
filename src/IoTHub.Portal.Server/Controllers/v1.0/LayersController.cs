// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.V10
{
    using System;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Hellang.Middleware.ProblemDetails;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using System.Collections.Generic;
    using IoTHub.Portal.Domain.Exceptions;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/building")]
    [ApiExplorerSettings(GroupName = "IoT Building")]
    public class LayersController : ControllerBase
    {
        private readonly ILayerService levelService;

        public LayersController(ILayerService levelService)
        {
            this.levelService = levelService;
        }

        /// <summary>
        /// Creates the level.
        /// </summary>
        /// <param name="level">The level.</param>
        [HttpPost(Name = "POST Create level")]
        public async Task<IActionResult> CreateLayerAsync(LayerDto level)
        {
            ArgumentNullException.ThrowIfNull(level, nameof(level));

            _ = await this.levelService.CreateLayer(level);

            return Ok(level);
        }

        /// <summary>
        /// Updates the specified level.
        /// </summary>
        /// <param name="Layer">The level.</param>
        /// <returns>The action result.</returns>
        [HttpPut(Name = "PUT Update the level")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateLayer(LayerDto Layer)
        {
            await this.levelService.UpdateLayer(Layer);

            return Ok();
        }

        /// <summary>
        /// Delete the level.
        /// </summary>
        /// <param name="levelId">the level id.</param>
        /// <returns>Http response</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpDelete("{levelId}", Name = "DELETE Remove the level")]
        public async Task<IActionResult> DeleteLayer(string levelId)
        {
            await this.levelService.DeleteLayer(levelId);

            return NoContent();
        }

        /// <summary>
        /// Gets the specified level.
        /// </summary>
        /// <param name="levelId">The level identifier.</param>
        [HttpGet("{levelId}", Name = "GET Layer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LayerDto))]
        public async Task<IActionResult> GetLayer(string levelId)
        {
            try
            {
                return Ok(await this.levelService.GetLayer(levelId));
            }
            catch (DeviceNotFoundException e)
            {
                return StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
        }

        /// <summary>
        /// Gets the level list.
        /// </summary>
        /// <returns>An array representing the levels.</returns>
        [HttpGet(Name = "GET Layer list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LayerDto>>> GetLayers()
        {
            return Ok(await this.levelService.GetLayers());
        }
    }
}
