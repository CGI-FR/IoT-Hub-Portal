// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Azure.Devices;
    using System;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Microsoft.Extensions.Logging;

    public class LoRaWANConcentratorService : ILoRaWANConcentratorService
    {
        /// <summary>
        /// The device Idevice service.
        /// </summary>
        private readonly IDeviceService devicesService;

        /// <summary>
        /// The device IConcentrator twin mapper.
        /// </summary>
        private readonly IConcentratorTwinMapper concentratorTwinMapper;

        /// <summary>
        /// The device IRouter config manager.
        /// </summary>
        private readonly IRouterConfigManager routerConfigManager;

        /// <summary>
        /// The device Lora wan concentrators service logger.
        /// </summary>
        private readonly ILogger<LoRaWANConcentratorService> logger;

        public LoRaWANConcentratorService(
            ILogger<LoRaWANConcentratorService> logger,
            IDeviceService devicesService,
            IConcentratorTwinMapper concentratorTwinMapper,
            IRouterConfigManager routerConfigManager)
        {
            this.logger = logger;
            this.devicesService = devicesService;
            this.concentratorTwinMapper = concentratorTwinMapper;
            this.routerConfigManager = routerConfigManager;
        }

        public async Task<bool> CreateDeviceAsync(Concentrator device)
        {
            try
            {
                // Create a new Twin from the form's fields.
                var newTwin = new Twin()
                {
                    DeviceId = device.DeviceId,
                };

                device.RouterConfig = await this.routerConfigManager.GetRouterConfig(device.LoraRegion);

                device.ClientThumbprint ??= string.Empty;

                this.concentratorTwinMapper.UpdateTwin(newTwin, device);

                var status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

                var result = await this.devicesService.CreateDeviceWithTwin(device.DeviceId, false, newTwin, status);

                if (!result.IsSuccessful)
                {
                    this.logger?.LogWarning(message: $"Failed to create concentrator. {result.Errors}");

                    return false;
                }
            }
            catch (DeviceAlreadyExistsException e)
            {
                this.logger?.LogError($"{device.DeviceId} - Create device failed", e);
                throw;
            }
            catch (InvalidOperationException e)
            {
                this.logger?.LogError($"Create device failed for {device.DeviceId} \n {e.Message}");
                return false;
            }

            return true;
        }
    }
}
