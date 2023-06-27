// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Azure.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Domain.Entities;
    using Domain;
    using Domain.Exceptions;
    using Domain.Repositories;

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

            return await deviceModelPropertiesRepository.GetModelProperties(modelId);
        }

        public async Task SavePropertiesForModel(string modelId, IEnumerable<DeviceModelProperty> items)
        {
            _ = await AssertModelExists(modelId);

            await deviceModelPropertiesRepository.SavePropertiesForModel(modelId, items);

            await unitOfWork.SaveAsync();
        }

        private async Task<bool> AssertModelExists(string deviceModelId)
        {
            var deviceModelEntity = await deviceModelRepository.GetByIdAsync(deviceModelId);

            if (deviceModelEntity == null)
                throw new ResourceNotFoundException($"The device model {deviceModelId} doesn't exist");

            return true;
        }

        public IEnumerable<string> GetAllPropertiesNames()
        {
            return deviceModelPropertiesRepository
                .GetAll()
                .Select(property => property.Name)
                .Distinct()
                .ToList();
        }
    }
}
