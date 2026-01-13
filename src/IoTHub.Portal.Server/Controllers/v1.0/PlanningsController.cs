// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.V10
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/planning")]
    [ApiExplorerSettings(GroupName = "IoT Planning")]
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
        [Authorize("planning:write")]
        [HttpPost(Name = "POST Create planning")]
        public async Task<IActionResult> CreatePlanningAsync(PlanningDto planning)
        {
            ArgumentNullException.ThrowIfNull(planning, nameof(planning));

            _ = await this.planningService.CreatePlanning(planning);

            return Ok(planning);
        }

        /// <summary>
        /// Updates the specified planning.
        /// </summary>
        /// <param name="Planning">The planning.</param>
        /// <returns>The action result.</returns>
        [Authorize("planning:write")]
        [HttpPut(Name = "PUT Update the planning")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePlanning(PlanningDto Planning)
        {
            await this.planningService.UpdatePlanning(Planning);

            return Ok();
        }

        /// <summary>
        /// Delete the planning.
        /// </summary>
        /// <param name="planningId">the planning id.</param>
        /// <returns>Http response</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [Authorize("planning:write")]
        [HttpDelete("{planningId}", Name = "DELETE Remove the planning")]
        public async Task<IActionResult> DeletePlanning(string planningId)
        {
            await this.planningService.DeletePlanning(planningId);

            return NoContent();
        }

        /// <summary>
        /// Gets the specified planning.
        /// </summary>
        /// <param name="planningId">The planning identifier.</param>
        [Authorize("planning:read")]
        [HttpGet("{planningId}", Name = "GET Planning")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlanningDto))]
        public async Task<IActionResult> GetPlanning(string planningId)
        {
            try
            {
                return Ok(await this.planningService.GetPlanning(planningId));
            }
            catch (DeviceNotFoundException e)
            {
                return StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
        }

        /// <summary>
        /// Gets the planning list.
        /// </summary>
        /// <returns>An array representing the plannings.</returns>
        [Authorize("planning:read")]
        [HttpGet(Name = "GET Planning list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PlanningDto>>> GetPlannings()
        {
            return Ok(await this.planningService.GetPlannings());
        }
    }
}
