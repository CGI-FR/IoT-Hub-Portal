// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.V10
{
    using AzureIoTHub.Portal.Server.Services;
    using AzureIoTHub.Portal.Models.v10;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("/api/settings/device-tags")]
    [ApiExplorerSettings(GroupName = "Portal Settings")]
    public class DeviceTagSettingsController : ControllerBase
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<DeviceTagSettingsController> log;

        /// <summary>
        /// The DeviceTag service.
        /// </summary>
        private readonly IDeviceTagService deviceTagService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTagSettingsController"/> class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="deviceTagService">The device tag service.</param>
        public DeviceTagSettingsController(
            ILogger<DeviceTagSettingsController> log,
            IDeviceTagService deviceTagService)
        {
            this.log = log;
            this.deviceTagService = deviceTagService;
        }

        /// <summary>
        /// Updates the device tag settings to be used in the application.
        /// </summary>
        /// <param name="tags">List of tags.</param>
        /// <returns>The action result.</returns>
        [HttpPost(Name = "POST Update the Device tags settings")]
        public async Task<IActionResult> Post(IEnumerable<DeviceTag> tags)
        {
            ArgumentNullException.ThrowIfNull(tags, nameof(tags));

            await this.deviceTagService.UpdateTags(tags);
            return Ok();
        }

        /// <summary>
        /// Gets the device tag settings to be used in the application
        /// </summary>
        /// <returns>The list of tags</returns>
        [HttpGet(Name = "GET Device tags settings")]
        public ActionResult<List<DeviceTag>> Get()
        {
            return Ok(this.deviceTagService.GetAllTags());
        }
    }
}
