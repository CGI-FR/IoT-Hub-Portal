// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Server.Entities;
    using AzureIoTHub.Portal.Server.Managers;
    using AzureIoTHub.Portal.Server.Mappers;
    using Microsoft.AspNetCore.Http;

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

        /// <summary>
        /// Return the edge model template list.
        /// </summary>
        /// <returns>IEnumerable IoTEdgeModelListItem.</returns>
        /// <exception cref="InternalServerErrorException"></exception>
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

        /// <summary>
        /// Get the edge model template and it's configuration by the edge model id.
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        /// <returns>An edge model object.</returns>
        /// <exception cref="ResourceNotFoundException">Resource not found if template does not exist.</exception>
        /// <exception cref="InternalServerErrorException">Internal server error exception.</exception>
        public async Task<IoTEdgeModel> GetEdgeModel(string modelId)
        {
            try
            {
                var query = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .GetEntityAsync<TableEntity>(DefaultPartitionKey, modelId);
                var modules = await this.configService.GetConfigModuleList(modelId);
                var commands = this.tableClientFactory.GetEdgeModuleCommands()
                    .Query<EdgeModuleCommand>(c => c.PartitionKey == modelId)
                    .ToArray();
                return this.edgeDeviceModelMapper.CreateEdgeDeviceModel(query.Value, modules, commands);
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

        /// <summary>
        /// Create a new edge model template and roll out
        /// the edge model configuration.
        /// </summary>
        /// <param name="edgeModel">the new edge modle object.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="ResourceAlreadyExistsException">If edge model template already exist return ResourceAlreadyExistsException.</exception>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task CreateEdgeModel(IoTEdgeModel edgeModel)
        {
            if (!string.IsNullOrEmpty(edgeModel?.ModelId))
            {
                try
                {
                    _ = await this.tableClientFactory
                                   .GetEdgeDeviceTemplates()
                                   .GetEntityAsync<TableEntity>(DefaultPartitionKey, edgeModel.ModelId);

                    throw new ResourceAlreadyExistsException($"The edge model with id {edgeModel?.ModelId} already exists");
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
                RowKey = string.IsNullOrEmpty(edgeModel?.ModelId) ? Guid.NewGuid().ToString() : edgeModel.ModelId
            };

            await SaveEntity(entity, edgeModel);
        }

        /// <summary>
        /// Update the edge model template and the configuration.
        /// </summary>
        /// <param name="edgeModel">The edge model.</param>
        /// <returns>nothing.</returns>
        /// <exception cref="RequestFailedException"></exception>
        /// <exception cref="InternalServerErrorException"></exception>
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

        /// <summary>
        /// Delete edge model template and it's configuration.
        /// </summary>
        /// <param name="edgeModelId">The edge model indentifier.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task DeleteEdgeModel(string edgeModelId)
        {
            try
            {
                _ = await this.tableClientFactory
                    .GetEdgeDeviceTemplates()
                    .DeleteEntityAsync(DefaultPartitionKey, edgeModelId);

                var config = this.configService.GetIoTEdgeConfigurations().Result.FirstOrDefault(x => x.Id.StartsWith(edgeModelId, StringComparison.Ordinal));

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

        /// <summary>
        /// Get the edge model avatar.
        /// </summary>
        /// <param name="edgeModelId">The edge model indentifier.</param>
        /// <returns></returns>
        /// <exception cref="ResourceNotFoundException"></exception>
        /// <exception cref="InternalServerErrorException"></exception>
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

        /// <summary>
        /// Update the edge model avatar.
        /// </summary>
        /// <param name="edgeModelId">The edge model indentifier</param>
        /// <param name="file">The image.</param>
        /// <returns></returns>
        /// <exception cref="ResourceNotFoundException"></exception>
        /// <exception cref="InternalServerErrorException"></exception>
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

        /// <summary>
        /// Delete the edge model avatar.
        /// </summary>
        /// <param name="edgeModelId">The edge model indentifier</param>
        /// <returns></returns>
        /// <exception cref="ResourceNotFoundException"></exception>
        /// <exception cref="InternalServerErrorException"></exception>
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
        /// Saves the entity and roll out the configuration.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="deviceModelObject">The device model object.</param>
        private async Task SaveEntity(TableEntity entity, IoTEdgeModel deviceModelObject)
        {
            this.edgeDeviceModelMapper.UpdateTableEntity(entity, deviceModelObject);

            await SaveModuleCommands(deviceModelObject);

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

        /// <summary>
        /// Saves the module commands for a specific model object.
        /// </summary>
        /// <param name="deviceModelObject">The device model object.</param>
        /// <returns></returns>
        /// <exception cref="InternalServerErrorException"></exception>
        public async Task SaveModuleCommands(IoTEdgeModel deviceModelObject)
        {
            IEnumerable<EdgeModuleCommand> moduleCommands = deviceModelObject.EdgeModules
                .SelectMany(x => x.Commands.Select(cmd => new EdgeModuleCommand
                {
                    PartitionKey = deviceModelObject.ModelId,
                    RowKey = x.ModuleName + "-" + cmd.Name,
                    Timestamp = DateTime.Now,
                    Name = cmd.Name,
                })).ToArray();

            try
            {
                var existingCommands = this.tableClientFactory.GetEdgeModuleCommands()
                                                .Query<EdgeModuleCommand>(c => c.PartitionKey == deviceModelObject.ModelId)
                                                .ToArray();
                foreach (var command in existingCommands.Where(c => !moduleCommands.Any(x => x.RowKey == c.RowKey)))
                {
                    _ = await this.tableClientFactory.GetEdgeModuleCommands().DeleteEntityAsync(command.PartitionKey, command.RowKey);
                }
                foreach (var moduleCommand in moduleCommands)
                {
                    _ = this.tableClientFactory
                        .GetEdgeModuleCommands()
                        .UpsertEntity(moduleCommand);
                }
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException("Unable to save device module commands", e);
            }
        }
    }
}
