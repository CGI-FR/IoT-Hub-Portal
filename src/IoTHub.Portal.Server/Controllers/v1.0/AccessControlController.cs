// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v10
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Shared.Models.v10;
    //using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Mvc.Routing;
    using System;

    //[Authorize]
    [ApiVersion("1.0")]
    [Route("api/access-controls")]
    [ApiExplorerSettings(GroupName = "Access Control Management")]
    [ApiController]
    public class AccessControlController : ControllerBase
    {
        private readonly ILogger<AccessControlController> logger;
        private readonly IAccessControlManagementService service;

        public AccessControlController(ILogger<AccessControlController> logger, IAccessControlManagementService service)
        {
            this.logger = logger;
            this.service = service;
        }

        [HttpGet(Name = "Get Pagined Access Control")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResult<AccessControlModel>))]
        public async Task<PaginationResult<AccessControlModel>> Get(
            string searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null,
            [FromQuery] string? principalId = null
            )
        {
            try
            {
                var paginedResult = await service.GetAccessControlPage(
                searchKeyword,
                pageSize,
                pageNumber,
                orderBy,
                principalId
                );
                logger.LogInformation("AccessControls fetched successfully. Total AccessControl fetched : {Count}", paginedResult.TotalCount);
                var nextPage = string.Empty;
                if (paginedResult.HasNextPage)
                {
                    nextPage = Url.RouteUrl(new UrlRouteContext
                    {
                        RouteName = "Get Pagined Access Control",
                        Values = new { searchKeyword, pageSize, pageNumber = pageNumber + 1, orderBy }
                    });
                }
                return new PaginationResult<AccessControlModel>
                {
                    Items = paginedResult.Data,
                    TotalItems = paginedResult.TotalCount,
                    NextPage = nextPage
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching accessControls.");
                throw;
            }
        }

        [HttpGet("{id}", Name = "Get AccessControl By Id")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessControlModel))]
        public async Task<IActionResult> GetACById(string id)
        {
            try
            {
                var ac = await service.GetAccessControlAsync(id);
                if (ac is null)
                {
                    logger.LogWarning("AccessControl with ID {AcId} not found", id);
                    return NotFound();
                }
                logger.LogInformation("Details retrieved for accessControl with ID {AcId}", id);
                return Ok(ac);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get details for accessControl with ID {AcId}", id);
                throw;
            }

        }

        /// <summary>
        /// Create a new role and the associated actions
        /// </summary>
        /// <param name="accessControl">AccessControl model that we want to create in db</param>
        /// <returns>HTTP Post response</returns>
        [HttpPost(Name = "POST Create a AccessControl")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccessControlAsync(AccessControlModel accessControl)
        {
            try
            {
                var result = await this.service.CreateAccessControl(accessControl);
                logger.LogInformation("AccessControl created successfully with ID {AcId}", result.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to create accessControl. : {ex}");
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Edit an existing role and the associated actions, delete the actions that are not in the new list
        /// </summary>
        /// <param name="accessControl">AccessControl model that we want to update in db</param>
        /// <param name="Id">Current role name (before any changes)</param>
        /// <returns>HTTP Put response, updated role</returns>
        [HttpPut("{id}", Name = "PUT Edit AccessControl")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditAccessControlAsync(string id, AccessControlModel accessControl)
        {
            try
            {
                var result = await this.service.UpdateAccessControl(id, accessControl);
                logger.LogInformation("AccessControl with ID {AcId} updated successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update AccessControl with ID {AcId}", id);
                throw;
            }
        }


        /// <summary>
        /// Delete a role by name
        /// </summary>
        /// <param name="id">AccessControl id that we want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        //[AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccessControl(string id)
        {
            try
            {
                var result = await service.DeleteAccessControl(id);
                if (!result)
                {
                    logger.LogWarning("AccessControl with ID {AcId} not found", id);
                    return NotFound("AccessControl not found.");
                }
                logger.LogInformation("AccessControl with ID {AcId} deleted successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete AccessControl with ID {AcId}", id);
                throw;
            }
        }
    }
}
