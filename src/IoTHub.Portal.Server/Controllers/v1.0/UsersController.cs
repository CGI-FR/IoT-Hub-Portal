// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace IoTHub.Portal.Server.Controllers.V10
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Http;
    using IoTHub.Portal.Shared.Models.v10;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/users")]
    [ApiExplorerSettings(GroupName = "User Management")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService userManagementService;

        public UsersController(IUserManagementService userManagementService)
        {
            this.userManagementService = userManagementService;
        }

        [HttpGet]
        //[Authorize(Policy = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserModel>))]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userManagementService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            var userDetails = await userManagementService.GetUserDetailsAsync(userId);
            if (userDetails == null)
            {
                return NotFound();
            }
            return Ok(userDetails);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDetailsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] UserDetailsModel userCreateModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userDetails = await userManagementService.CreateUserAsync(userCreateModel);
            return CreatedAtAction(nameof(GetUserDetails), new { userId = userDetails.Id }, userDetails);
        }
    }
}
