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
    using System;

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
        /// <param name="pageSize">Number of role per page</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="orderBy">Critera order</param>
        [HttpGet(Name = "GetRoles")]
        //[Authorize(Policy = "GetRoles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResult<RoleModel>))]
        public async Task<PaginationResult<RoleModel>> Get(
            [FromQuery] string searchKeyword = null,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 0,
            [FromQuery] string[] orderBy = null
        )
        {
            try
            {
                var paginedResult = await roleManagementService.GetRolePage(
                searchKeyword,
                pageSize,
                pageNumber,
                orderBy
                );
                logger.LogInformation("Roles fetched successfully. Total Role fetched : {Count}", paginedResult.TotalCount);
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
            catch (Exception ex)
            {
                logger.LogError("Error fetching roles.");
                throw ex; // Rethrowing the exception (preserves the stack trace)
            }
        }

        /// <summary>
        /// Get role details
        /// </summary>
        /// <param name="id">Role id</param>
        /// <returns>HTTP Get response</returns>
        [HttpGet("{id}", Name = "GetRole")]
        //[Authorize(Policy = Policies....)
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDetailsModel))]
        public async Task<IActionResult> GetRoleDetails(string id)
        {
            try
            {
                var role = await roleManagementService.GetRoleDetailsAsync(id);
                if (role == null)
                {
                    logger.LogWarning("Role with ID {RoleId} not found", id);
                    return NotFound();
                }
                logger.LogInformation("Details retrieved for role with ID {RoleId}", id);
                return Ok(role);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get details for role with ID {RoleId}", id);
                throw;
            }
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
            try
            {
                var result = await this.roleManagementService.CreateRole(role);
                logger.LogInformation("Role created successfully with ID {RoleId}", result.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to create role. : ", ex);
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Edit an existing role and the associated actions, delete the actions that are not in the new list
        /// </summary>
        /// <param name="roleDetails">Role details</param>
        /// <param name="id">Role id</param>
        /// <returns>HTTP Put response, updated role</returns>
        [HttpPut("{id}", Name = "PUT Edit Role")]
        //[Authorize(Policy = Policies.EditRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditRoleAsync(string id, RoleDetailsModel roleDetails)
        {
            try
            {
                var result = await this.roleManagementService.UpdateRole(id, roleDetails);
                logger.LogInformation("Role with ID {RoleId} updated successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update role with ID {RoleId}", id);
                throw;
            }
        }


        /// <summary>
        /// Delete a role by name
        /// </summary>
        /// <param name="id">Role id that we want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}", Name = "DELETE Role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                var result = await roleManagementService.DeleteRole(id);
                if (!result)
                {
                    logger.LogWarning("Role with ID {RoleId} not found", id);
                    return NotFound("Role not found.");
                }
                logger.LogInformation("Role with ID {RoleId} deleted successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete role with ID {RoleId}", id);
                throw;
            }
        }
    }
}
