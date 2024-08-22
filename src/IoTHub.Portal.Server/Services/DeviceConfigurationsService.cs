// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using Shared.Models;
    using Shared.Models.v1._0;

    public class DeviceConfigurationsService : IDeviceConfigurationsService
    {
        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly IConfigService configService;

        private readonly IDeviceModelPropertiesService deviceModelPropertiesService;

        public DeviceConfigurationsService(IConfigService configService, IDeviceModelPropertiesService deviceModelPropertiesService)
        {
            this.configService = configService;
            this.deviceModelPropertiesService = deviceModelPropertiesService;
        }

        public async Task<IEnumerable<ConfigListItem>> GetDeviceConfigurationListAsync()
        {
            var configList = await this.configService.GetDevicesConfigurations();

            return configList.Select(ConfigHelper.CreateConfigListItem);
        }

        public async Task<DeviceConfig> GetDeviceConfigurationAsync(string configurationId)
        {
            try
            {
                var configItem = await this.configService.GetConfigItem(configurationId);

                return ConfigHelper.CreateDeviceConfig(configItem);
            }
            catch (InvalidOperationException e)
            {
                throw new InternalServerErrorException("Something went wrong when getting device configuration.", e);
            }
        }

        public async Task<ConfigurationMetrics> GetConfigurationMetricsAsync(string configurationId)
        {
            var configItem = await this.configService.GetConfigItem(configurationId);

            return new ConfigurationMetrics
            {
                MetricsTargeted = ConfigHelper.RetrieveMetricValue(configItem, "targetedCount"),
                MetricsApplied = ConfigHelper.RetrieveMetricValue(configItem, "appliedCount"),
                MetricsSuccess = ConfigHelper.RetrieveMetricValue(configItem, "reportedSuccessfulCount"),
                MetricsFailure = ConfigHelper.RetrieveMetricValue(configItem, "reportedFailedCount"),
                CreationDate = configItem.CreatedTimeUtc
            };
        }

        public async Task CreateConfigurationAsync(DeviceConfig deviceConfig)
        {
            await CreateOrUpdateConfiguration(deviceConfig);
        }

        public async Task UpdateConfigurationAsync(DeviceConfig deviceConfig)
        {
            await CreateOrUpdateConfiguration(deviceConfig);
        }

        public async Task DeleteConfigurationAsync(string configurationId)
        {
            await this.configService.DeleteConfiguration(configurationId);
        }

        private async Task CreateOrUpdateConfiguration(DeviceConfig deviceConfig)
        {
            IEnumerable<DeviceModelProperty> items;
            try
            {
                items = await this.deviceModelPropertiesService.GetModelProperties(deviceConfig.ModelId);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to retrieve device model properties", e);
            }

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

            _ = await this.configService.RollOutDeviceConfiguration(deviceConfig.ModelId, desiredProperties, deviceConfig.ConfigurationId, deviceConfig.Tags, 100);
        }
    }
}
