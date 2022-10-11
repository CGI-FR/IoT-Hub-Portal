// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
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


        private readonly PortalDbContext portalDbContext;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IConcentratorRepository concentratorRepository;


        public LoRaWANConcentratorService(
            ILogger<LoRaWANConcentratorService> logger,
            IExternalDeviceService externalDevicesService,
            IConcentratorTwinMapper concentratorTwinMapper,
            IRouterConfigManager routerConfigManager,
            PortalDbContext portalDbContext,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IConcentratorRepository concentratorRepository
            )
        {
            this.logger = logger;
            this.externalDevicesService = externalDevicesService;
            this.concentratorTwinMapper = concentratorTwinMapper;
            this.routerConfigManager = routerConfigManager;
            this.portalDbContext = portalDbContext;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.concentratorRepository = concentratorRepository;
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

        public async Task<PaginatedResult<ConcentratorDto>> GetAllDeviceConcentrator(
            int pageSize = 10,
            int pageNumber = 0,
            string[] orderBy = null)
        {
            var paginatedConcentrator = await this.concentratorRepository.GetPaginatedListAsync(pageNumber, pageSize, orderBy);
            var paginatedConcentratorDto = new PaginatedResult<ConcentratorDto>()
            {
                Data = paginatedConcentrator.Data.Select(x => mapper.Map<ConcentratorDto>(x)).ToList(),
                TotalCount = paginatedConcentrator.TotalCount,
                CurrentPage = paginatedConcentrator.CurrentPage,
                PageSize = pageSize
            };
            return paginatedConcentratorDto;
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
