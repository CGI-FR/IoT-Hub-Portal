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
    [Route("api/roles")]
    [ApiExplorerSettings(GroupName = "Role Management")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleManagementService roleManagementService;

        public RolesController(IRoleManagementService roleManagementService)
        {
            this.roleManagementService = roleManagementService;
        }

        [HttpGet]
        //[Authorize(Policy = "GetAllRoles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleModel>))]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await roleManagementService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{roleId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDetailsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleDetails(string roleId)
        {
            var roleDetails = await roleManagementService.GetRoleDetailsAsync(roleId);
            if (roleDetails == null)
            {
                return NotFound();
            }
            return Ok(roleDetails);
        }
        // TODO : Other methods 
    }
}
