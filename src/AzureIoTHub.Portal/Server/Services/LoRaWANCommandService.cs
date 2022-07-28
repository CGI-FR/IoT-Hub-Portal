// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Data.Tables;
    using AzureIoTHub.Portal.Models.v10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Controllers.V10.LoRaWAN;
    using AzureIoTHub.Portal.Server.Exceptions;
    using AzureIoTHub.Portal.Server.Factories;
    using AzureIoTHub.Portal.Server.Mappers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class LoRaWANCommandService : ILoRaWANCommandService
    {
        /// <summary>
        /// The table client factory.
        /// </summary>
        private readonly ITableClientFactory tableClientFactory;

        /// <summary>
        /// The device model command mapper.
        /// </summary>H
        private readonly IDeviceModelCommandMapper deviceModelCommandMapper;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<LoRaWANCommandService> log;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaWANCommandService"/> class.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="deviceModelCommandMapper">The device model command mapper.</param>
        /// <param name="tableClientFactory">The table client factory.</param>
        public LoRaWANCommandService(
            ILogger<LoRaWANCommandService> log,
            IDeviceModelCommandMapper deviceModelCommandMapper,
            ITableClientFactory tableClientFactory)
        {
            this.log = log;
            this.tableClientFactory = tableClientFactory;
            this.deviceModelCommandMapper = deviceModelCommandMapper;
        }

        public async Task PostDeviceModelCommands(string id, DeviceModelCommand[] commands)
        {
            var query = await GetCommandsQueryFromModelId(id);

            foreach (var item in query)
            {
                try
                {
                    _ = await this.tableClientFactory
                        .GetDeviceCommands()
                        .DeleteEntityAsync(item.PartitionKey, item.RowKey);
                }
                catch (RequestFailedException e)
                {
                    throw new InternalServerErrorException($"Unable to delete the command {item.RowKey} of the model with id {id}", e);
                }
            }

            foreach (var command in commands)
            {
                var entity = new TableEntity()
                {
                    PartitionKey = id,
                    RowKey = command.Name
                };

                this.deviceModelCommandMapper.UpdateTableEntity(entity, command);

                try
                {
                    _ = await this.tableClientFactory
                        .GetDeviceCommands()
                        .AddEntityAsync(entity);
                }
                catch (RequestFailedException e)
                {
                    throw new InternalServerErrorException($"Unable to create the command {command.Name} of the model with id {id}", e);
                }
            }
        }
        public async Task<DeviceModelCommand[]> GetDeviceModelCommandsFromModel(string id)
        {
            var query = await GetCommandsQueryFromModelId(id);

            var commands = new List<DeviceModelCommand>();

            foreach (var item in query)
            {
                var command = this.deviceModelCommandMapper.GetDeviceModelCommand(item);
                commands.Add(command);
            }

            return commands.ToArray();
        }

        private async Task<Pageable<TableEntity>> GetCommandsQueryFromModelId(string id)
        {
            try
            {
                _ = await this.tableClientFactory
                             .GetDeviceTemplates()
                             .GetEntityAsync<TableEntity>(LoRaWANDeviceModelsController.DefaultPartitionKey, id);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == StatusCodes.Status404NotFound)
                {
                    throw new InternalServerErrorException($"Unable to find a model with id {id}.", e);
                }

                this.log.LogError(e.Message, e);

                throw;
            }

            Pageable<TableEntity> query;

            try
            {
                query = this.tableClientFactory
                    .GetDeviceCommands()
                    .Query<TableEntity>(filter: $"PartitionKey eq '{id}'");
            }
            catch (RequestFailedException e)
            {
                throw new InternalServerErrorException($"Unable to get existing commands of the model with id {id}", e);
            }

            return query;
        }
    }
}
