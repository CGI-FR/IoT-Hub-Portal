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
    using AutoMapper;
    using IoTHub.Portal.Domain.Entities;
    using System.Linq;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/roles")]
    [ApiExplorerSettings(GroupName = "Role Management")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleManagementService roleManagementService;
        private readonly IMapper mapper;

        public RolesController(IRoleManagementService roleManagementService, IMapper mapper)
        {
            this.roleManagementService = roleManagementService;
            this.mapper = mapper;
        }

        [HttpGet]
        //[Authorize(Policy = "GetAllRoles")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleModel>))]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await roleManagementService.GetAllRolesAsync();
            var rolesModel = mapper.Map<IEnumerable<RoleModel>>(roles);
            return Ok(rolesModel);
        }

        [HttpGet("{roleName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleDetailsModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleDetails(string roleName)
        {
            var role = await roleManagementService.GetRoleDetailsAsync(roleName);
            if (role == null)
            {
                return NotFound();
            }
            var roleDetails = mapper.Map<RoleDetailsModel>(role);
            return Ok(roleDetails);
        }

        [HttpPost(Name = "POST Create a Role")]
        //[Authorize(Policy = Policies.CreateRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRoleAsync(RoleDetailsModel roleDetails)
        {
            var role = mapper.Map<Role>(roleDetails);
            return Ok(await this.roleManagementService.CreateRole(role));
        }

        [HttpPut("{currentRoleName}")]
        //[Authorize(Policy = Policies.EditRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditRoleAsync(string currentRoleName, RoleDetailsModel roleDetails)
        {
            var existingRole = await roleManagementService.GetRoleDetailsAsync(currentRoleName);
            if (existingRole == null)
            {
                return NotFound($"Role with name {currentRoleName} not found.");
            }
            existingRole.Name = roleDetails.Name;
            existingRole.Description = roleDetails.Description;
            existingRole.Actions = UpdateRoleActions(existingRole.Actions, roleDetails.Actions.Select(a => new Action { Name = a }).ToList());

            var updatedRole = await roleManagementService.UpdateRole(existingRole);
            if (updatedRole == null)
            {
                return BadRequest("Unable to update the role.");
            }

            return Ok(updatedRole);
        }

        private static ICollection<Action> UpdateRoleActions(ICollection<Action> existingActions, ICollection<Action> newActions)
        {
            var updatedActions = new HashSet<Action>(existingActions);
            foreach (var action in newActions)
            {
                if (!updatedActions.Any(a => a.Name == action.Name))
                {
                    _ = updatedActions.Add(action);
                }
            }
            _ = updatedActions.RemoveWhere(a => !newActions.Any(na => na.Name == a.Name));
            return updatedActions;
        }


        [HttpDelete("{roleName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            return Ok(await roleManagementService.DeleteRole(roleName));
        }
    }
}
