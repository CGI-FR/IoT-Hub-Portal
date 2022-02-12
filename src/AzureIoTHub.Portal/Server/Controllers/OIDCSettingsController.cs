// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using AzureIoTHub.Portal.Server.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class OIDCSettingsController : ControllerBase
    {
        private readonly ClientApiIndentityOptions configuration;

        public OIDCSettingsController(IOptions<ClientApiIndentityOptions> configuration)
        {
            this.configuration = configuration.Value;
        }

        /// <summary>
        /// Get the Open ID Settings.
        /// </summary>
        /// <returns>The portal OIDC settnigs.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /OIDCSetings
        ///
        /// </remarks>
        ///
        /// <response code="200">Returns the OIDC settings.</response>
        [HttpGet(Name = nameof(GetOIDCSettings))]
        public IActionResult GetOIDCSettings()
        {
            return this.Ok(this.configuration);
        }
    }
}
