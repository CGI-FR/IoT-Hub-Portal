// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Shared;
    using AzureIoTHub.Portal.Infrastructure.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Models.v1._0;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Http;

    internal class AwsDeviceModelService<TListItem, TModel> : IDeviceModelService<TListItem, TModel>
        where TListItem : class, IDeviceModel
        where TModel : class, IDeviceModel
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IExternalDeviceServiceV2 externalDeviceService;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public AwsDeviceModelService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IExternalDeviceServiceV2 externalDeviceService,
            IDeviceModelImageManager deviceModelImageManager)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
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

            var paginatedDeviceModels = await this.unitOfWork.DeviceModelRepository.GetPaginatedListAsync(deviceModelFilter.PageNumber, deviceModelFilter.PageSize, deviceModelFilter.OrderBy, deviceModelPredicate, includes: new Expression<Func<DeviceModel, object>>[] { d => d.Labels });

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
            var deviceModelEntity = await this.unitOfWork.DeviceModelRepository.GetByIdAsync(deviceModelId, d => d.Labels);

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

            await this.unitOfWork.DeviceModelRepository.InsertAsync(deviceModelEntity);
            await this.unitOfWork.SaveAsync();

            _ = this.deviceModelImageManager.SetDefaultImageToModel(deviceModel.ModelId);

            return deviceModel;
        }

        public async Task UpdateDeviceModel(TModel deviceModel)
        {
            var deviceModelEntity = await this.unitOfWork.DeviceModelRepository.GetByIdAsync(deviceModel.ModelId, d => d.Labels);

            if (deviceModelEntity == null)
            {
                throw new ResourceNotFoundException($"The device model {deviceModel.ModelId} doesn't exist");
            }

            foreach (var labelEntity in deviceModelEntity.Labels)
            {
                this.unitOfWork.LabelRepository.Delete(labelEntity.Id);
            }

            _ = this.mapper.Map(deviceModel, deviceModelEntity);

            this.unitOfWork.DeviceModelRepository.Update(deviceModelEntity);

            await this.unitOfWork.SaveAsync();
        }

        public async Task DeleteDeviceModel(string deviceModelId)
        {
            var deviceModelEntity = await this.unitOfWork.DeviceModelRepository.GetByIdAsync(deviceModelId, d => d.Labels);

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
                this.unitOfWork.LabelRepository.Delete(labelEntity.Id);
            }

            // Image deletion
            await this.deviceModelImageManager.DeleteDeviceModelImageAsync(deviceModelId);

            this.unitOfWork.DeviceModelRepository.Delete(deviceModelId);

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
