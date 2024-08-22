// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Shared.Models.v1._0;

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
        public async Task<IActionResult> Post(IEnumerable<DeviceTagDto> tags)
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
        public ActionResult<List<DeviceTagDto>> Get()
        {
            return Ok(this.deviceTagService.GetAllTags());
        }

        /// <summary>
        /// Create or update a device tag
        /// </summary>
        /// <param name="deviceTag">Device Tag</param>
        /// <returns>The action result</returns>
        [HttpPatch(Name = "Create or update a device tag")]
        public async Task<IActionResult> CreateOrUpdateDeviceTag([FromBody] DeviceTagDto deviceTag)
        {
            await this.deviceTagService.CreateOrUpdateDeviceTag(deviceTag);
            return Ok();
        }

        /// <summary>
        /// Delete a device tag by name
        /// </summary>
        /// <param name="deviceTagName">Device Tag Name</param>
        /// <returns>The action result</returns>
        [HttpDelete("{deviceTagName}", Name = "Delete a device tag by name")]
        public async Task<IActionResult> DeleteDeviceTagByName([FromRoute] string deviceTagName)
        {
            await this.deviceTagService.DeleteDeviceTagByName(deviceTagName);
            return Ok();
        }
    }
}
