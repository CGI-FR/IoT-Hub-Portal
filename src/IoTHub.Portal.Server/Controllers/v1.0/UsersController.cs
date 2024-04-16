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

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/users")]
    [ApiExplorerSettings(GroupName = "User Management")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService userManagementService;
        private readonly ILogger<UsersController> logger;

        public UsersController(IUserManagementService userManagementService, ILogger<UsersController> logger)
        {
            this.userManagementService = userManagementService;
            this.logger = logger;
        }

        [HttpGet(Name = "Get Users")]
        //[Authorize(Policy = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserModel>))]
        public async Task<PaginationResult<UserModel>> Get(
            string searchName = null,
            string searchEmail = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null
            )
        {
            var paginedResult = await userManagementService.GetUserPage(
                searchName,
                searchEmail,
                pageSize,
                pageNumber,
                orderBy
                );
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

        [HttpPost(Name = "POST Create an User")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDetailsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] UserDetailsModel userCreateModel)
        {
            return Ok(await this.userManagementService.CreateUserAsync(userCreateModel));

        }

        [HttpPut(Name = "PUT Edit User")]
        //[Authorize(Policy = Policies.EditRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditUserAsync(UserDetailsModel userDetails)
        {
            return Ok(await this.userManagementService.UpdateUser(userDetails));
        }


        /// <summary>
        /// Delete an user by id
        /// </summary>
        /// <param name="id">User id that we want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            return Ok(await userManagementService.DeleteUser(id));
        }
    }
}
