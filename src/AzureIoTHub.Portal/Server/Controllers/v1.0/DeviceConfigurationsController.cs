// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v10
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/device/configurations")]
    public class DeviceConfigurationsController : ControllerBase
    {
        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly IConfigService configService;

        public DeviceConfigurationsController(IConfigService configService)
        {
            this.configService = configService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<ConfigListItem>> Get()
        {
            var configList = await this.configService.GetDevicesConfigurations();
            var results = new List<ConfigListItem>();

            foreach (var item in configList)
            {
                var configItem = ConfigHelper.CreateConfigListItem(item);
                results.Add(configItem);
            }

            return results;
        }

        [HttpGet("{configurationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DeviceConfig>> Get(string configurationId)
        {
            var configItem = await this.configService.GetConfigItem(configurationId);

            var deviceConfig = ConfigHelper.CreateDeviceConfig(configItem);

            // Define a regular expression for repeated words.
            var rx = new Regex(@"tags[.](?<tagName>\w*)[ ]?[=][ ]?\'(?<tagValue>[\w-]*)\'", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (string.IsNullOrEmpty(configItem.TargetCondition))
            {
                return this.BadRequest("Target condition is null.");
            }

            var matches = rx.Matches(configItem.TargetCondition);

            if (matches.Count == 0)
            {
                return this.BadRequest("Target condition is not formed as expected.");
            }

            // Find matches.
            foreach (Match match in matches)
            {
                var groups = match.Groups;

                deviceConfig.Tags.Add(groups["tagName"].Value, groups["tagValue"].Value);
            }

            foreach (var item in configItem.Content.DeviceContent)
            {
                deviceConfig.Properties.Add(item.Key, item.Value);
            }

            if (deviceConfig.Tags.ContainsKey("modelId"))
            {
                deviceConfig.model = new DeviceModel
                {
                    ModelId = deviceConfig.Tags["modelId"]
                };
                _ = deviceConfig.Tags.Remove("modelId");
            }

            return this.Ok(deviceConfig);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task CreateConfig(DeviceConfig deviceConfig)
        {
            var desiredProperties = new Dictionary<string, object>();

            foreach (var item in deviceConfig.Properties)
            {
                desiredProperties.Add($"properties.desired.{item.Key}", JsonSerializer.Serialize(item.Value));
            }

            await this.configService.RolloutDeviceConfiguration(deviceConfig.model.ModelId, desiredProperties, deviceConfig.ConfigurationID, deviceConfig.Tags, 100);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task DeleteConfig(string deviceConfigId)
        {
            await this.configService.DeleteConfiguration(deviceConfigId);
        }
    }
}
