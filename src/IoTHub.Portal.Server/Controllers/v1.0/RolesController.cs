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

        [HttpGet("{roleName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDetailsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleDetails(string roleName)
        {
            var roleDetails = await roleManagementService.GetRoleDetailsAsync(roleName);
            if (roleDetails == null)
            {
                return NotFound();
            }
            return Ok(roleDetails);
        }

        [HttpPost(Name = "POST Create a Role")]
        //[Authorize(Policy = Policies.CreateRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRoleAsync(RoleDetailsModel role)
        {
            return Ok(await this.roleManagementService.CreateRole(role));
        }

        [HttpPut("{roleName}")]
        //[Authorize(Policy = Policies.EditRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditRoleAsync(string roleName, RoleDetailsModel roleDetails)
        {
            return Ok(await this.roleManagementService.UpdateRole(roleName, roleDetails));
        }

        [HttpDelete("{roleName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            return Ok(await roleManagementService.DeleteRole(roleName));
        }

        // TODO : Other methods 
    }
}
