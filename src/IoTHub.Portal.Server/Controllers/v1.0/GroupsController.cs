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
    [Route("api/groups")]
    [ApiExplorerSettings(GroupName = "Group Management")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupManagementService groupManagementService;
        private readonly ILogger<GroupsController> logger;

        public GroupsController(IGroupManagementService groupManagementService, ILogger<GroupsController> logger)
        {
            this.groupManagementService = groupManagementService;
            this.logger = logger;

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GroupModel>))]
        public async Task<PaginationResult<GroupModel>> Get(
            string searchKeyword = null,
            int pageSize = 10,
            int pageNumber = 0,
            [FromQuery] string[] orderBy = null
            )
        {
            var paginedResult = await groupManagementService.GetGroupPage(
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
                    RouteName = "Get Users",
                    Values = new { searchKeyword, pageSize, pageNumber = pageNumber + 1, orderBy }
                });
            }
            return new PaginationResult<GroupModel>
            {
                Items = paginedResult.Data,
                TotalItems = paginedResult.TotalCount,
                NextPage = nextPage
            };

        }

        [HttpGet("{groupId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GroupDetailsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupDetails(string groupId)
        {
            var groupDetails = await groupManagementService.GetGroupDetailsAsync(groupId);
            if (groupDetails == null)
            {
                return NotFound();
            }
            return Ok(groupDetails);
        }

        [HttpPost(Name = "POST Create a Group")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(GroupDetailsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDetailsModel groupCreateModel)
        {
            return Ok(await this.groupManagementService.CreateGroupAsync(groupCreateModel));
        }

        /// <summary>
        /// Delete a group by id
        /// </summary>
        /// <param name="id">Group id that we want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            return Ok(await groupManagementService.DeleteGroup(id));
        }
    }
}
