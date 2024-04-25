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
                    RouteName = "Get Groups",
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

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GroupDetailsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupDetails(string id)
        {
            var groupDetails = await groupManagementService.GetGroupDetailsAsync(id);
            if (groupDetails == null)
            {
                return NotFound();
            }
            return Ok(groupDetails);
        }

        [HttpPost(Name = "POST Create a Group")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(GroupDetailsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDetailsModel group)
        {
            return Ok(await this.groupManagementService.CreateGroupAsync(group));
        }


        [HttpPut("{id}", Name = "PUT Edit a Group")]
        //[Authorize(Policy = Policies.EditGroup)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditGroupAsync(string id, GroupDetailsModel group)
        {
            return Ok(await this.groupManagementService.UpdateGroup(id, group));
        }

        /// <summary>
        /// Delete a group by id
        /// </summary>
        /// <param name="id">Group id that we want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            return Ok(await groupManagementService.DeleteGroup(id));
        }

        // Ajouter un utilisateur à un groupe
        [HttpPost("{groupId}/users/{userId}")]
        public async Task<IActionResult> AddUserToGroup(string groupId, string userId)
        {
            var result = await groupManagementService.AddUserToGroup(groupId, userId);
            if (result)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpDelete("{groupId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromGroup(string groupId, string userId)
        {
            var result = await groupManagementService.RemoveUserFromGroup(groupId, userId);
            if (result)
                return Ok(result);
            else
                return NotFound();
        }
    }
}
