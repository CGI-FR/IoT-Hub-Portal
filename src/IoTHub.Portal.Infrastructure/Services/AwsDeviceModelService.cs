// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Services
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain;
    using IoTHub.Portal.Domain.Entities;
    using IoTHub.Portal.Domain.Exceptions;
    using IoTHub.Portal.Domain.Repositories;
    using IoTHub.Portal.Domain.Shared;
    using IoTHub.Portal.Infrastructure.Repositories;
    using IoTHub.Portal.Shared.Models;
    using IoTHub.Portal.Shared.Models.v1._0;
    using Microsoft.AspNetCore.Http;
    using Shared.Models.v1._0.Filters;

    internal class AwsDeviceModelService<TListItem, TModel> : IDeviceModelService<TListItem, TModel>
        where TListItem : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IDeviceModelRepository deviceModelRepository;
        private readonly ILabelRepository labelRepository;
        private readonly IExternalDeviceService externalDeviceService;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public AwsDeviceModelService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IDeviceModelRepository deviceModelRepository,
            ILabelRepository labelRepository,
            IExternalDeviceService externalDeviceService,
            IDeviceModelImageManager deviceModelImageManager)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.deviceModelRepository = deviceModelRepository;
            this.labelRepository = labelRepository;
            this.externalDeviceService = externalDeviceService;
            this.deviceModelImageManager = deviceModelImageManager;
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
            var externalDeviceModel = await this.externalDeviceService.CreateDeviceModel(this.mapper.Map<ExternalDeviceModelDto>(deviceModel));

            deviceModel.ModelId = externalDeviceModel.Id;

            var deviceModelEntity = this.mapper.Map<DeviceModel>(deviceModel);

            await this.deviceModelRepository.InsertAsync(deviceModelEntity);
            await this.unitOfWork.SaveAsync();

            _ = this.deviceModelImageManager.SetDefaultImageToModel(deviceModel.ModelId);

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
        }

        public async Task DeleteDeviceModel(string deviceModelId)
        {
            var deviceModelEntity = await this.deviceModelRepository.GetByIdAsync(deviceModelId, d => d.Labels);

            if (deviceModelEntity == null)
            {
                return;
            }

            await this.externalDeviceService.DeleteDeviceModel(new ExternalDeviceModelDto
            {
                Name = deviceModelEntity.Name,
            });

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
    }
}
