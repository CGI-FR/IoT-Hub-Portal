// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using Microsoft.Azure.Devices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

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

        public async Task<ConcentratorDto> GetConcentrator(string deviceId)
        {
            var concentratorEntity = await this.concentratorRepository.GetByIdAsync(deviceId);

            if (concentratorEntity == null)
            {
                throw new ResourceNotFoundException($"The concentrator with id {deviceId} doesn't exist");
            }

            var concentratorDto = this.mapper.Map<ConcentratorDto>(concentratorEntity);

            return concentratorDto;
        }

        protected async Task<ConcentratorDto> CreateDeviceInDatabase(ConcentratorDto concentrator)
        {
            try
            {
                var concentratorEntity = this.mapper.Map<Concentrator>(concentrator);
                await this.concentratorRepository.InsertAsync(concentratorEntity);
                await this.unitOfWork.SaveAsync();
                return concentrator;
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to create the concentrator {concentrator.DeviceName}", e);
            }
        }

        public async Task<ConcentratorDto> CreateDeviceAsync(ConcentratorDto concentrator)
        {
            var newTwin = await this.externalDevicesService.CreateNewTwinFromDeviceId(concentrator.DeviceId);
            concentrator.RouterConfig = await this.routerConfigManager.GetRouterConfig(concentrator.LoraRegion);
            concentrator.ClientThumbprint ??= string.Empty;

            this.concentratorTwinMapper.UpdateTwin(newTwin, concentrator);
            var status = concentrator.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.externalDevicesService.CreateDeviceWithTwin(concentrator.DeviceId, false, newTwin, status);

            return await CreateDeviceInDatabase(concentrator);
        }

        protected async Task<ConcentratorDto> UpdateDeviceInDatabase(ConcentratorDto concentrator)
        {
            try
            {
                var concentratorEntity = await this.concentratorRepository.GetByIdAsync(concentrator.DeviceId);

                if (concentratorEntity == null)
                {
                    throw new ResourceNotFoundException($"The device {concentrator.DeviceId} doesn't exist");
                }

                _ = this.mapper.Map(concentrator, concentratorEntity);

                this.concentratorRepository.Update(concentratorEntity);
                await this.unitOfWork.SaveAsync();

                return concentrator;
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to update the concentrator {concentrator.DeviceName}", e);
            }
        }

        public async Task<ConcentratorDto> UpdateDeviceAsync(ConcentratorDto concentrator)
        {
            // Device status (enabled/disabled) has to be dealt with afterwards
            var currentConcentrator = await this.externalDevicesService.GetDevice(concentrator.DeviceId);
            currentConcentrator.Status = concentrator.IsEnabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;

            _ = await this.externalDevicesService.UpdateDevice(currentConcentrator);

            // Get the current twin from the hub, based on the device ID
            var currentTwin = await this.externalDevicesService.GetDeviceTwin(concentrator.DeviceId);
            concentrator.RouterConfig = await this.routerConfigManager.GetRouterConfig(concentrator.LoraRegion);

            // Update the twin properties
            this.concentratorTwinMapper.UpdateTwin(currentTwin, concentrator);

            _ = await this.externalDevicesService.UpdateDeviceTwin(currentTwin);

            return await UpdateDeviceInDatabase(concentrator);
        }
    }
}
