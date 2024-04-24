// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Shared.Models.v10;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Mvc.Routing;

    [Authorize]
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResult<AccessControlModel>))]
        public async Task<PaginationResult<AccessControlModel>> Get(
            string searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null
            )
        {
            var paginedResult = await service.GetAccessControlPage(
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

        [HttpGet("{id}", Name = "Get AccessControl By Id")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessControlModel))]
        public async Task<IActionResult> GetACById(string Id)
        {
            var ac = await service.GetAccessControlAsync(Id);
            if (ac is null)
            {
                return NotFound();
            }
            return Ok(ac);
        }

        /// <summary>
        /// Create a new role and the associated actions
        /// </summary>
        /// <param name="accessControl">AccessControl model that we want to create in db</param>
        /// <returns>HTTP Post response</returns>
        [HttpPost(Name = "POST Create a AccessControl")]
        //[Authorize(Policy = Policies.CreateAccessControl)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccessControlAsync(AccessControlModel accessControl)
        {
            return Ok(await this.service.CreateAccessControl(accessControl));
        }

        /// <summary>
        /// Edit an existing role and the associated actions, delete the actions that are not in the new list
        /// </summary>
        /// <param name="accessControl">AccessControl model that we want to update in db</param>
        /// <param name="Id">Current role name (before any changes)</param>
        /// <returns>HTTP Put response, updated role</returns>
        [HttpPut(Name = "PUT Edit AccessControl")]
        //[Authorize(Policy = Policies.EditAccessControl)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditAccessControlAsync(AccessControlModel roleDetails)
        {
            return Ok(await this.service.UpdateAccessControl(roleDetails));
        }


        /// <summary>
        /// Delete a role by name
        /// </summary>
        /// <param name="id">AccessControl id that we want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccessControl(string id)
        {
            return Ok(await service.DeleteAccessControl(id));
        }

    }
}
