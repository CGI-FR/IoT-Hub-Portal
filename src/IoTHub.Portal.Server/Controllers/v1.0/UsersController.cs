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
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Logging;
    using System;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/users")]
    [ApiExplorerSettings(GroupName = "User Management")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService userManagementService;
        private readonly IAccessControlManagementService accessControlService;

        private readonly ILogger<UsersController> logger;

        public UsersController(IUserManagementService userManagementService, ILogger<UsersController> logger, IAccessControlManagementService accessControlService)
        {
            this.userManagementService = userManagementService;
            this.logger = logger;
            this.accessControlService = accessControlService;
        }

        [HttpGet(Name = "Get Users")]
        [Authorize("user:read")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserModel>))]
        public async Task<PaginationResult<UserModel>> Get(
            [FromQuery] string searchName = null,
            [FromQuery] string searchEmail = null,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 0,
            [FromQuery] string[] orderBy = null
            )
        {
            try
            {
                var paginedResult = await userManagementService.GetUserPage(
                searchName,
                searchEmail,
                pageSize,
                pageNumber,
                orderBy
                );
                logger.LogInformation("Users fetched successfully. Total users fetched: {Count}", paginedResult.TotalCount);
                var nextPage = string.Empty;
                if (paginedResult.HasNextPage)
                {
                    nextPage = Url.RouteUrl(new UrlRouteContext
                    {
                        RouteName = "Get Users",
                        Values = new { searchName, searchEmail, pageSize, pageNumber = pageNumber + 1, orderBy }
                    });
                }
                return new PaginationResult<UserModel>
                {
                    Items = paginedResult.Data,
                    TotalItems = paginedResult.TotalCount,
                    NextPage = nextPage
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching users.");
                throw;
            }
        }

        [HttpGet("{id}")]
        [Authorize("user:read")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            try
            {
                var userDetails = await userManagementService.GetUserDetailsAsync(id);
                if (userDetails == null)
                {
                    logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound();
                }
                var accessControls = await accessControlService.GetAccessControlPage(null,100, 0,null, userDetails.PrincipalId);
                if (accessControls.Data is not null)
                {
                    foreach (var ac in accessControls.Data)
                    {
                        userDetails.AccessControls.Add(ac);
                    }
                }
                logger.LogInformation("Details retrieved for user with ID {UserId}", id);
                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get details for user with ID {UserId}", id);
                throw;
            }
        }

        [HttpPost(Name = "POST Create an User")]
        [Authorize("user:write")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDetailsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] UserDetailsModel user)
        {
            try
            {
                var result = await this.userManagementService.CreateUserAsync(user);
                logger.LogInformation("User created successfully with ID {UserId}", result.Id);
                return CreatedAtAction(nameof(GetUserDetails), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create user.");
                return BadRequest();
            }
        }

        [HttpPut("{id}", Name = "PUT Edit User")]
        [Authorize("user:write")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditUserAsync(string id, UserDetailsModel user)
        {
            try
            {
                var result = await this.userManagementService.UpdateUser(id, user);
                logger.LogInformation("User with ID {UserId} updated successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update user with ID {UserId}", id);
                throw;
            }
        }


        /// <summary>
        /// Delete an user by id
        /// </summary>
        /// <param name="id">User id that we want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize("user:write")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var result = await userManagementService.DeleteUser(id);
                if (!result)
                {
                    logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound("User not found.");
                }
                logger.LogInformation("User with ID {UserId} deleted successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete user with ID {UserId}", id);
                throw;
            }
        }
    }
}
