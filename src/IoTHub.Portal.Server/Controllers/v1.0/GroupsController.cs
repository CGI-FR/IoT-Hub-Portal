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
    [Route("api/groups")]
    [ApiExplorerSettings(GroupName = "Group Management")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupManagementService groupManagementService;

        public GroupsController(IGroupManagementService groupManagementService)
        {
            this.groupManagementService = groupManagementService;
        }

        [HttpGet]
        //[Authorize(Policy = "GetAllGroups")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GroupModel>))]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await groupManagementService.GetAllGroupsAsync();
            return Ok(groups);
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
        // TODO : Other methods 
    }
}
