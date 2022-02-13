// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using AzureIoTHub.Portal.Server.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [ApiController]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("/api/settings")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "Portal Settings")]
    public class SettingsController : ControllerBase
    {
        private readonly ClientApiIndentityOptions configuration;

        public SettingsController(IOptions<ClientApiIndentityOptions> configuration)
        {
            this.configuration = configuration.Value;
        }

        /// <summary>
        /// Get the Open ID Settings.
        /// </summary>
        /// <returns>The portal OIDC settnigs.</returns>
        /// <response code="200">Returns the OIDC settings.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("oidc", Name = "GET OIDC")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetOIDCSettings()
        {
            return this.Ok(this.configuration);
        }
    }
}
