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
    using IoTHub.Portal.Shared.Models.v1._0;

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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleDto>))]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await roleManagementService.GetAllRolesAsync();
            return Ok(roles);
        }

        // TODO : Other methods 
    }
}
