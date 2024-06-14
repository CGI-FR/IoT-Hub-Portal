// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using System;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Helpers;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Providers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Infrastructure.Mappers;
    using IoTHub.Portal.Models.v10;
    using IoTHub.Portal.Shared.Models;
    using IoTHub.Portal.Shared.Models.v10;
    using IoTHub.Portal.Shared.Models.v10.Filters;
    using Domain;
    using Domain.Entities;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.EntityFrameworkCore;
    using IoTHub.Portal.Crosscutting;

    public class DeviceModelService<TListItem, TModel> : IDeviceModelService<TListItem, TModel>
        where TListItem : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly IDeviceModelCommandRepository deviceModelCommandRepository;
        private readonly ILabelRepository labelRepository;

        private readonly IDeviceRegistryProvider deviceRegistryProvider;
        private readonly IConfigService configService;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IDeviceModelMapper<TListItem, TModel> deviceModelMapper;
        private readonly IExternalDeviceService externalDeviceService;

        public DeviceModelService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceModelRepository deviceModelRepository,
            IDeviceModelCommandRepository deviceModelCommandRepository,
            ILabelRepository labelRepository,
            IDeviceRegistryProvider deviceRegistryProvider,
            IConfigService configService,
            IDeviceModelImageManager deviceModelImageManager,
            IDeviceModelMapper<TListItem, TModel> deviceModelMapper,
            IExternalDeviceService externalDeviceService)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceModelRepository = deviceModelRepository;
            this.deviceModelCommandRepository = deviceModelCommandRepository;
            this.labelRepository = labelRepository;
            this.deviceRegistryProvider = deviceRegistryProvider;
            this.configService = configService;
            this.deviceModelImageManager = deviceModelImageManager;
            this.deviceModelMapper = deviceModelMapper;
            this.externalDeviceService = externalDeviceService;
        }

        public async Task<PaginatedResult<DeviceModelDto>> GetDeviceModels(DeviceModelFilter deviceModelFilter)
        {
            var deviceModelPredicate = PredicateBuilder.True<DeviceModel>();

            if (!string.IsNullOrWhiteSpace(deviceModelFilter.SearchText))
            {
                deviceModelPredicate = deviceModelPredicate.And(model => model.Name.ToLower().Contains(deviceModelFilter.SearchText.ToLower()) || model.Description.ToLower().Contains(deviceModelFilter.SearchText.ToLower()));
            }

            var paginatedDeviceModels = await this.deviceModelRepository.GetPaginatedListAsync(deviceModelFilter.PageNumber, deviceModelFilter.PageSize, deviceModelFilter.OrderBy, deviceModelPredicate, includes: new Expression<Func<DeviceModel, object>>[] { d => d.Labels });

            var paginateDeviceModelsDto = new PaginatedResult<DeviceModelDto>
            {
                Data = paginatedDeviceModels.Data.Select(x => this.mapper.Map<DeviceModelDto>(x, opts =>
                {
                    opts.AfterMap((src, dest) => dest.ImageUrl = this.deviceModelImageManager.ComputeImageUri(x.Id));
                })).ToList(),
                TotalCount = paginatedDeviceModels.TotalCount,
                CurrentPage = paginatedDeviceModels.CurrentPage,
                PageSize = deviceModelFilter.PageSize
            };

            return new PaginatedResult<DeviceModelDto>(paginateDeviceModelsDto.Data, paginateDeviceModelsDto.TotalCount, paginateDeviceModelsDto.CurrentPage, paginateDeviceModelsDto.PageSize);
        }

        public async Task<TModel> GetDeviceModel(string deviceModelId)
        {
            var deviceModelEntity = await this.deviceModelRepository.GetByIdAsync(deviceModelId, d => d.Labels);

            if (deviceModelEntity == null)
            {
                throw new ResourceNotFoundException($"The device model {deviceModelId} doesn't exist");
            }

            return this.mapper.Map<TModel>(deviceModelEntity);
        }

        public async Task<TModel> CreateDeviceModel(TModel deviceModel)
        {
            var deviceModelEntity = this.mapper.Map<DeviceModel>(deviceModel);

            await this.deviceModelRepository.InsertAsync(deviceModelEntity);
            await this.unitOfWork.SaveAsync();

            _ = this.deviceModelImageManager.SetDefaultImageToModel(deviceModel.ModelId);

            await CreateDeviceModelConfiguration(deviceModel);

            return deviceModel;
        }

        public async Task UpdateDeviceModel(TModel deviceModel)
        {
            var deviceModelEntity = await this.deviceModelRepository.GetByIdAsync(deviceModel.ModelId, d => d.Labels);

            if (deviceModelEntity == null)
            {
                throw new ResourceNotFoundException($"The device model {deviceModel.ModelId} doesn't exist");
            }

            foreach (var labelEntity in deviceModelEntity.Labels)
            {
                this.labelRepository.Delete(labelEntity.Id);
            }

            _ = this.mapper.Map(deviceModel, deviceModelEntity);

            this.deviceModelRepository.Update(deviceModelEntity);
            await this.unitOfWork.SaveAsync();

            await CreateDeviceModelConfiguration(deviceModel);
        }

        public async Task DeleteDeviceModel(string deviceModelId)
        {
            var deviceModelEntity = await this.deviceModelRepository.GetByIdAsync(deviceModelId, d => d.Labels);

            if (deviceModelEntity == null)
            {
                return;
            }

            var devices = await this.externalDeviceService.GetAllDevice();

            if (devices.Items.Any(x => DeviceHelper.RetrieveTagValue(x, "modelId") == deviceModelId))
            {
                throw new ResourceAlreadyExistsException(
                    $"The device model {deviceModelId} is already in use by a device and cannot be deleted");
            }

            // TODO : Delete DPS and Configurations
            await this.deviceRegistryProvider.DeleteEnrollmentGroupByDeviceModelIdAsync(deviceModelId, default);

            await this.configService.DeleteDeviceModelConfigurationByConfigurationNamePrefix(deviceModelId);

            var deviceModelCommands = this.deviceModelCommandRepository.GetAll().Where(command =>
                    command.DeviceModelId.Equals(deviceModelId, StringComparison.Ordinal)).ToList();

            foreach (var deviceModelCommand in deviceModelCommands)
            {
                this.deviceModelCommandRepository.Delete(deviceModelCommand.Id);
            }

            foreach (var labelEntity in deviceModelEntity.Labels)
            {
                this.labelRepository.Delete(labelEntity.Id);
            }

            // Image deletion
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelId);

            this.deviceModelRepository.Delete(deviceModelId);

            await this.unitOfWork.SaveAsync();
        }

        public Task<string> GetDeviceModelAvatar(string deviceModelId)
        {
            return Task.Run(() => this.deviceModelImageManager.ComputeImageUri(deviceModelId).ToString());
        }

        public Task<string> UpdateDeviceModelAvatar(string deviceModelId, IFormFile file)
        {
            return Task.Run(() => this.deviceModelImageManager.ChangeDeviceModelImageAsync(deviceModelId, file?.OpenReadStream()));
        }

        public Task DeleteDeviceModelAvatar(string deviceModelId)
        {
            return this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelId);
        }

        private async Task CreateDeviceModelConfiguration(TModel deviceModel)
        {
            var desiredProperties = this.deviceModelMapper.BuildDeviceModelDesiredProperties(deviceModel);

            var deviceModelTwin = new TwinCollection();

            _ = await this.deviceRegistryProvider.CreateEnrollmentGroupFromModelAsync(deviceModel.ModelId, deviceModel.Name, deviceModelTwin);

            _ = await this.configService.RollOutDeviceModelConfiguration(deviceModel.ModelId, desiredProperties);
        }
    }
}
