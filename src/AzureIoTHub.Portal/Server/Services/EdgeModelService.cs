// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using Azure.Data.Tables;
    using Azure;
    using AzureIoTHub.Portal.Models.v10;
    using System.Collections.Generic;
    using System.Linq;
    using AzureIoTHub.Portal.Server.Exceptions;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;
    using System;

    public class EdgeModelService : IEdgeModelService
    {
        /// <summary>
        /// The default partition key.
        /// </summary>
        public const string DefaultPartitionKey = "0";

        /// <summary>
        /// The Configurations service.
        /// </summary>
        private readonly IConfigService configService;

        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device model mapper.
        /// </summary>
        private readonly IEdgeDeviceModelMapper edgeDeviceModelMapper;

        /// <summary>
        /// The device model image manager.
        /// </summary>
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public EdgeModelService(IConfigService configService,
            ITableClientFactory tableClientFactory,
            IEdgeDeviceModelMapper edgeDeviceModelMapper,
            IDeviceModelImageManager deviceModelImageManager)
        {
            this.configService = configService;
            this.tableClientFactory = tableClientFactory;
            this.edgeDeviceModelMapper = edgeDeviceModelMapper;
            this.deviceModelImageManager = deviceModelImageManager;
        }

        public IEnumerable<IoTEdgeModelListItem> GetEdgeModels()
        {
            try
            {
                return this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .Query<TableEntity>()
                    .Select(this.edgeDeviceModelMapper.CreateEdgeDeviceModelListItem);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to get edge models.", e);
            }
        }

        public async Task<IoTEdgeModel> GetEdgeModel(string modelId)
        {
            try
            {
                var query = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .GetEntityAsync<TableEntity>(DefaultPartitionKey, modelId);

                var modules = await this.configService.GetConfigModuleList(modelId);

                return this.edgeDeviceModelMapper.CreateEdgeDeviceModel(query.Value, modules);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    throw new ResourceNotFoundException($"The edge model with id {modelId} doesn't exist");
                }

                throw new InternalServerErrorException($"Unable to get the edge model with id {modelId}", e);
            }
        }

        public async Task CreateEdgeModel(IoTEdgeModel edgeModel)
        {
            if (!string.IsNullOrEmpty(edgeModel?.ModelId))
            {
                try
                {
                    _ = await this.tableClientFactory
                                   .GetEdgeDeviceTemplates()
                                   .GetEntityAsync<TableEntity>(DefaultPartitionKey, edgeModel.ModelId);

                    throw new ResourceAlreadyExistsException($"The edge model with id {edgeModel.ModelId} already exists");
                }
                catch (RequestFailedException e)
                {
                    if (e.Status != StatusCodes.Status404NotFound)
                    {
                        throw new InternalServerErrorException($"Unable create the device model with id: {edgeModel?.ModelId}.", e);
                    }
                }
            }

            var entity = new TableEntity()
            {
                PartitionKey = DefaultPartitionKey,
                RowKey = string.IsNullOrEmpty(edgeModel.ModelId) ? Guid.NewGuid().ToString() : edgeModel.ModelId
            };

            await SaveEntity(entity, edgeModel);
        }

        public async Task UpdateEdgeModel(IoTEdgeModel edgeModel)
        {
            if (string.IsNullOrEmpty(edgeModel?.ModelId))
            {
                throw new RequestFailedException("edge model id is null.");
            }

            try
            {
                var entity = await this.tableClientFactory
                                   .GetEdgeDeviceTemplates()
                                   .GetEntityAsync<TableEntity>(DefaultPartitionKey, edgeModel.ModelId);

                await SaveEntity(entity, edgeModel);

            }
            catch (RequestFailedException e)
            {
                if (e.Status != StatusCodes.Status404NotFound)
                {
                    throw new InternalServerErrorException($"Unable create the device model with id: {edgeModel?.ModelId}.", e);
                }
            }
        }

        public async Task DeleteEdgeModel(string edgeModelId)
        {
            try
            {
                _ = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .DeleteEntityAsync(DefaultPartitionKey, edgeModelId);

                var config = this.configService.GetIoTEdgeConfigurations().Result.FirstOrDefault(x => x.Id.StartsWith(edgeModelId));

                if (config != null)
                {
                    await this.configService.DeleteConfiguration(config.Id);
                }
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to delete the edge model with {edgeModelId} model entity.", e);
            }
        }

        public async Task<string> GetEdgeModelAvatar(string edgeModelId)
        {
            try
            {
                _ = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .GetEntityAsync<TableEntity>(DefaultPartitionKey, edgeModelId);

                return this.deviceModelImageManager.ComputeImageUri(edgeModelId).ToString();
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    throw new ResourceNotFoundException($"The avatar of the edge model with id {edgeModelId} doesn't exist");
                }

                throw new InternalServerErrorException($"Unable to get the edge model avatar with id {edgeModelId}", e);
            }
        }

        public async Task<string> UpdateEdgeModelAvatar(string edgeModelId, IFormFile file)
        {
            try
            {
                _ = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .GetEntityAsync<TableEntity>(DefaultPartitionKey, edgeModelId);

                return await this.deviceModelImageManager.ChangeDeviceModelImageAsync(edgeModelId, file?.OpenReadStream());
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    throw new ResourceNotFoundException($"The avatar of the edge model with id {edgeModelId} doesn't exist");
                }

                throw new InternalServerErrorException($"Unable to update the edge model avatar with id {edgeModelId}.", e);
            }
        }

        public async Task DeleteEdgeModelAvatar(string edgeModelId)
        {
            try
            {
                _ = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .GetEntityAsync<TableEntity>(DefaultPartitionKey, edgeModelId);

                await this.deviceModelImageManager.DeleteDeviceModelImageAsync(edgeModelId);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    throw new ResourceNotFoundException($"The avatar of the edge model with id {edgeModelId} doesn't exist");
                }

                throw new InternalServerErrorException($"Unable to delete the edge model avatar with id {edgeModelId}.", e);
            }
        }

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="deviceModelObject">The device model object.</param>
        private async Task SaveEntity(TableEntity entity, IoTEdgeModel deviceModelObject)
        {
            this.edgeDeviceModelMapper.UpdateTableEntity(entity, deviceModelObject);

            try
            {
                _ = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .UpsertEntityAsync(entity);
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to upsert the edge device model entity.", e);
            }

            await this.configService.RollOutEdgeModelConfiguration(deviceModelObject);
        }
    }
}
