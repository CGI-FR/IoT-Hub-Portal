// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class DeviceModelPropertiesService : IDeviceModelPropertiesService
    {
        /// <summary>
        /// The unit of work.
        /// </summary>
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device model properties repository.
        /// </summary>
        private readonly IDeviceModelPropertiesRepository deviceModelPropertiesRepository;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger log;

        public DeviceModelPropertiesService(
            ILogger<DeviceModelPropertiesService> log, IUnitOfWork unitOfWork, ITableClientFactory tableClientFactory, IDeviceModelPropertiesRepository deviceModelPropertiesRepository)
        {
            this.log = log;
            this.unitOfWork = unitOfWork;
            this.tableClientFactory = tableClientFactory;
            this.deviceModelPropertiesRepository = deviceModelPropertiesRepository;
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
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to set properties for model {modelId}", e);
            }
        }

        private async Task<bool> AssertModelExists(string id)
        {
            try
            {
                _ = await this.tableClientFactory
                                    .GetDeviceTemplates()
                                    .GetEntityAsync<TableEntity>("0", id);

                return true;
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    throw new ResourceNotFoundException($"The model {id} doesn't exist.");
                }

                this.log.LogError(e.Message, e);

                throw new InternalServerErrorException($"Unable to check if device model with id {id} exist", e);
            }
        }
    }
}
