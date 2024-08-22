// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.v1._0
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models.v1._0;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/device-configurations")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class DeviceConfigurationsController : ControllerBase
    {
        private readonly IDeviceConfigurationsService deviceConfigurationsService;

        public DeviceConfigurationsController(IDeviceConfigurationsService deviceConfigurationsService)
        {
            this.deviceConfigurationsService = deviceConfigurationsService;
        }

        [HttpGet(Name = "GET Device configurations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<ConfigListItem>> Get()
        {
            return await this.deviceConfigurationsService.GetDeviceConfigurationListAsync();
        }

        [HttpGet("{configurationId}", Name = "GET Device configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DeviceConfig>> Get(string configurationId)
        {
            return Ok(await this.deviceConfigurationsService.GetDeviceConfigurationAsync(configurationId));
        }

        [HttpGet("{configurationId}/metrics", Name = "GET Device configuration metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ConfigurationMetrics>> GetConfigurationMetrics(string configurationId)
        {
            return Ok(await this.deviceConfigurationsService.GetConfigurationMetricsAsync(configurationId));
        }

        [HttpPost(Name = "POST Create Device configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task CreateConfig(DeviceConfig deviceConfig)
        {
            await this.deviceConfigurationsService.CreateConfigurationAsync(deviceConfig);
        }

        [HttpPut(Name = "PUT Update Device configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task UpdateConfig(DeviceConfig deviceConfig)
        {
            await this.deviceConfigurationsService.UpdateConfigurationAsync(deviceConfig);
        }

        [HttpDelete("{configurationId}", Name = "DELETE Device configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task DeleteConfig(string configurationId)
        {
            await this.deviceConfigurationsService.DeleteConfigurationAsync(configurationId);
        }
    }
}
