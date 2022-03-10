// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using System.Reflection;
    using AzureIoTHub.Portal.Server.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using static AzureIoTHub.Portal.Server.Startup;

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

        private readonly ConfigHandler configHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="configHandler">The configHandler.</param>
        public SettingsController(IOptions<ClientApiIndentityOptions> configuration, ConfigHandler configHandler)
        {
            this.configuration = configuration.Value;
            this.configHandler = configHandler;
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

        /// <summary>
        /// Get the a boolean for LoRa feature enable on the portal or not.
        /// </summary>
        /// <returns>The LoRa support setting.</returns>
        /// <response code="200">Returns the LoRa support setting.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("lora", Name = "GET LoRa settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetLoRaActivationSetting()
        {
            return this.Ok(this.configHandler.IsLoRaEnabled);
        }

        /// <summary>
        /// Get the portal version.
        /// </summary>
        /// <returns>The server version.</returns>
        /// <response code="200">The server version.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("version", Name = "GET Portal Version")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetVersion()
        {
            return this.Ok(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}
