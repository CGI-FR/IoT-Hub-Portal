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
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Mvc.Routing;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/building")]
    [ApiExplorerSettings(GroupName = "IoT Building")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService roomService;

        public RoomsController(IRoomService roomService)
        {
            this.roomService = roomService;
        }

        /// <summary>
        /// Creates the room.
        /// </summary>
        /// <param name="room">The room.</param>
        [HttpPost(Name = "POST Create room")]
        public async Task<IActionResult> CreateRoomAsync(RoomDto room)
        {
            ArgumentNullException.ThrowIfNull(room, nameof(room));

            if (!ModelState.IsValid)
            {
                var validation = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status422UnprocessableEntity
                };

                throw new ProblemDetailsException(validation);
            }

            _ = await this.roomService.CreateRoom(room);

            return Ok(room);
        }


        /// <summary>
        /// Gets the specified room.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        [HttpGet("{roomId}", Name = "GET Room")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoomDto))]
        public async Task<IActionResult> GetRoom(string roomId)
        {
            try
            {
                return Ok(await this.roomService.GetRoom(roomId));
            }
            catch (DeviceNotFoundException e)
            {
                return StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
        }
    }
}
