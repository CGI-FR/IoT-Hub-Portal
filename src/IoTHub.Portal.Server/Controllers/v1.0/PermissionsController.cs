// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.V10
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Server.Security;

    [Authorize] // page requires auth in general, but we allow anonymous for listing static permissions
    [ApiVersion("1.0")]
    [Route("api/permissions")]
    [ApiExplorerSettings(GroupName = "Role Management")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly ILogger<PermissionsController> logger;

        public PermissionsController(ILogger<PermissionsController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Returns the list of all available permissions (policies) that can be assigned to roles.
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // permissions are static; allow pre-auth calls to avoid 401 causing empty list
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string[]))]
        public ActionResult<string[]> Get()
        {
            logger.LogDebug("Returning {Count} portal permissions", PortalPermissions.All.Length);
            return Ok(PortalPermissions.All);
        }
    }
}
