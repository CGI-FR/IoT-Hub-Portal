// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.V10
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Shared.Extensions;
    using Shared.Helpers;
    using Shared.Security;

    [Authorize] // page requires auth in general, but we allow anonymous for listing static permissions
    [ApiVersion("1.0")]
    [Route("api/permissions")]
    [ApiExplorerSettings(GroupName = "Role Management")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly ILogger<PermissionsController> logger;
        private readonly IUserManagementService userManagementService;
        private readonly IAccessControlManagementService accessControlService;

        public PermissionsController(
            ILogger<PermissionsController> logger,
            IUserManagementService userManagementService,
            IAccessControlManagementService accessControlService)
        {
            this.logger = logger;
            this.userManagementService = userManagementService;
            this.accessControlService = accessControlService;
        }

        /// <summary>
        /// Returns the list of all available permissions (policies) that can be assigned to roles.
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // permissions are static; allow pre-auth calls to avoid 401 causing empty list
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortalPermissions[]))]
        public ActionResult<PortalPermissions[]> Get()
        {
            logger.LogDebug("Returning {Count} portal permissions", PortalPermissionsHelper.GetAllPermissions().Length);
            return Ok(PortalPermissionsHelper.GetAllPermissions());
        }

        /// <summary>
        /// Returns the list of permissions that the current authenticated user has.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortalPermissions[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PortalPermissions[]>> GetMyPermissions()
        {
            var userPermissions = new List<PortalPermissions>();
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(emailClaim))
            {
                this.logger.LogWarning("User authenticated but email claim is missing");
                return Unauthorized();
            }

            this.logger.LogDebug("Getting permissions for user {Email}", emailClaim);

            var user = await userManagementService.GetOrCreateUserByEmailAsync(emailClaim, User);

            foreach (var permission in PortalPermissionsHelper.GetAllPermissions())
            {
                var hasPermission = await accessControlService.UserHasPermissionAsync(user.PrincipalId, permission.AsString());
                if (hasPermission)
                {
                    userPermissions.Add(permission);
                }
            }

            this.logger.LogInformation("User {Email} has {Count} permissions", emailClaim, userPermissions.Count);

            return Ok(userPermissions.ToArray());
        }
    }
}
