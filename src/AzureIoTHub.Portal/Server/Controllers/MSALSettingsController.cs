// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using AzureIoTHub.Portal.Server.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MSALSettingsController : ControllerBase
    {
        private readonly ClientApiIndentityOptions configuration;

        public MSALSettingsController(IOptions<ClientApiIndentityOptions> configuration)
        {
            this.configuration = configuration.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok(this.configuration);
        }
    }
}
