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
    [Route("api/accessControls")]
    [ApiExplorerSettings(GroupName = "AccessControl Management")]
    public class AccessControlsController : ControllerBase
    {
        private readonly IAccessControlManagementService accessControlManagementService;

        public AccessControlsController(IAccessControlManagementService accessControlManagementService)
        {
            this.accessControlManagementService = accessControlManagementService;
        }

        [HttpGet]
        //[Authorize(Policy = "GetAllAccessControls")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AccessControlDto>))]
        public async Task<IActionResult> GetAllAccessControls()
        {
            var accessControls = await accessControlManagementService.GetAllAccessControlsAsync();
            return Ok(accessControls);
        }
        [HttpGet("{accessControlId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessControlDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccessControlDetails(string accessControlId)
        {
            var accessControl = await accessControlManagementService.GetAccessControlByIdAsync(accessControlId);
            if (accessControl == null)
            {
                return NotFound();
            }
            return Ok(accessControl);
        }

        // TODO : Other methods 
    }
}
