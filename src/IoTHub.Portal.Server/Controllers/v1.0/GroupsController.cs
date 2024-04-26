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
    [Route("api/groups")]
    [ApiExplorerSettings(GroupName = "Group Management")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupManagementService groupManagementService;
        private readonly IAccessControlManagementService accessControlService;
        private readonly ILogger<GroupsController> logger;

        public GroupsController(IGroupManagementService groupManagementService, ILogger<GroupsController> logger, IAccessControlManagementService accessControlService)
        {
            this.groupManagementService = groupManagementService;
            this.logger = logger;
            this.accessControlService = accessControlService;
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
            try
            {
                var paginedResult = await groupManagementService.GetGroupPage(
                searchKeyword,
                pageSize,
                pageNumber,
                orderBy
                );
                logger.LogInformation("Groups fetched successfully. Total groups fetched: {Count}", paginedResult.TotalCount);
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching groups.");
                throw;
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GroupDetailsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupDetails(string id)
        {
            try
            {
                var groupDetails = await groupManagementService.GetGroupDetailsAsync(id);
                if (groupDetails == null)
                {
                    logger.LogWarning("Group with ID {GroupId} not found", id);
                    return NotFound();
                }
                var accessControls = await accessControlService.GetAccessControlPage(null,100, 0,null, groupDetails.PrincipalId);
                if (accessControls.Data is not null)
                {
                    foreach (var ac in accessControls.Data)
                    {
                        groupDetails.AccessControls.Add(ac);
                    }
                }
                logger.LogInformation("Details retrieved for group with ID {GroupId}", id);
                return Ok(groupDetails);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get details for group with ID {GroupId}", id);
                throw;
            }
        }

        [HttpPost(Name = "POST Create a Group")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(GroupDetailsModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDetailsModel group)
        {
            try
            {
                var result = await this.groupManagementService.CreateGroupAsync(group);
                logger.LogInformation("Group created successfully with ID {GroupId}", result.Id);
                return CreatedAtAction(nameof(GetGroupDetails), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create group.");
                throw;
            }
        }


        [HttpPut("{id}", Name = "PUT Edit a Group")]
        //[Authorize(Policy = Policies.EditGroup)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EditGroupAsync(string id, GroupDetailsModel group)
        {
            try
            {
                var result = await this.groupManagementService.UpdateGroup(id, group);
                logger.LogInformation("Group with ID {GroupId} updated successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update group with ID {GroupId}", id);
                throw;
            }
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
            try
            {
                var result =await groupManagementService.DeleteGroup(id);
                if (!result)
                {
                    logger.LogWarning("Group with ID {GroupId} not found", id);
                    return NotFound("Group not found.");
                }
                logger.LogInformation("Group with ID {GroupId} deleted successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete group with ID {GroupId}", id);
                throw;
            }
        }

        // Ajouter un utilisateur à un groupe
        [HttpPost("{groupId}/members/{userId}")]
        public async Task<IActionResult> AddUserToGroup(string groupId, string userId)
        {
            var result = await groupManagementService.AddUserToGroup(groupId, userId);
            if (result)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpDelete("{groupId}/members/{userId}")]
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
