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
    public class LevelsController : ControllerBase
    {
        private readonly ILevelService levelService;

        public LevelsController(ILevelService levelService)
        {
            this.levelService = levelService;
        }

        /// <summary>
        /// Creates the level.
        /// </summary>
        /// <param name="level">The level.</param>
        [HttpPost(Name = "POST Create level")]
        public async Task<IActionResult> CreateLevelAsync(LevelDto level)
        {
            ArgumentNullException.ThrowIfNull(level, nameof(level));

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            _ = await this.levelService.CreateLevel(level);

            return Ok(level);
        }

        /// <summary>
        /// Updates the specified level.
        /// </summary>
        /// <param name="Level">The level.</param>
        /// <returns>The action result.</returns>
        [HttpPut(Name = "PUT Update the level")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateLevel(LevelDto Level)
        {
            await this.levelService.UpdateLevel(Level);

            return Ok();
        }

        /// <summary>
        /// Delete the level.
        /// </summary>
        /// <param name="levelId">the level id.</param>
        /// <returns>Http response</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpDelete("{levelId}", Name = "DELETE Remove the level")]
        public async Task<IActionResult> DeleteLevel(string levelId)
        {
            await this.levelService.DeleteLevel(levelId);

            return NoContent();
        }

        /// <summary>
        /// Gets the specified level.
        /// </summary>
        /// <param name="levelId">The level identifier.</param>
        [HttpGet("{levelId}", Name = "GET Level")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LevelDto))]
        public async Task<IActionResult> GetLevel(string levelId)
        {
            try
            {
                return Ok(await this.levelService.GetLevel(levelId));
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
        [HttpGet(Name = "GET Level list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LevelDto>>> GetLevels()
        {
            return Ok(await this.levelService.GetLevels());
        }
    }
}
