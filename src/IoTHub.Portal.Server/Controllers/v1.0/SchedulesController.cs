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
    [Route("api/schedule")]
    [ApiExplorerSettings(GroupName = "IoT Schedule")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            this.scheduleService = scheduleService;
        }

        /// <summary>
        /// Creates the schedule.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        [HttpPost(Name = "POST Create schedule")]
        public async Task<IActionResult> CreateScheduleAsync(ScheduleDto schedule)
        {
            ArgumentNullException.ThrowIfNull(schedule, nameof(schedule));

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            _ = await this.scheduleService.CreateSchedule(schedule);

            return Ok(schedule);
        }

        /// <summary>
        /// Updates the specified schedule.
        /// </summary>
        /// <param name="Schedule">The schedule.</param>
        /// <returns>The action result.</returns>
        [HttpPut(Name = "PUT Update the schedule")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSchedule(ScheduleDto Schedule)
        {
            await this.scheduleService.UpdateSchedule(Schedule);

            return Ok();
        }

        /// <summary>
        /// Delete the schedule.
        /// </summary>
        /// <param name="scheduleId">the schedule id.</param>
        /// <returns>Http response</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpDelete("{scheduleId}", Name = "DELETE Remove the schedule")]
        public async Task<IActionResult> DeleteSchedule(string scheduleId)
        {
            await this.scheduleService.DeleteSchedule(scheduleId);

            return NoContent();
        }

        /// <summary>
        /// Gets the specified schedule.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        [HttpGet("{scheduleId}", Name = "GET Schedule")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ScheduleDto))]
        public async Task<IActionResult> GetSchedule(string scheduleId)
        {
            try
            {
                return Ok(await this.scheduleService.GetSchedule(scheduleId));
            }
            catch (DeviceNotFoundException e)
            {
                return StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
        }

        /// <summary>
        /// Gets the schedule list.
        /// </summary>
        /// <returns>An array representing the schedules.</returns>
        [HttpGet(Name = "GET Schedule list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedules()
        {
            return Ok(await this.scheduleService.GetSchedules());
        }
    }
}
