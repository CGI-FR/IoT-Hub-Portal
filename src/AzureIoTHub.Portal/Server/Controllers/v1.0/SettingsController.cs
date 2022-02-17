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
        /// <summary>
        /// The device client api indentity options.
        /// </summary>
        private readonly ClientApiIndentityOptions configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
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
        [HttpGet("oidc", Name = "GET Open ID settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetOIDCSettings()
        {
            return this.Ok(this.configuration);
        }
    }
}
