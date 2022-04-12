// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Services;
    using Entities;
    using Factories;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Shared.Models.v10;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/device-configurations")]
    [ApiExplorerSettings(GroupName = "IoT Devices")]
    public class DeviceConfigurationsController : ControllerBase
    {
        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly IConfigService configService;

        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The mapper.
        /// </summary>
        private readonly IMapper mapper;

        public DeviceConfigurationsController(IConfigService configService,
            IMapper mapper,
            ITableClientFactory tableClientFactory)
        {
            this.configService = configService;
            this.mapper = mapper;
            this.tableClientFactory = tableClientFactory;
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
            try
            {
                var configItem = await this.configService.GetConfigItem(configurationId);

                var deviceConfig = ConfigHelper.CreateDeviceConfig(configItem);

                return this.Ok(deviceConfig);
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [HttpGet("{configurationId}/metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ConfigurationMetrics>> GetConfigurationMetrics(string configurationId)
        {
            var configItem = await this.configService.GetConfigItem(configurationId);

            return this.Ok(new ConfigurationMetrics
            {
                MetricsTargeted = ConfigHelper.RetrieveMetricValue(configItem, "targetedCount"),
                MetricsApplied = ConfigHelper.RetrieveMetricValue(configItem, "appliedCount"),
                MetricsSuccess = ConfigHelper.RetrieveMetricValue(configItem, "reportedSuccessfulCount"),
                MetricsFailure = ConfigHelper.RetrieveMetricValue(configItem, "reportedFailedCount"),
                CreationDate = configItem.CreatedTimeUtc
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task CreateConfig(DeviceConfig deviceConfig)
        {
            await CreateOrUpdateConfiguration(deviceConfig);
        }

        [HttpPut("{configurationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task UpdateConfig(DeviceConfig deviceConfig)
        {
            await CreateOrUpdateConfiguration(deviceConfig);
        }

        [HttpDelete("{configurationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task DeleteConfig(string configurationId)
        {
            await this.configService.DeleteConfiguration(configurationId);
        }

        private async Task CreateOrUpdateConfiguration(DeviceConfig deviceConfig)
        {
            var table = this.tableClientFactory
                .GetDeviceTemplateProperties();

            var items = table
                .Query<DeviceModelProperty>($"PartitionKey eq '{deviceConfig.ModelId}'")
                .ToArray();

            var desiredProperties = new Dictionary<string, object>();

            foreach (var item in deviceConfig.Properties)
            {
                var modelProperty = items.SingleOrDefault(c => c.Name == item.Key);

                if (modelProperty == null)
                    continue;

                object propertyValue = modelProperty.PropertyType switch
                {
                    DevicePropertyType.Boolean => bool.TryParse(item.Value, out var boolResult) ? boolResult : null,
                    DevicePropertyType.Double => double.TryParse(item.Value, out var doubleResult) ? doubleResult : null,
                    DevicePropertyType.Float => float.TryParse(item.Value, out var floatResult) ? floatResult : null,
                    DevicePropertyType.Integer => int.TryParse(item.Value, out var intResult) ? intResult : null,
                    DevicePropertyType.Long => long.TryParse(item.Value, out var logResult) ? logResult : null,
                    DevicePropertyType.String => item.Value,
                    _ => null,
                };

                desiredProperties.Add($"properties.desired.{item.Key}", propertyValue);
            }

            await this.configService.RollOutDeviceConfiguration(deviceConfig.ModelId, desiredProperties, deviceConfig.ConfigurationId, deviceConfig.Tags, 100);
        }
    }
}
