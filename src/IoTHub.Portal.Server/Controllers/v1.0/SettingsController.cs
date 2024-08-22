// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Server.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Shared.Models.v1._0;

    [ApiController]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("/api/settings")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "Portal Settings")]
    public class SettingsController : ControllerBase
    {
        /// <summary>
        /// The device client api identity options.
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
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(configHandler, nameof(configHandler));

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
            return Ok(this.configuration);
        }

        /// <summary>
        /// Get the portal settings.
        /// </summary>
        [HttpGet("portal", Name = "GET Portal settings")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortalSettings))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPortalSetting()
        {
            return Ok(new PortalSettings
            {
                IsLoRaSupported = this.configHandler.IsLoRaEnabled,
                PortalName = this.configHandler.PortalName ?? "Azure IoT Hub Portal",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                CopyrightYear = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                IsIdeasFeatureEnabled = this.configHandler.IdeasEnabled,
                CloudProvider = this.configHandler.CloudProvider
            });
        }
    }
}
