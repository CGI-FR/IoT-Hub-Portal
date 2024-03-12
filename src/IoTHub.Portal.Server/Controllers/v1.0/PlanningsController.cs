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

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/planning")]
    [ApiExplorerSettings(GroupName = "IoT Building")]
    public class PlanningsController : ControllerBase
    {
        private readonly IPlanningService planningService;

        public PlanningsController(IPlanningService planningService)
        {
            this.planningService = planningService;
        }

        /// <summary>
        /// Creates the planning.
        /// </summary>
        /// <param name="planning">The planning.</param>
        [HttpPost(Name = "POST Create planning")]
        public async Task<IActionResult> CreatePlanningAsync(PlanningDto planning)
        {
            ArgumentNullException.ThrowIfNull(planning, nameof(planning));

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            _ = await this.planningService.CreatePlanning(planning);

            return Ok(planning);
        }

        /// <summary>
        /// Gets the specified Planning.
        /// </summary>
        /// <param name="PlanningId">The Planning identifier.</param>
        [HttpGet("{PlanningId}", Name = "GET Planning")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlanningDto))]
        public async Task<IActionResult> GetPlanning(string PlanningId)
        {
            try
            {
                return Ok(await this.planningService.GetPlanning(PlanningId));
            }
            catch (DeviceNotFoundException e)
            {
                return StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
        }
    }
}
