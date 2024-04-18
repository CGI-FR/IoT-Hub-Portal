// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace IoTHub.Portal.Server.Controllers.V10
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Http;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Mvc.Routing;

    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/roles")]
    [ApiExplorerSettings(GroupName = "Role Management")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        /// <summary>
        /// The Role Controller logger :
        /// </summary>
        private readonly ILogger<RolesController> logger;

        /// <summary>
        /// The Role Management Service :
        /// </summary>
        private readonly IRoleManagementService roleManagementService;

        /// <summary>
        /// Constructor : Initialize an instance of the <see cref="RoleController"/>.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="roleManagementService">The Role Management Service</param>
        public RolesController(IRoleManagementService roleManagementService, ILogger<RolesController> logger)
        {
            this.roleManagementService = roleManagementService;
            this.logger = logger;
        }

        /// <summary>
        ///  Get all roles
        /// </summary>
        /// <param name="searchName">Role name</param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="orderBy"></param>
        /// <param name="RoleId"></param>
        [HttpGet(Name = "GetRoles")]
        //[Authorize(Policy = "GetRoles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResult<RoleModel>))]
        public async Task<PaginationResult<RoleModel>> Get(
            string searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null
        )
        {
            var paginedResult = await roleManagementService.GetRolePage(
                searchKeyword,
                pageSize,
                pageNumber,
                orderBy
            );
            var nextPage = string.Empty;
            if (paginedResult.HasNextPage)
            {
                nextPage = Url.RouteUrl(new UrlRouteContext
                {
                    RouteName = "GetRoles",
                    Values = new { searchKeyword, pageSize, pageNumber = pageNumber + 1, orderBy },
                });
            }
            return new PaginationResult<RoleModel>
            {
                Items = paginedResult.Data,
                TotalItems = paginedResult.TotalCount,
                NextPage = nextPage,
            };
        }

        /// <summary>
        /// Get role details
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <returns>HTTP Get response</returns>
        [HttpGet("{roleName}", Name = "GetRole")]
        //[Authorize(Policy = Policies....)
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDetailsModel))]
        public async Task<IActionResult> GetRoleDetails(string roleName)
        {
            var role = await roleManagementService.GetRoleDetailsAsync(roleName);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        /// <summary>
        /// Create a new role and the associated actions
        /// </summary>
        /// <param name="roleDetails">Role details</param>
        /// <returns>HTTP Post response</returns>
        [HttpPost(Name = "POST Create a Role")]
        //[Authorize(Policy = Policies.CreateRole)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RoleDetailsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRoleAsync(RoleDetailsModel role)
        {
            return Ok(await this.roleManagementService.CreateRole(role));
        }

        /// <summary>
        /// Edit an existing role and the associated actions, delete the actions that are not in the new list
        /// </summary>
        /// <param name="roleDetails">Role details</param>
        /// <param name="roleName">Current role name (before any changes)</param>
        /// <returns>HTTP Put response, updated role</returns>
        [HttpPut("{roleName}", Name = "PUT Edit Role")]
        //[Authorize(Policy = Policies.EditRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditRoleAsync(string roleName, RoleDetailsModel roleDetails)
        {
            return Ok(await this.roleManagementService.UpdateRole(roleName, roleDetails));
        }


        /// <summary>
        /// Delete a role by name
        /// </summary>
        /// <param name="id">Role id that we want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            return Ok(await roleManagementService.DeleteRole(id));
        }
    }
}
