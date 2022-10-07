// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class LoRaWANConcentratorService : ILoRaWANConcentratorService
    {
        /// <summary>
        /// The device Idevice service.
        /// </summary>
        private readonly IExternalDeviceService externalDevicesService;

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
            IExternalDeviceService externalDevicesService,
            IConcentratorTwinMapper concentratorTwinMapper,
            IRouterConfigManager routerConfigManager)
        {
            this.logger = logger;
            this.externalDevicesService = externalDevicesService;
            this.concentratorTwinMapper = concentratorTwinMapper;
            this.routerConfigManager = routerConfigManager;
        }

        public async Task<bool> CreateDeviceAsync(ConcentratorDto device)
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

                var result = await this.externalDevicesService.CreateDeviceWithTwin(device.DeviceId, false, newTwin, status);

                if (!result.IsSuccessful)
                {
                    this.logger?.LogWarning(message: $"Failed to create concentrator. {string.Join(Environment.NewLine, result.Errors?.Select(c => JsonConvert.SerializeObject(c)) ?? Array.Empty<string>())}");

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

        public PaginationResult<ConcentratorDto> GetAllDeviceConcentrator(PaginationResult<Twin> twinResults, IUrlHelper urlHelper)
        {
            string nextPage = null;

            if (!string.IsNullOrEmpty(twinResults.NextPage))
            {
                nextPage = urlHelper.RouteUrl(new UrlRouteContext
                {
                    Values = new
                    {
                        continuationToken = twinResults.NextPage
                    }
                });
            }

            return new PaginationResult<ConcentratorDto>
            {
                Items = twinResults.Items.Select(this.concentratorTwinMapper.CreateDeviceDetails),
                TotalItems = twinResults.TotalItems,
                NextPage = nextPage
            };
        }

        public async Task<bool> UpdateDeviceAsync(ConcentratorDto device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            // Device status (enabled/disabled) has to be dealt with afterwards
            var currentDevice = await this.externalDevicesService.GetDevice(device.DeviceId);
            currentDevice.Status = device.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.externalDevicesService.UpdateDevice(currentDevice);

            // Get the current twin from the hub, based on the device ID
            var currentTwin = await this.externalDevicesService.GetDeviceTwin(device.DeviceId);
            device.RouterConfig = await this.routerConfigManager.GetRouterConfig(device.LoraRegion);

            // Update the twin properties
            this.concentratorTwinMapper.UpdateTwin(currentTwin, device);

            _ = await this.externalDevicesService.UpdateDeviceTwin(currentTwin);

            return true;
        }
    }
}
