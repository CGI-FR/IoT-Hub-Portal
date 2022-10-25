// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using Domain.Exceptions;
    using Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class DeviceModelPropertiesService : IDeviceModelPropertiesService
    {
        /// <summary>
        /// The unit of work.
        /// </summary>
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// The device model properties repository.
        /// </summary>
        private readonly IDeviceModelPropertiesRepository deviceModelPropertiesRepository;

        private readonly IDeviceModelRepository deviceModelRepository;

        public DeviceModelPropertiesService(IUnitOfWork unitOfWork, IDeviceModelPropertiesRepository deviceModelPropertiesRepository, IDeviceModelRepository deviceModelRepository)
        {
            this.unitOfWork = unitOfWork;
            this.deviceModelPropertiesRepository = deviceModelPropertiesRepository;
            this.deviceModelRepository = deviceModelRepository;
        }

        public async Task<IEnumerable<DeviceModelProperty>> GetModelProperties(string modelId)
        {
            _ = await AssertModelExists(modelId);

            return await this.deviceModelPropertiesRepository.GetModelProperties(modelId);
        }

        public async Task SavePropertiesForModel(string modelId, IEnumerable<DeviceModelProperty> items)
        {
            _ = await AssertModelExists(modelId);

            try
            {
                await this.deviceModelPropertiesRepository.SavePropertiesForModel(modelId, items);

                await this.unitOfWork.SaveAsync();
            }
            catch (DbUpdateException e)
            {
                throw new InternalServerErrorException($"Unable to set properties for model {modelId}", e);
            }
        }

        private async Task<bool> AssertModelExists(string deviceModelId)
        {
            var deviceModelEntity = await this.deviceModelRepository.GetByIdAsync(deviceModelId);

            return deviceModelEntity == null
                ? throw new ResourceNotFoundException($"The device model {deviceModelId} doesn't exist")
                : true;
        }
    }
}
