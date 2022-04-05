// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers.v10
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Helpers;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices.Shared;

    [ApiController]
    [Route("api/device/configurations")]
    public class DeviceConfigurationsController : ControllerBase
    {
        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly IConfigService configService;

        /// <summary>
        /// The device provisioning service manager.
        /// </summary>
        private readonly IDeviceProvisioningServiceManager deviceProvisioningServiceManager;

        public DeviceConfigurationsController(IConfigService configService, IDeviceProvisioningServiceManager deviceProvisioningServiceManager)
        {
            this.configService = configService;
            this.deviceProvisioningServiceManager = deviceProvisioningServiceManager;
        }

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

        [HttpPost]
        public async Task CreateConfig(DeviceConfig deviceConfig)
        {
            var desiredProperties = new Dictionary<string, object>();

            foreach (var item in deviceConfig.Properties)
            {
                desiredProperties.Add($"properties.desired.{item.Key}", item.Value);
            }

            var deviceModelTwin = new TwinCollection();
            _ = this.deviceProvisioningServiceManager.CreateEnrollmentGroupFromModelAsync(deviceConfig.model.ModelId, deviceConfig.model.Name, deviceModelTwin);
            await this.configService.RolloutDeviceConfiguration(deviceConfig.model.ModelId, desiredProperties, deviceConfig.Tags);
        }
    }
}
