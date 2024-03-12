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
    [Route("api/schedule")]
    [ApiExplorerSettings(GroupName = "IoT Building")]
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
    }
}
